using System.Globalization;
using Portwise.Application.Stocks.Dtos;
using Portwise.Domain.Codes;
using Portwise.Domain.Securities;

namespace Portwise.Infrastructure.FtShare;

internal static class FtSharePayloadNormalizer
{
    internal static readonly IReadOnlySet<string> ProfileLeafPropertyNames =
        CreatePropertySet(
            "security_name",
            "stock_name",
            "company_name",
            "name");

    internal static readonly IReadOnlySet<string> MarketLeafPropertyNames =
        CreatePropertySet(
            "close_price",
            "closing_price",
            "last_price",
            "price",
            "trading_date",
            "data_as_of_date");

    internal static readonly IReadOnlySet<string> DividendLeafPropertyNames =
        CreatePropertySet(
            "dividend_per_share",
            "cash_dividend_per_share",
            "amount_per_share",
            "dividend_amount");

    internal static readonly IReadOnlySet<string> FinancialLeafPropertyNames =
        CreatePropertySet(
            "data_as_of_date",
            "financial_date",
            "period_end_date",
            "earnings_per_share",
            "dividend_payout_ratio");

    internal static readonly IReadOnlyList<string> ProfileEnvelopeNames =
        ["data", "result", "profile", "security"];

    internal static readonly IReadOnlyList<string> MarketEnvelopeNames =
        ["data", "result", "market", "quote"];

    internal static readonly IReadOnlyList<string> DividendEnvelopeNames =
        ["data", "result", "dividends", "events", "items"];

    internal static readonly IReadOnlyList<string> FinancialEnvelopeNames =
        ["data", "result", "financials", "snapshots", "items"];

    public static StockData? NormalizeProfile(
        AShareReference reference,
        FtShareProfileWire? profile)
    {
        if (profile is null)
        {
            return null;
        }

        var returnedCode = First(
            profile.SecurityCode,
            profile.StockCode,
            profile.Code,
            profile.Symbol);
        if (returnedCode is not null
            && NormalizeSecurityCode(returnedCode) != reference.SecurityCode)
        {
            return null;
        }

        var returnedExchange = First(profile.ExchangeCode, profile.Exchange);
        if (returnedExchange is not null
            && NormalizeExchangeCode(returnedExchange) != reference.ExchangeCode)
        {
            return null;
        }

        var securityName = First(
            profile.SecurityName,
            profile.StockName,
            profile.CompanyName,
            profile.Name);
        var marketCode = NormalizeMarketCode(First(
            profile.MarketCode,
            profile.Market,
            profile.MarketType));
        var currencyCode = NormalizeCurrencyCode(First(
            profile.CurrencyCode,
            profile.Currency));
        var sectorCode = First(
            profile.SectorCode,
            profile.IndustryCode,
            profile.Sector,
            profile.Industry);

        return securityName is null || marketCode is null || currencyCode is null
            ? null
            : new StockData(
                reference.SecurityCode,
                reference.ExchangeCode,
                securityName,
                marketCode,
                currencyCode,
                sectorCode);
    }

    public static StockMarketData? NormalizeMarket(
        AShareReference reference,
        FtShareMarketWire? market)
    {
        if (market is null)
        {
            return null;
        }

        if (!MatchesReference(reference, market.SecurityCode, market.StockCode, market.Code, market.Symbol)
            || !MatchesExchange(reference, market.ExchangeCode, market.Exchange))
        {
            return null;
        }

        var closePrice = First(
            market.ClosePrice,
            market.ClosingPrice,
            market.LastPrice,
            market.Price);
        var tradingDate = ParseDate(First(
            market.TradingDate,
            market.DataAsOfDate,
            market.AsOfDate,
            market.Date));
        var priceObservedAt = ParseDateTime(First(
            market.PriceObservedAt,
            market.ObservedAt));
        var sourceRecordId = First(market.SourceRecordId, market.RecordId, market.Id);
        var dataSource = First(market.DataSource, market.Source);

        return closePrice is null
            || tradingDate is null
            || priceObservedAt is null
            || sourceRecordId is null
            || dataSource is null
            ? null
            : new StockMarketData(
                reference.SecurityCode,
                reference.ExchangeCode,
                closePrice.Value,
                tradingDate.Value,
                priceObservedAt.Value,
                dataSource,
                sourceRecordId,
                First(market.DataQualityCode, market.QualityCode) ?? DataQualityCodes.Missing);
    }

    public static IReadOnlyList<StockDividendData>? NormalizeDividends(
        AShareReference reference,
        IReadOnlyList<FtShareDividendWire>? items,
        TimeProvider timeProvider)
    {
        if (items is null)
        {
            return null;
        }

        var dividends = new List<StockDividendData>(items.Count);
        foreach (var item in items)
        {
            if (!MatchesReference(reference, item.SecurityCode, item.StockCode, item.Code, item.Symbol)
                || !MatchesExchange(reference, item.ExchangeCode, item.Exchange))
            {
                return null;
            }

            var dividendPerShare = First(
                item.DividendPerShare,
                item.CashDividendPerShare,
                item.AmountPerShare,
                item.DividendAmount);
            var dividendTypeCode = NormalizeDividendType(
                First(item.DividendTypeCode, item.DividendType, item.Type),
                item.IsSpecialDividend);
            var dividendStatusCode = NormalizeDividendStatus(
                First(item.DividendStatusCode, item.DividendStatus, item.Status));
            var dataSource = First(item.DataSource, item.Source);
            var sourceRecordId = First(item.SourceRecordId, item.RecordId, item.Id);

            if (dividendPerShare is null
                || dividendTypeCode is null
                || dividendStatusCode is null
                || dataSource is null
                || sourceRecordId is null)
            {
                return null;
            }

            dividends.Add(new StockDividendData(
                reference.SecurityCode,
                reference.ExchangeCode,
                dividendPerShare.Value,
                dividendTypeCode,
                dividendStatusCode,
                ParseDate(First(item.AnnouncementDate, item.AnnouncedDate)),
                ParseDate(First(item.ExDividendDate, item.ExDate)),
                ParseDate(First(item.PaymentDate, item.PaidDate)),
                item.IsSpecialDividend ?? string.Equals(
                    dividendTypeCode,
                    "special_cash",
                    StringComparison.Ordinal),
                ParseDateTime(First(item.PublishedAt, item.PublishedTime)),
                ParseDateTime(First(item.CapturedAt, item.CapturedTime))
                    ?? timeProvider.GetUtcNow(),
                dataSource,
                sourceRecordId,
                First(item.DataQualityCode, item.QualityCode) ?? DataQualityCodes.Missing));
        }

        return dividends;
    }

