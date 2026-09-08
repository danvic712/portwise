using Portwise.Application.Portfolio.Dtos;
using Portwise.Application.Validators;
using Portwise.Domain.Portfolio;
using FluentValidation;

namespace Portwise.Application.Portfolio.Validators;

public sealed class RecordCashLedgerEntryRequestValidator
    : AbstractValidator<RecordCashLedgerEntryRequest>
{
    public RecordCashLedgerEntryRequestValidator()
    {
        RuleFor(x => x.EntryDate)
            .NotEqual(DateOnly.MinValue)
            .WithMessage("Cash ledger date is required.");

        RuleFor(x => x.EntryTypeCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(CashLedgerCodes.IsSupportedEntryType)
            .WithMessage("Cash ledger type is not supported.");

        RuleFor(x => x.CashDirectionCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(CashLedgerCodes.IsSupportedDirection)
            .WithMessage("Cash ledger direction is not supported.");

        RuleFor(x => x.CashDirectionCode)
            .Must((request, direction) =>
                CashLedgerCodes.IsCompatible(request.EntryTypeCode, direction))
            .WithMessage("Cash ledger type and direction do not match.");

        RuleFor(x => x.CashAmount)
            .GreaterThan(0)
            .WithMessage("Cash ledger amount must be greater than zero.");

        RuleFor(x => x.SourceRecordId)
            .MaximumLength(200)
            .WithMessage("Source record identifier cannot exceed 200 characters.");

        RuleFor(x => x.SecurityCode)
            .Must(value => string.IsNullOrWhiteSpace(value)
                || AShareValidationRules.IsValidSecurityCode(value))
            .WithMessage("Security code must contain exactly 6 digits.");

        RuleFor(x => x.ExchangeCode)
            .Must(value => string.IsNullOrWhiteSpace(value)
                || AShareValidationRules.IsSupportedExchange(value))
            .WithMessage("Exchange code must be SSE, SZSE or BSE.");

        RuleFor(x => x.ExchangeCode)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.SecurityCode))
            .WithMessage("An exchange code is required when a security code is provided.");

        RuleFor(x => x.SecurityCode)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.ExchangeCode))
            .WithMessage("A security code is required when an exchange code is provided.");

        RuleFor(x => x.SecurityCode)
            .NotEmpty()
            .When(x => x.EntryTypeCode?.Trim().ToLowerInvariant() is
                CashLedgerCodes.Buy or
                CashLedgerCodes.Sell or
                CashLedgerCodes.DividendReceived)
            .WithMessage("Buy, sell and received-dividend entries must reference a security.");

        RuleFor(x => x.ExchangeCode)
            .NotEmpty()
            .When(x => x.EntryTypeCode?.Trim().ToLowerInvariant() is
                CashLedgerCodes.Buy or
                CashLedgerCodes.Sell or
                CashLedgerCodes.DividendReceived)
            .WithMessage("Buy, sell and received-dividend entries must reference an exchange.");
    }

}
