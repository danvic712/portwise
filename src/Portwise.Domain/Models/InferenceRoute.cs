using Portwise.Domain.Enums;
using Portwise.Domain.Codes;
using Portwise.Domain.Exceptions;

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

    public void Bind(
        Guid? providerId,
        string? modelName,
        DateTimeOffset updatedAtUtc)
    {
        if ((providerId is null) != (modelName is null))
        {
            throw new DomainRuleViolationException(
                DomainRuleCodes.InferenceRouteBindingInvalid);
        }

        ProviderId = providerId;
        ModelName = modelName?.Trim();
        Revision = checked(Revision + 1);
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }
}
