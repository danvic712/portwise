using Asp.Versioning;
using Portwise.Application.Contracts;
using Portwise.Application.Recommendations.Contracts;
using Portwise.Application.Recommendations.Dtos;
using Portwise.Application.Stocks.Contracts;
using Portwise.Application.Stocks.Dtos;
using Portwise.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Portwise.Controllers;

/// <summary>Provides stock watchlist, market data and recommendation endpoints.</summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/stocks")]
public sealed class StocksController(
    IStockWatchlistAppService stockWatchlistAppService,
    IStockModelParameterAppService stockModelParameterAppService,
    IStockPriceObservationAppService stockPriceObservationAppService,
    IStockDividendEventAppService stockDividendEventAppService,
    IStockRecommendationAppService stockRecommendationAppService,
    IStockFinancialSnapshotAppService stockFinancialSnapshotAppService,
    IStockDataSyncRunner stockDataSyncRunner)
    : ControllerBase
{
    /// <summary>Returns all stocks configured in the portfolio watchlist.</summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StockWatchlistItem>>> GetStocks(
        CancellationToken cancellationToken)
    {
        var stocks = await stockWatchlistAppService.GetAsync(cancellationToken);
        return Ok(stocks);
    }

    /// <summary>Runs a manual synchronization for all configured stocks.</summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpPost("sync")]
    public async Task<ActionResult<StockDataSyncRunResult>> SyncStocks(
        CancellationToken cancellationToken)
    {
        var execution = await stockDataSyncRunner.RunAsync(
            StockDataSyncTrigger.Manual,
            cancellationToken);
        Response.Headers["X-Sync-Run-Id"] = execution.RunId;
        return Ok(execution.Result);
    }

    /// <summary>Returns the active model parameters for one stock.</summary>
    /// <param name="securityCode">Six-digit A-share security code.</param>
    /// <param name="exchangeCode">Exchange code: SSE, SZSE or BSE.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpGet("{securityCode}/{exchangeCode}/model-parameters")]
    public async Task<ActionResult<StockModelParameterSet>> GetModelParameters(
        string securityCode,
        string exchangeCode,
        CancellationToken cancellationToken)
    {
        var parameters = await stockModelParameterAppService.GetAsync(
            new GetStockModelParametersRequest(securityCode, exchangeCode),
            cancellationToken);
        return parameters is null ? NotFound() : Ok(parameters);
    }

    /// <summary>Creates a new model-parameter version for one stock.</summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <param name="request">Model parameter values and effective date.</param>
    [HttpPost("model-parameters")]
    public async Task<ActionResult<StockModelParameterSet>> SaveModelParameters(
        [FromBody] SaveStockModelParametersRequest request,
        CancellationToken cancellationToken)
    {
        var parameters = await stockModelParameterAppService.SaveAsync(
            request,
            cancellationToken);
        return CreatedAtAction(
            nameof(GetModelParameters),
            new
            {
                securityCode = parameters.SecurityCode,
                exchangeCode = parameters.ExchangeCode
            },
            parameters);
    }

    /// <summary>Synchronizes the latest price observation for one stock.</summary>
    /// <param name="securityCode">Six-digit A-share security code.</param>
    /// <param name="exchangeCode">Exchange code: SSE, SZSE or BSE.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpPost("{securityCode}/{exchangeCode}/price-observations/sync")]
    public async Task<ActionResult<StockPriceObservationResult>> SyncPriceObservation(
        string securityCode,
        string exchangeCode,
        CancellationToken cancellationToken)
    {
        var result = await stockPriceObservationAppService.SyncAsync(
            new SyncStockPriceRequest(securityCode, exchangeCode),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Synchronizes dividend events for one stock.</summary>
    /// <param name="securityCode">Six-digit A-share security code.</param>
    /// <param name="exchangeCode">Exchange code: SSE, SZSE or BSE.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpPost("{securityCode}/{exchangeCode}/dividend-events/sync")]
    public async Task<ActionResult<IReadOnlyList<StockDividendEventResult>>> SyncDividendEvents(
        string securityCode,
        string exchangeCode,
        CancellationToken cancellationToken)
    {
        var result = await stockDividendEventAppService.SyncAsync(
            new SyncStockDividendsRequest(securityCode, exchangeCode),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Synchronizes financial snapshots for one stock.</summary>
    /// <param name="securityCode">Six-digit A-share security code.</param>
    /// <param name="exchangeCode">Exchange code: SSE, SZSE or BSE.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpPost("{securityCode}/{exchangeCode}/financial-snapshots/sync")]
    public async Task<ActionResult<IReadOnlyList<StockFinancialSnapshotResult>>>
        SyncFinancialSnapshots(
            string securityCode,
            string exchangeCode,
            CancellationToken cancellationToken)
    {
        var result = await stockFinancialSnapshotAppService.SyncAsync(
            new SyncStockFinancialsRequest(securityCode, exchangeCode),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Returns the current recommendation analysis for one stock.</summary>
    /// <param name="securityCode">Six-digit A-share security code.</param>
    /// <param name="exchangeCode">Exchange code: SSE, SZSE or BSE.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpGet("{securityCode}/{exchangeCode}/analysis")]
    public async Task<ActionResult<StockRecommendationResult>> GetAnalysis(
        string securityCode,
        string exchangeCode,
        CancellationToken cancellationToken)
    {
        var result = await stockRecommendationAppService.GetAsync(
            new GetStockAnalysisRequest(securityCode, exchangeCode),
            cancellationToken);
        return Ok(result);
    }
}
