using Portwise.Domain.Enums;

namespace Portwise.Domain.Codes;

/// <summary>
/// Defines stable persistence and transport codes for inference capabilities.
/// </summary>
public static class InferenceCapabilityCodes
{
    public const string Chat = "chat";

    public const string Embedding = "embedding";

    public static string From(InferenceCapability capability) => capability switch
    {
        InferenceCapability.Chat => Chat,
        InferenceCapability.Embedding => Embedding,
        _ => throw new ArgumentOutOfRangeException(nameof(capability), capability, null)
    };

    public static bool TryParse(string? code, out InferenceCapability capability)
    {
        switch (code)
        {
            case Chat:
                capability = InferenceCapability.Chat;
                return true;
            case Embedding:
                capability = InferenceCapability.Embedding;
                return true;
            default:
                capability = default;
                return false;
        }
    }
}
