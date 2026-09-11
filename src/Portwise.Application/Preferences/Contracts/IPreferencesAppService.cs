using Portwise.Application.Preferences.Dtos;

namespace Portwise.Application.Preferences.Contracts;

/// <summary>
/// Provides the application interface for reading and updating preferences.
/// </summary>
public interface IPreferencesAppService
{
    Task<PreferencesResponse> GetAsync(CancellationToken cancellationToken);

    Task<PreferencesResponse> UpdateAsync(
        UpdatePreferencesRequest request,
        CancellationToken cancellationToken);
}
