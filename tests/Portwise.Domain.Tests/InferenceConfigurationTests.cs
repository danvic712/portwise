using Portwise.Domain.Codes;
using Portwise.Domain.Enums;
using Portwise.Domain.Exceptions;
using Portwise.Domain.Models;
using Xunit;

namespace Portwise.Domain.Tests;

public sealed class InferenceConfigurationTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 9, 12, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ProviderCreate_NormalizesNameAndUsesVersion7Id()
    {
        var provider = InferenceProvider.Create(
            " Primary Ai ",
            "https://ai.example/v1",
            "protected:key",
            Now);

        Assert.Equal(7, provider.Id.Version);
        Assert.Equal("Primary Ai", provider.Name);
        Assert.Equal("PRIMARY AI", provider.NormalizedName);
        Assert.Equal(ProviderVerificationState.Unverified, provider.VerificationState);
    }

    [Fact]
    public void ProviderConnectionChange_ClearsPreviousVerification()
    {
        var provider = InferenceProvider.Create(
            "Primary",
            "https://ai.example/v1",
            "protected:old",
            Now.AddMinutes(-10));
        provider.MarkVerificationSucceeded(Now.AddMinutes(-5));

        provider.Update(
            "Primary",
            "https://other.example/v1",
            true,
            "protected:new",
            Now);

        Assert.Equal(ProviderVerificationState.Unverified, provider.VerificationState);
        Assert.Null(provider.LastVerifiedAtUtc);
        Assert.Equal(3, provider.Revision);
    }

    [Fact]
    public void RouteBinding_RequiresProviderAndModelTogether()
    {
        var route = new InferenceRoute { Capability = InferenceCapability.Chat };

        var exception = Assert.Throws<DomainRuleViolationException>(() =>
            route.Bind(Guid.CreateVersion7(), null, Now));

        Assert.Equal(DomainRuleCodes.InferenceRouteBindingInvalid, exception.ErrorCode);
    }
}
