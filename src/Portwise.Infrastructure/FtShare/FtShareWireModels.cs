using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Portwise.Infrastructure.FtShare;

internal sealed class FtShareProfileWire
{
    [JsonPropertyName("security_code")]
    public string? SecurityCode { get; init; }

    [JsonPropertyName("stock_code")]
    public string? StockCode { get; init; }

    [JsonPropertyName("code")]
    public string? Code { get; init; }

    [JsonPropertyName("symbol")]
    public string? Symbol { get; init; }

    [JsonPropertyName("exchange_code")]
    public string? ExchangeCode { get; init; }

    [JsonPropertyName("exchange")]
    public string? Exchange { get; init; }

    [JsonPropertyName("security_name")]
    public string? SecurityName { get; init; }

    [JsonPropertyName("stock_name")]
    public string? StockName { get; init; }

    [JsonPropertyName("company_name")]
    public string? CompanyName { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("market_code")]
    public string? MarketCode { get; init; }

    [JsonPropertyName("market")]
    public string? Market { get; init; }

    [JsonPropertyName("market_type")]
    public string? MarketType { get; init; }

    [JsonPropertyName("currency_code")]
    public string? CurrencyCode { get; init; }

    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    [JsonPropertyName("sector_code")]
    public string? SectorCode { get; init; }

    [JsonPropertyName("industry_code")]
    public string? IndustryCode { get; init; }

    [JsonPropertyName("sector")]
    public string? Sector { get; init; }

    [JsonPropertyName("industry")]
    public string? Industry { get; init; }
}

internal sealed class FtShareMarketWire
{
    [JsonPropertyName("security_code")]
    public string? SecurityCode { get; init; }

    [JsonPropertyName("stock_code")]
    public string? StockCode { get; init; }

    [JsonPropertyName("code")]
    public string? Code { get; init; }

    [JsonPropertyName("symbol")]
    public string? Symbol { get; init; }

    [JsonPropertyName("exchange_code")]
    public string? ExchangeCode { get; init; }

    [JsonPropertyName("exchange")]
    public string? Exchange { get; init; }

