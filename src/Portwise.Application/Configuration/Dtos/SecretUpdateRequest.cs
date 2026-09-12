namespace Portwise.Application.Configuration.Dtos;

/// <summary>
/// Requests an explicit keep, replace, or clear operation for a secret.
/// </summary>
/// <param name="Action">Stable secret update action code.</param>
/// <param name="Value">New plaintext only when the action is replace.</param>
public sealed record SecretUpdateRequest(string Action, string? Value);
