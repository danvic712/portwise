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

    public Task<StockDataSyncJobResponse> EnqueueStockAsync(
        string triggerCode,
        Guid batchId,
        Guid securityId,
        string? deduplicationKey,
        CancellationToken cancellationToken)
        => EnqueueInternalAsync(
            triggerCode,
            batchId,
            securityId,
            deduplicationKey,
            cancellationToken);

    private async Task<StockDataSyncJobResponse> EnqueueInternalAsync(
        string triggerCode,
        Guid batchId,
        Guid securityId,
        string? deduplicationKey,
        CancellationToken cancellationToken)
    {
        var id = Guid.CreateVersion7();
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using (var command = new NpgsqlCommand(
            """
            INSERT INTO public.stock_data_sync_jobs
                (id, trigger_code, status_code, deduplication_key, batch_id, security_id,
                 created_at_utc, available_at_utc, attempt_count)
            VALUES (@id, @trigger, 'pending', @key, @batch_id, @security_id,
                    @now, @now, 0)
            ON CONFLICT (deduplication_key) DO NOTHING
            RETURNING id
            """, connection))
        {
            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("trigger", triggerCode);
            command.Parameters.AddWithValue("key", (object?)deduplicationKey ?? DBNull.Value);
            command.Parameters.AddWithValue("batch_id", batchId);
            command.Parameters.AddWithValue("security_id", securityId);
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
            SELECT job.id, job.trigger_code, job.status_code, job.attempt_count,
                   job.created_at_utc, job.started_at_utc, job.completed_at_utc,
                   job.result_json::text, job.error_code, job.batch_id, job.security_id,
                   security.security_code, security.exchange_code
            FROM public.stock_data_sync_jobs AS job
            JOIN public.securities AS security ON security.security_id = job.security_id
            WHERE job.id = @id
            """, connection);
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadJob(reader) : null;
    }

    public async Task<StockDataSyncBatchResponse?> GetBatchAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(
            """
            SELECT job.id, job.trigger_code, job.status_code, job.attempt_count,
                   job.created_at_utc, job.started_at_utc, job.completed_at_utc,
                   job.result_json::text, job.error_code, job.batch_id, job.security_id,
                   security.security_code, security.exchange_code
            FROM public.stock_data_sync_jobs AS job
            JOIN public.securities AS security ON security.security_id = job.security_id
            WHERE job.batch_id = @batch_id
            ORDER BY job.created_at_utc, job.id
            """, connection);
        command.Parameters.AddWithValue("batch_id", id);

        var jobs = new List<StockDataSyncJobResponse>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            jobs.Add(ReadJob(reader));
        }

        if (jobs.Count == 0)
        {
            return null;
        }

        var pending = jobs.Count(job => job.StatusCode == "pending");
        var running = jobs.Count(job => job.StatusCode == "running");
        var completed = jobs.Count(job => job.StatusCode == "completed");
        var failed = jobs.Count(job => job.StatusCode is "failed" or "completed_with_failures"
            || job.Result?.Failures.Count > 0);
        var status = pending > 0 || running > 0
            ? running > 0 ? "running" : "pending"
            : failed > 0 ? "completed_with_failures" : "completed";
        var terminal = completed + failed;
        var completedAt = terminal == jobs.Count
            ? jobs.Max(job => job.CompletedAtUtc)
            : null;
        return new StockDataSyncBatchResponse(
            id,
            jobs[0].TriggerCode,
            status,
            jobs.Count,
            pending,
            running,
            completed,
            failed,
            jobs.Min(job => job.CreatedAtUtc),
            completedAt,
            jobs);
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
            RETURNING job.id, job.trigger_code, job.attempt_count, job.batch_id,
                      job.security_id,
                      (SELECT security_code FROM public.securities WHERE security_id = job.security_id),
                      (SELECT exchange_code FROM public.securities WHERE security_id = job.security_id)
            """, connection, transaction);
        claim.Parameters.AddWithValue("max_attempts", MaxAttempts);
        claim.Parameters.AddWithValue("owner", ownerId);
        StockDataSyncJobLease? lease;
        await using (var reader = await claim.ExecuteReaderAsync(cancellationToken))
        {
            lease = await reader.ReadAsync(cancellationToken)
                ? new StockDataSyncJobLease(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetInt32(2),
                    reader.GetGuid(3),
                    reader.GetGuid(4),
                    reader.GetString(5),
                    reader.GetString(6))
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
        StockFactSyncResult result,
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
        command.Parameters.AddWithValue(
            "status",
            result.Failures.Count > 0 ? "completed_with_failures" : "completed");
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

    private static StockDataSyncJobResponse ReadJob(NpgsqlDataReader reader)
    {
        var batchId = reader.GetGuid(9);
        var securityId = reader.GetGuid(10);
        StockFactSyncResult? result = reader.IsDBNull(7)
            ? null
            : JsonSerializer.Deserialize<StockFactSyncResult>(reader.GetString(7));

        return new StockDataSyncJobResponse(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetInt32(3),
            reader.GetFieldValue<DateTimeOffset>(4),
            reader.IsDBNull(5) ? null : reader.GetFieldValue<DateTimeOffset>(5),
            reader.IsDBNull(6) ? null : reader.GetFieldValue<DateTimeOffset>(6),
            result,
            reader.IsDBNull(8) ? null : reader.GetString(8),
            batchId,
            securityId,
            reader.GetString(11),
            reader.GetString(12));
    }
}
