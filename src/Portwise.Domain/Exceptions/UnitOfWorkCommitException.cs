namespace Portwise.Domain.Exceptions;

/// <summary>
/// Represents a persistence commit failure without exposing the ORM provider
/// to an Application module.
/// </summary>
public sealed class UnitOfWorkCommitException(
    Exception innerException,
    bool isUniqueConstraintViolation = false,
    bool isConcurrencyConflict = false,
    bool isForeignKeyConstraintViolation = false)
    : Exception("The unit of work could not be committed.", innerException)
{
    public bool IsUniqueConstraintViolation { get; } = isUniqueConstraintViolation;

    public bool IsConcurrencyConflict { get; } = isConcurrencyConflict;

    public bool IsForeignKeyConstraintViolation { get; } = isForeignKeyConstraintViolation;
}