    public static IReadOnlyList<StockFinancialData>? NormalizeFinancials(
        AShareReference reference,
        IReadOnlyList<FtShareFinancialWire>? items,
        TimeProvider timeProvider)
    {
        if (items is null)
        {
            return null;
        }

        var snapshots = new List<StockFinancialData>(items.Count);
        foreach (var item in items)
        {
            if (!MatchesReference(reference, item.SecurityCode, item.StockCode, item.Code, item.Symbol)
                || !MatchesExchange(reference, item.ExchangeCode, item.Exchange))
            {
                return null;
            }

            var dataAsOfDate = ParseDate(First(
                item.DataAsOfDate,
                item.FinancialDate,
                item.PeriodEndDate,
                item.Date));
            var dataSource = First(item.DataSource, item.Source);
            var sourceRecordId = First(item.SourceRecordId, item.RecordId, item.Id);
            if (dataAsOfDate is null || dataSource is null || sourceRecordId is null)
            {
                return null;
            }

            snapshots.Add(new StockFinancialData(
                reference.SecurityCode,
                reference.ExchangeCode,
                dataAsOfDate.Value,
                ParseDateTime(First(item.CapturedAt, item.CapturedTime))
                    ?? timeProvider.GetUtcNow(),
                ParseDateTime(First(item.PublishedAt, item.PublishedTime)),
                First(item.EarningsPerShare, item.Eps),
                First(item.DividendPayoutRatio, item.PayoutRatio),
                First(
                    item.ThreeYearAverageDividendPayoutRatio,
                    item.AverageDividendPayoutRatio),
                First(item.PriceToBookRatio, item.Pb),
                First(item.ReturnOnEquity, item.Roe),
                dataSource,
                sourceRecordId,
                First(item.DataQualityCode, item.QualityCode) ?? DataQualityCodes.Missing));
        }

        return snapshots;
    }

    private static bool MatchesReference(
        AShareReference reference,
        params string?[] values)
    {
        var returnedCode = First(values);
        return returnedCode is null
            || NormalizeSecurityCode(returnedCode) == reference.SecurityCode;
    }

    private static bool MatchesExchange(
        AShareReference reference,
        params string?[] values)
    {
        var returnedExchange = First(values);
        return returnedExchange is null
            || NormalizeExchangeCode(returnedExchange) == reference.ExchangeCode;
    }

    private static string? First(params string?[] values)
        => values.Select(value => value?.Trim())
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    private static decimal? First(params decimal?[] values)
        => values.FirstOrDefault(value => value is not null);

    private static DateOnly? ParseDate(string? value)
        => value is not null
            && DateOnly.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date)
            ? date
            : null;

    private static DateTimeOffset? ParseDateTime(string? value)
        => value is not null
            && DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out var dateTime)
            ? dateTime
            : null;

    private static string? NormalizeDividendType(
        string? value,
        bool? isSpecialDividend)
        => value?.Trim().ToLowerInvariant() switch
        {
            "special_cash" or "special" or "special cash" => "special_cash",
            "regular_cash" or "regular" or "cash" or "regular cash" => "regular_cash",
            _ => isSpecialDividend switch
            {
                true => "special_cash",
                false => "regular_cash",
                _ => null
            }
        };

    private static string? NormalizeDividendStatus(string? value)
        => value?.Trim().ToLowerInvariant() switch
        {
            "implemented" or "paid" or "completed" or "actual" => "implemented",
            "proposed" or "announced" or "pending" => "proposed",
            "cancelled" or "canceled" => "cancelled",
            _ => null
        };

    private static string NormalizeSecurityCode(string value)
    {
        var trimmed = value.Trim();
        return trimmed.Length < 6 && trimmed.All(character => character is >= '0' and <= '9')
            ? trimmed.PadLeft(6, '0')
            : trimmed;
    }

    private static string NormalizeExchangeCode(string value) => value.Trim().ToUpperInvariant() switch
    {
        "SH" or "SSE" => "SSE",
        "SZ" or "SZSE" => "SZSE",
        "BJ" or "BSE" => "BSE",
        _ => value.Trim().ToUpperInvariant()
    };

    private static string? NormalizeMarketCode(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        "A 股" or "A股" or "A-SHARE" or "A_SHARE" or "ASHARE" => "A-share",
        _ => null
    };

    private static string? NormalizeCurrencyCode(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        "CNY" or "RMB" or "人民币" => "CNY",
        _ => null
    };

    private static IReadOnlySet<string> CreatePropertySet(params string[] names)
        => new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
}
