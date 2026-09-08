using Portwise.Application.Contracts;
using Portwise.Application.Stocks.Contracts;
using Portwise.Application.Stocks.Dtos;
using Portwise.Application.Exceptions;
using Portwise.Application.Validators;
using Portwise.Domain.Securities;
using FluentValidation;

namespace Portwise.Application.Stocks;

public sealed class StockFinancialSnapshotAppService(
    IStockFactSyncAppService stockFactSyncAppService,
    IValidator<SyncStockFinancialsRequest> requestValidator)
    : IStockFinancialSnapshotAppService
{
    public async Task<IReadOnlyList<StockFinancialSnapshotResult>> SyncAsync(
        SyncStockFinancialsRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var validationResult = await requestValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw ApplicationErrors.Validation(
                ApplicationErrorCodes.StockDataSyncValidationFailed,
                ValidationErrorFormatter.Format(validationResult));
        }

        var reference = AShareReference.Create(request.SecurityCode, request.ExchangeCode);
        return await stockFactSyncAppService.SyncFinancialsAsync(
            reference,
            cancellationToken);
    }
}
