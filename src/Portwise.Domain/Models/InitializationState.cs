using Portwise.Domain.Codes;

namespace Portwise.Domain.Models;

/// <summary>
/// Records that the user explicitly completed onboarding.
/// </summary>
public sealed class InitializationState
{
    public Guid Id { get; set; } = KnownConfigurationIds.InitializationState;

    public DateTimeOffset CompletedAtUtc { get; set; }

    public long Revision { get; set; } = 1;
}
