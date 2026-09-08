using Portwise.Application.Exceptions;
using Portwise.Application.Localization;

namespace Portwise.Application.Contracts;

public interface IApplicationErrorCatalog
{
    string DefaultCultureName { get; }

    IReadOnlyCollection<string> SupportedCultureNames { get; }

    ApplicationErrorDefinition GetDefinition(
        string cultureName,
        string errorCode);
}
