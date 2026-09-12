using Portwise.Domain.Enums;

namespace Portwise.Domain.Codes;

/// <summary>
/// Defines stable persistence and transport codes for inference provider types.
/// </summary>
public static class InferenceProviderTypeCodes
{
    public const string OpenAiCompatible = "openai-compatible";

    public static string From(InferenceProviderType providerType) => providerType switch
    {
        InferenceProviderType.OpenAiCompatible => OpenAiCompatible,
        _ => throw new ArgumentOutOfRangeException(
            nameof(providerType), providerType, null)
    };

    public static bool TryParse(string? code, out InferenceProviderType providerType)
    {
        if (code == OpenAiCompatible)
        {
            providerType = InferenceProviderType.OpenAiCompatible;
            return true;
        }

        providerType = default;
        return false;
    }
}
