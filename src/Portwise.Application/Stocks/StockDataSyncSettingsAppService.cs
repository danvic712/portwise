using System.Globalization;
using System.Text.Json;
using Portwise.Application.Exceptions;
using Portwise.Application.Stocks.Contracts;
using Portwise.Application.Stocks.Dtos;
using Portwise.Domain.Contracts;
using Portwise.Domain.Exceptions;
using Portwise.Domain.Models;

namespace Portwise.Application.Stocks;

public sealed class StockDataSyncSettingsAppService(
    IUow uow,
    TimeProvider timeProvider) : IStockDataSyncSettingsAppService
{
    private const int MaxRunTimes = 8;

    public async Task<StockDataSyncSettingsResponse> GetAsync(
        CancellationToken cancellationToken)
    {
        var settings = await uow.Get<StockDataSyncSettings>()
            .SingleOrDefaultAsync(cancellationToken);
        return settings is null
            ? throw ApplicationErrors.Simple(ApplicationErrorCodes.StockDataSyncValidationFailed)
            : ToResponse(settings);
    }

    public async Task<StockDataSyncSettingsResponse> UpdateAsync(
        UpdateStockDataSyncSettingsRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!TryNormalize(request, out var timeZoneId, out var runTimes, out var error))
        {
            throw ApplicationErrors.Validation(
                ApplicationErrorCodes.StockDataSyncValidationFailed,
                error);
        }

        var settings = await uow.Get<StockDataSyncSettings>()
            .SingleOrDefaultAsync(cancellationToken, asNoTracking: false);
        if (settings is null)
        {
            throw ApplicationErrors.Simple(ApplicationErrorCodes.StockDataSyncValidationFailed);
        }

        if (settings.Revision != request.ExpectedRevision)
        {
            throw ApplicationErrors.Simple(ApplicationErrorCodes.StockDataSyncSettingsRevisionConflict);
        }

        settings.Update(
            request.Enabled,
            timeZoneId,
            JsonSerializer.Serialize(runTimes),
            timeProvider.GetUtcNow());
        try
        {
            await uow.CommitAsync(cancellationToken);
        }
        catch (UnitOfWorkCommitException exception) when (exception.IsConcurrencyConflict)
        {
            throw ApplicationErrors.Simple(
                ApplicationErrorCodes.StockDataSyncSettingsRevisionConflict,
                exception);
        }

        return ToResponse(settings);
    }

    private StockDataSyncSettingsResponse ToResponse(StockDataSyncSettings settings)
    {
        var runTimes = ParseStoredRunTimes(settings.RunTimesJson);
        DateTimeOffset? nextRun = settings.Enabled
            ? DailySyncSchedule.GetNextRunUtc(
                timeProvider.GetUtcNow(),
                runTimes,
                FindTimeZone(settings.TimeZoneId))
            : null;
        return new StockDataSyncSettingsResponse(
            settings.Enabled,
            settings.TimeZoneId,
            runTimes.Select(time => time.ToString("HH:mm", CultureInfo.InvariantCulture)).ToArray(),
            settings.Revision,
            settings.UpdatedAtUtc,
            nextRun);
    }

    private static bool TryNormalize(
        UpdateStockDataSyncSettingsRequest request,
        out string timeZoneId,
        out IReadOnlyList<string> runTimes,
        out string error)
    {
        timeZoneId = request.TimeZoneId?.Trim() ?? string.Empty;
        runTimes = [];
        error = "Stock synchronization schedule is invalid.";
        if (request.ExpectedRevision < 1 || string.IsNullOrWhiteSpace(timeZoneId))
        {
            return false;
        }

        try
        {
            _ = FindTimeZone(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            error = "The selected time zone is not available.";
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            error = "The selected time zone is invalid.";
            return false;
        }

        if (request.RunTimes is null || request.RunTimes.Count is < 1 or > MaxRunTimes)
        {
            error = $"Choose between one and {MaxRunTimes} daily run times.";
            return false;
        }

        var parsed = new List<TimeOnly>(request.RunTimes.Count);
        foreach (var value in request.RunTimes)
        {
            if (!TimeOnly.TryParseExact(
                    value?.Trim(),
                    "HH:mm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var time))
            {
                error = "Run times must use the HH:mm format.";
                return false;
            }

            parsed.Add(time);
        }

        if (parsed.Distinct().Count() != parsed.Count)
        {
            error = "Each daily run time can only appear once.";
            return false;
        }

        runTimes = parsed
            .Order()
            .Select(time => time.ToString("HH:mm", CultureInfo.InvariantCulture))
            .ToArray();
        return true;
    }

    private static IReadOnlyList<TimeOnly> ParseStoredRunTimes(string json)
    {
        var values = JsonSerializer.Deserialize<string[]>(json) ?? [];
        var result = new List<TimeOnly>(values.Length);
        foreach (var value in values)
        {
            if (TimeOnly.TryParseExact(
                    value,
                    "HH:mm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var time))
            {
                result.Add(time);
            }
        }

        return result.Count == 0 ? [new TimeOnly(18, 0)] : result.Order().ToArray();
    }

    private static TimeZoneInfo FindTimeZone(string timeZoneId)
        => TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
}