    [JsonPropertyName("close_price")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? ClosePrice { get; init; }

    [JsonPropertyName("closing_price")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? ClosingPrice { get; init; }

    [JsonPropertyName("last_price")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? LastPrice { get; init; }

    [JsonPropertyName("price")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? Price { get; init; }

    [JsonPropertyName("trading_date")]
    public string? TradingDate { get; init; }

    [JsonPropertyName("data_as_of_date")]
    public string? DataAsOfDate { get; init; }

    [JsonPropertyName("as_of_date")]
    public string? AsOfDate { get; init; }

    [JsonPropertyName("date")]
    public string? Date { get; init; }

    [JsonPropertyName("price_observed_at")]
    public string? PriceObservedAt { get; init; }

    [JsonPropertyName("observed_at")]
    public string? ObservedAt { get; init; }

    [JsonPropertyName("source_record_id")]
    public string? SourceRecordId { get; init; }

    [JsonPropertyName("record_id")]
    public string? RecordId { get; init; }

    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("data_source")]
    public string? DataSource { get; init; }

    [JsonPropertyName("source")]
    public string? Source { get; init; }

    [JsonPropertyName("data_quality_code")]
    public string? DataQualityCode { get; init; }

    [JsonPropertyName("quality_code")]
    public string? QualityCode { get; init; }
}

internal sealed class FtShareDividendWire
{
    [JsonPropertyName("security_code")]
    public string? SecurityCode { get; init; }

    [JsonPropertyName("stock_code")]
    public string? StockCode { get; init; }

    [JsonPropertyName("code")]
    public string? Code { get; init; }

    [JsonPropertyName("symbol")]
    public string? Symbol { get; init; }

    [JsonPropertyName("exchange_code")]
    public string? ExchangeCode { get; init; }

    [JsonPropertyName("exchange")]
    public string? Exchange { get; init; }

    [JsonPropertyName("dividend_per_share")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? DividendPerShare { get; init; }

    [JsonPropertyName("cash_dividend_per_share")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? CashDividendPerShare { get; init; }

    [JsonPropertyName("amount_per_share")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? AmountPerShare { get; init; }

    [JsonPropertyName("dividend_amount")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? DividendAmount { get; init; }

    [JsonPropertyName("dividend_type_code")]
    public string? DividendTypeCode { get; init; }

    [JsonPropertyName("dividend_type")]
    public string? DividendType { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("is_special_dividend")]
    [JsonConverter(typeof(FtShareBooleanConverter))]
    public bool? IsSpecialDividend { get; init; }

    [JsonPropertyName("dividend_status_code")]
    public string? DividendStatusCode { get; init; }

    [JsonPropertyName("dividend_status")]
    public string? DividendStatus { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("announcement_date")]
    public string? AnnouncementDate { get; init; }

    [JsonPropertyName("announced_date")]
    public string? AnnouncedDate { get; init; }

    [JsonPropertyName("ex_dividend_date")]
    public string? ExDividendDate { get; init; }

    [JsonPropertyName("ex_date")]
    public string? ExDate { get; init; }

    [JsonPropertyName("payment_date")]
    public string? PaymentDate { get; init; }

    [JsonPropertyName("paid_date")]
    public string? PaidDate { get; init; }

    [JsonPropertyName("published_at")]
    public string? PublishedAt { get; init; }

    [JsonPropertyName("published_time")]
    public string? PublishedTime { get; init; }

    [JsonPropertyName("captured_at")]
    public string? CapturedAt { get; init; }

    [JsonPropertyName("captured_time")]
    public string? CapturedTime { get; init; }

    [JsonPropertyName("data_source")]
    public string? DataSource { get; init; }

    [JsonPropertyName("source")]
    public string? Source { get; init; }

    [JsonPropertyName("source_record_id")]
    public string? SourceRecordId { get; init; }

    [JsonPropertyName("record_id")]
    public string? RecordId { get; init; }

    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("data_quality_code")]
    public string? DataQualityCode { get; init; }

    [JsonPropertyName("quality_code")]
    public string? QualityCode { get; init; }
}

internal sealed class FtShareFinancialWire
{
    [JsonPropertyName("security_code")]
    public string? SecurityCode { get; init; }

    [JsonPropertyName("stock_code")]
    public string? StockCode { get; init; }

    [JsonPropertyName("code")]
    public string? Code { get; init; }

    [JsonPropertyName("symbol")]
    public string? Symbol { get; init; }

    [JsonPropertyName("exchange_code")]
    public string? ExchangeCode { get; init; }

    [JsonPropertyName("exchange")]
    public string? Exchange { get; init; }

    [JsonPropertyName("data_as_of_date")]
    public string? DataAsOfDate { get; init; }

    [JsonPropertyName("financial_date")]
    public string? FinancialDate { get; init; }

    [JsonPropertyName("period_end_date")]
    public string? PeriodEndDate { get; init; }

    [JsonPropertyName("date")]
    public string? Date { get; init; }

    [JsonPropertyName("captured_at")]
    public string? CapturedAt { get; init; }

    [JsonPropertyName("captured_time")]
    public string? CapturedTime { get; init; }

    [JsonPropertyName("published_at")]
    public string? PublishedAt { get; init; }

    [JsonPropertyName("published_time")]
    public string? PublishedTime { get; init; }

    [JsonPropertyName("earnings_per_share")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? EarningsPerShare { get; init; }

    [JsonPropertyName("eps")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? Eps { get; init; }

    [JsonPropertyName("dividend_payout_ratio")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? DividendPayoutRatio { get; init; }

    [JsonPropertyName("payout_ratio")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? PayoutRatio { get; init; }

    [JsonPropertyName("three_year_average_dividend_payout_ratio")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? ThreeYearAverageDividendPayoutRatio { get; init; }

    [JsonPropertyName("average_dividend_payout_ratio")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? AverageDividendPayoutRatio { get; init; }

    [JsonPropertyName("price_to_book_ratio")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? PriceToBookRatio { get; init; }

    [JsonPropertyName("pb")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? Pb { get; init; }

    [JsonPropertyName("return_on_equity")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? ReturnOnEquity { get; init; }

    [JsonPropertyName("roe")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? Roe { get; init; }

    [JsonPropertyName("data_source")]
    public string? DataSource { get; init; }

    [JsonPropertyName("source")]
    public string? Source { get; init; }

    [JsonPropertyName("source_record_id")]
    public string? SourceRecordId { get; init; }

    [JsonPropertyName("record_id")]
    public string? RecordId { get; init; }

    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("data_quality_code")]
    public string? DataQualityCode { get; init; }

    [JsonPropertyName("quality_code")]
    public string? QualityCode { get; init; }
}

internal sealed class FtShareBooleanConverter : JsonConverter<bool?>
{
    public override bool? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.String when bool.TryParse(reader.GetString(), out var value) => value,
            _ => ReadUnsupportedValue(ref reader)
        };

    private static bool? ReadUnsupportedValue(ref Utf8JsonReader reader)
    {
        reader.Skip();
        return null;
    }

    public override void Write(
        Utf8JsonWriter writer,
        bool? value,
        JsonSerializerOptions options)
    {
        if (value is { } boolean)
        {
            writer.WriteBooleanValue(boolean);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

internal sealed class FtShareStringConverter : JsonConverter<string?>
{
    public override bool HandleNull => true;

    public override string? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number or JsonTokenType.True or JsonTokenType.False => ReadPrimitiveAsString(ref reader),
            _ => ReadUnsupportedValue(ref reader)
        };

    private static string? ReadUnsupportedValue(ref Utf8JsonReader reader)
    {
        reader.Skip();
        return null;
    }

    private static string ReadPrimitiveAsString(ref Utf8JsonReader reader)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        return document.RootElement.GetRawText();
    }

    public override void Write(
        Utf8JsonWriter writer,
        string? value,
        JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value);
    }
}

internal sealed class FtShareDecimalConverter : JsonConverter<decimal?>
{
    public override bool HandleNull => true;

    public override decimal? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.Number:
                return reader.TryGetDecimal(out var number) ? number : null;
            case JsonTokenType.String:
                return decimal.TryParse(
                    reader.GetString(),
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out number)
                    ? number
                    : null;
            default:
                reader.Skip();
                return null;
        }
    }

    public override void Write(
        Utf8JsonWriter writer,
        decimal? value,
        JsonSerializerOptions options)
    {
        if (value is { } number)
        {
            writer.WriteNumberValue(number);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    Converters = new[] { typeof(FtShareStringConverter), typeof(FtShareDecimalConverter) })]
[JsonSerializable(typeof(FtShareProfileWire))]
[JsonSerializable(typeof(FtShareMarketWire))]
[JsonSerializable(typeof(FtShareDividendWire))]
[JsonSerializable(typeof(FtShareFinancialWire))]
internal partial class FtShareJsonContext : JsonSerializerContext;

internal static class FtSharePayloadReader
{
    public static T? ReadOne<T>(
        JsonElement? payload,
        JsonTypeInfo<T> typeInfo,
        IReadOnlySet<string> leafPropertyNames,
        IReadOnlyList<string> envelopeNames)
    {
        var selected = SelectPayload(payload, leafPropertyNames, envelopeNames);
        return selected is { ValueKind: JsonValueKind.Object } value
            ? Deserialize(value, typeInfo)
            : default;
    }

    public static IReadOnlyList<T>? ReadMany<T>(
        JsonElement? payload,
        JsonTypeInfo<T> typeInfo,
        IReadOnlySet<string> leafPropertyNames,
        IReadOnlyList<string> envelopeNames)
    {
        if (payload is not { } value)
        {
            return null;
        }

        var selected = SelectCollection(value, leafPropertyNames, envelopeNames);
        if (selected is null)
        {
            return null;
        }

        if (selected.Value.ValueKind == JsonValueKind.Object)
        {
            var item = Deserialize(selected.Value, typeInfo);
            return item is null ? null : [item];
        }

        if (selected.Value.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var items = new List<T>();
        foreach (var element in selected.Value.EnumerateArray())
        {
            var item = Deserialize(element, typeInfo);
            if (item is null)
            {
                return null;
            }

            items.Add(item);
        }

        return items;
    }

    private static JsonElement? SelectPayload(
        JsonElement? payload,
        IReadOnlySet<string> leafPropertyNames,
        IReadOnlyList<string> envelopeNames)
    {
        if (payload is not { } value)
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            try
            {
                using var document = JsonDocument.Parse(value.GetString() ?? string.Empty);
                return SelectPayload(document.RootElement, leafPropertyNames, envelopeNames);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        if (value.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (HasAnyProperty(value, leafPropertyNames))
        {
            return value.Clone();
        }

        foreach (var envelopeName in envelopeNames)
        {
            if (TryGetProperty(value, envelopeName, out var nested))
            {
                var selected = SelectPayload(nested, leafPropertyNames, envelopeNames);
                if (selected is not null)
                {
                    return selected;
                }
            }
        }

        return null;
    }

    private static JsonElement? SelectCollection(
        JsonElement value,
        IReadOnlySet<string> leafPropertyNames,
        IReadOnlyList<string> envelopeNames)
    {
        if (value.ValueKind == JsonValueKind.String)
        {
            try
            {
                using var document = JsonDocument.Parse(value.GetString() ?? string.Empty);
                return SelectCollection(document.RootElement, leafPropertyNames, envelopeNames);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        if (value.ValueKind == JsonValueKind.Array)
        {
            return value.Clone();
        }

        if (value.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (HasAnyProperty(value, leafPropertyNames))
        {
            return value.Clone();
        }

        foreach (var envelopeName in envelopeNames)
        {
            if (TryGetProperty(value, envelopeName, out var nested))
            {
                var selected = SelectCollection(nested, leafPropertyNames, envelopeNames);
                if (selected is not null)
                {
                    return selected;
                }
            }
        }

        return null;
    }

    private static T? Deserialize<T>(JsonElement value, JsonTypeInfo<T> typeInfo)
    {
        try
        {
            return value.Deserialize(typeInfo);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private static bool HasAnyProperty(
        JsonElement value,
        IReadOnlySet<string> propertyNames)
        => value.EnumerateObject().Any(property =>
            propertyNames.Contains(property.Name));

    private static bool TryGetProperty(
        JsonElement value,
        string propertyName,
        out JsonElement property)
    {
        foreach (var candidate in value.EnumerateObject())
        {
            if (string.Equals(candidate.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                property = candidate.Value;
                return true;
            }
        }

        property = default;
        return false;
    }
}
