namespace Portwise.Application.Setup.Dtos;

public sealed record SetupStatus(
    bool IsComplete,
    IReadOnlyList<string> MissingRequirements);
