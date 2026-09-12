using Portwise.Domain.Enums;

namespace Portwise.Domain.Codes;

/// <summary>
/// Defines stable persistence and transport codes for provider verification states.
/// </summary>
public static class ProviderVerificationStateCodes
{
    public const string Unverified = "unverified";

    public const string Succeeded = "succeeded";

    public const string Failed = "failed";

    public static string From(ProviderVerificationState state) => state switch
    {
        ProviderVerificationState.Unverified => Unverified,
        ProviderVerificationState.Succeeded => Succeeded,
        ProviderVerificationState.Failed => Failed,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
    };

    public static bool TryParse(string? code, out ProviderVerificationState state)
    {
        switch (code)
        {
            case Unverified:
                state = ProviderVerificationState.Unverified;
                return true;
            case Succeeded:
                state = ProviderVerificationState.Succeeded;
                return true;
            case Failed:
                state = ProviderVerificationState.Failed;
                return true;
            default:
                state = default;
                return false;
        }
    }
}
