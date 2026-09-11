namespace Portwise.Application.Configuration.Contracts;

/// <summary>
/// Protects secret values and safely detects values that can no longer be unprotected.
/// </summary>
public interface ISecretProtector
{
    /// <summary>
    /// Protects a plaintext value for the specified isolated purpose.
    /// </summary>
    /// <param name="plaintext">The plaintext value to protect.</param>
    /// <param name="purpose">The isolated protection purpose.</param>
    /// <returns>The protected value suitable for persistence.</returns>
    string Protect(string plaintext, SecretProtectionPurpose purpose);

    /// <summary>
    /// Attempts to unprotect a persisted value without exposing protection failures.
    /// </summary>
    /// <param name="protectedValue">The protected value to read.</param>
    /// <param name="purpose">The isolated protection purpose.</param>
    /// <param name="plaintext">The plaintext when unprotection succeeds.</param>
    /// <returns><see langword="true" /> when the value was unprotected.</returns>
    bool TryUnprotect(
        string protectedValue,
        SecretProtectionPurpose purpose,
        out string? plaintext);
}
