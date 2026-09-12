namespace Portwise.Domain.Exceptions;

/// <summary>
/// Reports a domain invariant violation through a stable code rather than localized text.
/// </summary>
public sealed class DomainRuleViolationException(string errorCode) : Exception(errorCode)
{
    public string ErrorCode { get; } = errorCode;
}
