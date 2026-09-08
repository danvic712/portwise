namespace Portwise.Application.Setup.Dtos;

/// <summary>Reports whether first-run setup has been completed.</summary>
/// <param name="IsComplete">Whether all setup requirements are complete.</param>
/// <param name="MissingRequirements">Requirement codes that are still missing.</param>
public sealed record SetupStatus(
    bool IsComplete,
    IReadOnlyList<string> MissingRequirements);
