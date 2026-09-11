using Microsoft.AspNetCore.DataProtection;
using Portwise.Application.Configuration;
using Portwise.Infrastructure.DataProtection;
using Xunit;

namespace Portwise.Infrastructure.Tests;

public sealed class DataProtectionSecretProtectorTests
{
    [Fact]
    public void Protect_RoundTripsWithinTheSamePurpose()
    {
        var protector = new DataProtectionSecretProtector(
            new EphemeralDataProtectionProvider());

        var protectedValue = protector.Protect(
            "secret-value",
            SecretProtectionPurpose.StockDataProviderCredentials);

        var succeeded = protector.TryUnprotect(
            protectedValue,
            SecretProtectionPurpose.StockDataProviderCredentials,
            out var plaintext);

        Assert.True(succeeded);
        Assert.Equal("secret-value", plaintext);
        Assert.DoesNotContain("secret-value", protectedValue, StringComparison.Ordinal);
    }

    [Fact]
    public void TryUnprotect_ReturnsFalseForAnotherPurpose()
    {
        var protector = new DataProtectionSecretProtector(
            new EphemeralDataProtectionProvider());
        var protectedValue = protector.Protect(
            "secret-value",
            SecretProtectionPurpose.StockDataProviderCredentials);

        var succeeded = protector.TryUnprotect(
            protectedValue,
            SecretProtectionPurpose.InferenceProviderApiKey,
            out var plaintext);

        Assert.False(succeeded);
        Assert.Null(plaintext);
    }
}
