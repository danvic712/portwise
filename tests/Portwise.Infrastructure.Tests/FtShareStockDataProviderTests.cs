using System.Text.Json;
using Portwise.Application.Contracts;
using Portwise.Application.Diagnostics;
using Portwise.Infrastructure.Contracts;
using Portwise.Infrastructure.FtShare;
using Portwise.Domain.Securities;
using Xunit;

namespace Portwise.Infrastructure.Tests;

public sealed class FtShareStockDataProviderTests
{
    [Fact]
    public async Task GetAsyncDeserializesProfileEnvelopeIntoNormalizedData()
    {
        var provider = CreateProvider("""
            {
              "result": {
                "security_code": "600000",
                "exchange": "sse",
                "company_name": "浦发银行",
                "market": "A股",
                "currency": "RMB",
                "industry": "银行"
              }
            }
            """);

        var result = await provider.GetAsync(
            AShareReference.Create("600000", "SSE"),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("浦发银行", result.SecurityName);
        Assert.Equal("A-share", result.MarketCode);
        Assert.Equal("CNY", result.CurrencyCode);
        Assert.Equal("银行", result.SectorCode);
    }

    [Fact]
    public async Task GetAsyncAcceptsNumericIdentityWireValues()
    {
        var provider = CreateProvider("""
            {
              "profile": {
                "code": 600000,
                "exchange": "SSE",
                "security_name": "浦发银行",
                "market_code": "A股",
                "currency_code": "CNY"
              }
            }
            """);

        var result = await provider.GetAsync(
            AShareReference.Create("600000", "SSE"),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("浦发银行", result.SecurityName);
    }

    [Fact]
    public async Task GetAsyncUnwrapsStatusEnvelopeBeforeSelectingProfile()
    {
        var provider = CreateProvider("""
            {
              "code": 0,
              "message": "ok",
              "data": {
                "code": "600000",
                "exchange": "SSE",
                "security_name": "浦发银行",
                "market_code": "A股",
                "currency_code": "CNY"
              }
            }
            """);

        var result = await provider.GetAsync(
            AShareReference.Create("600000", "SSE"),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("浦发银行", result.SecurityName);
    }

    [Fact]
    public async Task GetMarketDataAsyncAcceptsNumericAndStringWireValues()
    {
        var provider = CreateProvider("""
            {
              "data": {
                "code": "600000",
                "exchange": "SH",
                "closing_price": "10.50",
                "date": "2026-09-10",
                "observed_at": "2026-09-10T08:00:00Z",
                "source": "ftshare",
                "record_id": "market-1"
              }
            }
            """);

        var result = await provider.GetMarketDataAsync(
            AShareReference.Create("600000", "SSE"),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(10.50m, result.ClosePrice);
        Assert.Equal(new DateOnly(2026, 9, 10), result.TradingDate);
        Assert.Equal("market-1", result.SourceRecordId);
    }

    [Fact]
    public async Task GetMarketDataAsyncAcceptsStringifiedJsonPayload()
    {
        var provider = CreateProvider("""
            "{\"data\":{\"code\":\"600000\",\"exchange\":\"SSE\",\"price\":\"10.50\",\"date\":\"2026-09-10\",\"observed_at\":\"2026-09-10T08:00:00Z\",\"source\":\"ftshare\",\"id\":123}}"
            """);

        var result = await provider.GetMarketDataAsync(
            AShareReference.Create("600000", "SSE"),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(10.50m, result.ClosePrice);
        Assert.Equal("123", result.SourceRecordId);
    }

    [Fact]
    public async Task GetDividendEventsAsyncNormalizesAliasesAndBooleanStrings()
    {
        var provider = CreateProvider("""
            {
              "events": [
                {
                  "code": "600000",
                  "exchange": "SSE",
                  "amount_per_share": "0.30",
                  "type": "special",
                  "status": "paid",
                  "announced_date": "2026-05-01",
                  "ex_date": "2026-05-10",
                  "paid_date": "2026-05-20",
                  "is_special_dividend": "true",
                  "published_time": "2026-05-01T08:00:00Z",
                  "captured_time": "2026-05-02T08:00:00Z",
                  "source": "ftshare",
                  "id": "dividend-1"
                }
              ]
            }
            """);

        var result = await provider.GetDividendEventsAsync(
            AShareReference.Create("600000", "SSE"),
            CancellationToken.None);

        var dividend = Assert.Single(result!);
        Assert.Equal(0.30m, dividend.DividendPerShare);
        Assert.Equal("special_cash", dividend.DividendTypeCode);
        Assert.Equal("implemented", dividend.DividendStatusCode);
        Assert.True(dividend.IsSpecialDividend);
    }

    [Fact]
    public async Task GetFinancialSnapshotsAsyncNormalizesMetricAliases()
    {
        var provider = CreateProvider("""
            {
              "snapshots": [
                {
                  "code": "600000",
                  "exchange": "SSE",
                  "financial_date": "2025-12-31",
                  "captured_time": "2026-04-01T08:00:00Z",
                  "eps": "1.20",
                  "payout_ratio": 0.30,
                  "average_dividend_payout_ratio": 0.25,
                  "pb": "1.50",
                  "roe": 0.12,
                  "source": "ftshare",
                  "id": "financial-1"
                }
              ]
            }
            """);

        var result = await provider.GetFinancialSnapshotsAsync(
            AShareReference.Create("600000", "SSE"),
            CancellationToken.None);

        var snapshot = Assert.Single(result!);
        Assert.Equal(new DateOnly(2025, 12, 31), snapshot.DataAsOfDate);
        Assert.Equal(1.20m, snapshot.EarningsPerShare);
        Assert.Equal(0.30m, snapshot.DividendPayoutRatio);
        Assert.Equal(0.25m, snapshot.ThreeYearAverageDividendPayoutRatio);
        Assert.Equal(1.50m, snapshot.PriceToBookRatio);
        Assert.Equal(0.12m, snapshot.ReturnOnEquity);
    }

    [Fact]
    public async Task GetFinancialSnapshotsAsyncKeepsSnapshotWhenOptionalMetricIsMalformed()
    {
        var provider = CreateProvider("""
            {
              "snapshots": [
                {
                  "code": "600000",
                  "exchange": "SSE",
                  "financial_date": "2025-12-31",
                  "eps": "",
                  "pb": "not-a-number",
                  "source": "ftshare",
                  "id": "financial-2"
                }
              ]
            }
            """);

        var result = await provider.GetFinancialSnapshotsAsync(
            AShareReference.Create("600000", "SSE"),
            CancellationToken.None);

        var snapshot = Assert.Single(result!);
        Assert.Equal(new DateOnly(2025, 12, 31), snapshot.DataAsOfDate);
        Assert.Null(snapshot.EarningsPerShare);
        Assert.Null(snapshot.PriceToBookRatio);
    }

    private static FtShareStockDataProvider CreateProvider(string json)
    {
        using var document = JsonDocument.Parse(json);
        var payload = document.RootElement.Clone();

        return new FtShareStockDataProvider(
            new StubInvoker(payload),
            new FtShareOptions(),
            new StubDiagnosticContext(),
            TimeProvider.System);
    }

    private sealed class StubInvoker(JsonElement payload) : IFtShareMcpToolInvoker
    {
        public Task VerifyAsync(
            FtShareOptions options,
            CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<JsonElement?> InvokeAsync(
            FtShareOptions options,
            string toolName,
            IReadOnlyDictionary<string, object?> arguments,
            CancellationToken cancellationToken)
            => Task.FromResult<JsonElement?>(payload);
    }

    private sealed class StubDiagnosticContext : IDiagnosticContext
    {
        public IDisposable BeginScope(DiagnosticScope scope) => NoopDisposable.Instance;
    }

    private sealed class NoopDisposable : IDisposable
    {
        public static readonly NoopDisposable Instance = new();

        public void Dispose()
        {
        }
    }
}
