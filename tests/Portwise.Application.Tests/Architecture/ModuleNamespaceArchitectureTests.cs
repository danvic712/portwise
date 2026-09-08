using Portwise.Application;
using Xunit;

namespace Portwise.Application.Tests;

public sealed class ModuleNamespaceArchitectureTests
{
    [Fact]
    public void Public_application_contracts_dtos_and_validators_stay_with_their_module()
    {
        var assembly = typeof(ApplicationServiceCollectionExtensions).Assembly;
        var technicalBuckets = new[]
        {
            "Portwise.Application.Contracts",
            "Portwise.Application.Dtos",
            "Portwise.Application.Validators"
        };

        var leakedTypes = assembly
            .GetExportedTypes()
            .Where(type => technicalBuckets.Contains(type.Namespace, StringComparer.Ordinal))
            .Where(type => type.Name is not "IApplicationErrorCatalog"
                and not "IApplicationErrorLocalizer"
                and not "IDiagnosticContext"
                and not "StockHoldingSnapshot"
                and not "AShareValidationRules"
                and not "ValidationErrorFormatter")
            .Select(type => type.FullName)
            .ToArray();

        Assert.Empty(leakedTypes);
    }

    [Fact]
    public void Mapper_definitions_are_owned_by_their_module()
    {
        var assembly = typeof(ApplicationServiceCollectionExtensions).Assembly;
        var mapperNamespaces = assembly
            .GetExportedTypes()
            .Where(type => type.Name.EndsWith("Mapper", StringComparison.Ordinal))
            .Select(type => type.Namespace!)
            .OrderBy(namespaceName => namespaceName)
            .ToArray();

        Assert.Equal(
            [
                "Portwise.Application.Portfolio",
                "Portwise.Application.Recommendations",
                "Portwise.Application.Stocks"
            ],
            mapperNamespaces);
    }
}
