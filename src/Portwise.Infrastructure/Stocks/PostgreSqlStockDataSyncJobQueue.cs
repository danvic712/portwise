using System.Text.Json;
using Npgsql;
using NpgsqlTypes;
using Portwise.Application.Stocks.Contracts;
using Portwise.Application.Stocks.Dtos;

namespace Portwise.Infrastructure.Stocks;

internal sealed class PostgreSqlStockDataSyncJobQueue(
    NpgsqlDataSource dataSource,
    TimeProvider timeProvider) : IStockDataSyncJobQueue
{
    private const int MaxAttempts = 3;

    public async Task<StockDataSyncJobResponse> EnqueueAsync(
        string triggerCode,
        string? deduplicationKey,
        CancellationToken cancellationToken)
    {
        var id = Guid.CreateVersion7();
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using (var command = new NpgsqlCommand(
            """
            INSERT INTO public.stock_data_sync_jobs
                (id, trigger_code, status_code, deduplication_key, created_at_utc,
                 available_at_utc, attempt_count)
            VALUES (@id, @trigger, 'pending', @key, @now, @now, 0)
            ON CONFLICT (deduplication_key) DO NOTHING
            RETURNING id
            """, connection))
        {
            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("trigger", triggerCode);
            command.Parameters.AddWithValue("key", (object?)deduplicationKey ?? DBNull.Value);
            command.Parameters.AddWithValue("now", timeProvider.GetUtcNow());
            var insertedId = await command.ExecuteScalarAsync(cancellationToken);
            if (insertedId is Guid persistedId)
            {
                id = persistedId;
            }
            else if (deduplicationKey is not null)
            {
                await using var existing = new NpgsqlCommand(
                    "SELECT id FROM public.stock_data_sync_jobs WHERE deduplication_key = @key",
                    connection);
                existing.Parameters.AddWithValue("key", deduplicationKey);
                id = (Guid)(await existing.ExecuteScalarAsync(cancellationToken)
                    ?? throw new InvalidOperationException("The deduplicated sync job was not found."));
            }
        }

        return await GetAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("The newly queued sync job was not found.");
    }

    public async Task<StockDataSyncJobResponse?> GetAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(
            """
            SELECT id, trigger_code, status_code, attempt_count, created_at_utc,
                   started_at_utc, completed_at_utc, result_json::text, error_code
            FROM public.stock_data_sync_jobs WHERE id = @id
            """, connection);
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var result = reader.IsDBNull(7)
            ? null
            : JsonSerializer.Deserialize<StockDataSyncRunResult>(reader.GetString(7));
        return new StockDataSyncJobResponse(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetInt32(3),
            reader.GetFieldValue<DateTimeOffset>(4),
            reader.IsDBNull(5) ? null : reader.GetFieldValue<DateTimeOffset>(5),
            reader.IsDBNull(6) ? null : reader.GetFieldValue<DateTimeOffset>(6),
            result,
            reader.IsDBNull(8) ? null : reader.GetString(8));
    }

    public async Task<StockDataSyncJobLease?> TryClaimAsync(
        Guid ownerId,
        CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using (var gate = new NpgsqlCommand(
            "SELECT pg_advisory_xact_lock(744266818)", connection, transaction))
        {
            await gate.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var expire = new NpgsqlCommand(
            """
            UPDATE public.stock_data_sync_jobs
            SET status_code = 'failed', completed_at_utc = clock_timestamp(),
                lease_owner_id = NULL, lease_expires_at_utc = NULL,
                error_code = 'stock_sync_lease_expired'
            WHERE status_code = 'running'
              AND lease_expires_at_utc <= clock_timestamp()
              AND attempt_count >= @max_attempts
            """, connection, transaction))
        {
            expire.Parameters.AddWithValue("max_attempts", MaxAttempts);
            await expire.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var claim = new NpgsqlCommand(
            """
            WITH candidate AS (
                SELECT id FROM public.stock_data_sync_jobs
                WHERE ((status_code = 'pending' AND available_at_utc <= clock_timestamp())
                    OR (status_code = 'running' AND lease_expires_at_utc <= clock_timestamp()
                        AND attempt_count < @max_attempts))
                  AND NOT EXISTS (
                      SELECT 1 FROM public.stock_data_sync_jobs
                      WHERE status_code = 'running'
                        AND lease_expires_at_utc > clock_timestamp())
                ORDER BY created_at_utc, id
                FOR UPDATE SKIP LOCKED
                LIMIT 1
            )
            UPDATE public.stock_data_sync_jobs AS job
            SET status_code = 'running', attempt_count = job.attempt_count + 1,
                lease_owner_id = @owner, started_at_utc = clock_timestamp(),
                lease_expires_at_utc = clock_timestamp() + interval '2 minutes',
                error_code = NULL
            FROM candidate
            WHERE job.id = candidate.id
            RETURNING job.id, job.trigger_code, job.attempt_count
            """, connection, transaction);
        claim.Parameters.AddWithValue("max_attempts", MaxAttempts);
        claim.Parameters.AddWithValue("owner", ownerId);
        StockDataSyncJobLease? lease;
        await using (var reader = await claim.ExecuteReaderAsync(cancellationToken))
        {
            lease = await reader.ReadAsync(cancellationToken)
                ? new StockDataSyncJobLease(
                    reader.GetGuid(0), reader.GetString(1), reader.GetInt32(2))
                : null;
        }

        await transaction.CommitAsync(cancellationToken);
        return lease;
    }

    public async Task<bool> RenewLeaseAsync(
        Guid id,
        Guid ownerId,
        CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(
            """
            UPDATE public.stock_data_sync_jobs
            SET lease_expires_at_utc = clock_timestamp() + interval '2 minutes'
            WHERE id = @id AND lease_owner_id = @owner AND status_code = 'running'
            """, connection);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("owner", ownerId);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task CompleteAsync(
        Guid id,
        Guid ownerId,
        StockDataSyncRunResult result,
        CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(
            """
            UPDATE public.stock_data_sync_jobs
            SET status_code = @status, result_json = @result,
                completed_at_utc = clock_timestamp(), lease_owner_id = NULL,
                lease_expires_at_utc = NULL
            WHERE id = @id AND lease_owner_id = @owner AND status_code = 'running'
            """, connection);
        command.Parameters.AddWithValue("status",
            result.PartiallyFailedStockCount > 0 ? "completed_with_failures" : "completed");
        command.Parameters.Add("result", NpgsqlDbType.Jsonb).Value =
            JsonSerializer.Serialize(result);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("owner", ownerId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
        {
            throw new InvalidOperationException("The stock sync job lease was lost.");
        }
    }

    public async Task FailAsync(
        Guid id,
        Guid ownerId,
        int attemptCount,
        string errorCode,
        CancellationToken cancellationToken)
    {
        var exhausted = attemptCount >= MaxAttempts;
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(
            """
            UPDATE public.stock_data_sync_jobs
            SET status_code = @status, error_code = @error,
                available_at_utc = clock_timestamp() + (@delay_seconds * interval '1 second'),
                completed_at_utc = CASE WHEN @exhausted THEN clock_timestamp() ELSE NULL END,
                lease_owner_id = NULL, lease_expires_at_utc = NULL
            WHERE id = @id AND lease_owner_id = @owner AND status_code = 'running'
            """, connection);
        command.Parameters.AddWithValue("status", exhausted ? "failed" : "pending");
        command.Parameters.AddWithValue("error", errorCode);
        command.Parameters.AddWithValue("delay_seconds", attemptCount == 1 ? 30 : 120);
        command.Parameters.AddWithValue("exhausted", exhausted);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("owner", ownerId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
