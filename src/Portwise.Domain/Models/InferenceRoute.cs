using Portwise.Domain.Enums;

namespace Portwise.Domain.Models;

/// <summary>
/// Maps one inference capability to a provider and model.
/// </summary>
public sealed class InferenceRoute
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public InferenceCapability Capability { get; set; }

    public Guid? ProviderId { get; set; }

    public string? ModelName { get; set; }

    public long Revision { get; set; } = 1;

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
