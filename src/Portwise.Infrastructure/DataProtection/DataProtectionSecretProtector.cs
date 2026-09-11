using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Portwise.Application.Configuration;
using Portwise.Application.Configuration.Contracts;

namespace Portwise.Infrastructure.DataProtection;

internal sealed class DataProtectionSecretProtector(IDataProtectionProvider provider)
    : ISecretProtector
{
    private const string StockDataProviderCredentialsPurpose =
        "Portwise.StockDataProviderCredentials.v1";
    private const string InferenceProviderApiKeyPurpose =
        "Portwise.InferenceProviderApiKey.v1";

    public string Protect(string plaintext, SecretProtectionPurpose purpose)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plaintext);
        return CreateProtector(purpose).Protect(plaintext);
    }

    public bool TryUnprotect(
        string protectedValue,
        SecretProtectionPurpose purpose,
        out string? plaintext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(protectedValue);

        try
        {
            plaintext = CreateProtector(purpose).Unprotect(protectedValue);
            return true;
        }
        catch (CryptographicException)
        {
            plaintext = null;
            return false;
        }
    }

    private IDataProtector CreateProtector(SecretProtectionPurpose purpose) =>
        provider.CreateProtector(purpose switch
        {
            SecretProtectionPurpose.StockDataProviderCredentials =>
                StockDataProviderCredentialsPurpose,
            SecretProtectionPurpose.InferenceProviderApiKey => InferenceProviderApiKeyPurpose,
            _ => throw new ArgumentOutOfRangeException(nameof(purpose), purpose, null)
        });
}
