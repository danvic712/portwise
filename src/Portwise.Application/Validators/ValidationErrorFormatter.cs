using FluentValidation.Results;

namespace Portwise.Application.Validators;

internal static class ValidationErrorFormatter
{
    public static string Format(ValidationResult validationResult)
        => string.Join(
            "；",
            validationResult.Errors.Select(error =>
                $"{error.PropertyName}: {error.ErrorMessage}"));
}
