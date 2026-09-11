using Portwise.Application.Setup.Dtos;

namespace Portwise.Application.Setup.Contracts;

public interface ISetupAppService
{
    Task<SetupStatusDto> GetStatusAsync(CancellationToken cancellationToken =  default);

    Task<SetupResult> InitializeAsync(
        SetupRequest request,
        CancellationToken cancellationToken = default);
}
