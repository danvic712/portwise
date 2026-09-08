using Portwise.Application.Exceptions;
using Portwise.Application.Localization;

namespace Portwise.Application.Contracts;

public interface IApplicationErrorLocalizer
{
    LocalizedApplicationError Localize(
        ApplicationExceptionBase exception,
        string? acceptLanguage = null);
}
