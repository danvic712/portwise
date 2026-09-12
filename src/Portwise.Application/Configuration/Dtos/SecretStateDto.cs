namespace Portwise.Application.Configuration.Dtos;

/// <summary>
/// Describes whether a saved secret is missing, configured, or unreadable.
/// </summary>
/// <param name="StateCode">Stable secret state code.</param>
public sealed record SecretStateDto(string StateCode);
