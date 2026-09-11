using Portwise.Application.Exceptions;
using Portwise.Application.Preferences.Contracts;
using Portwise.Application.Preferences.Dtos;
using Portwise.Domain.Codes;
using Portwise.Domain.Contracts;
using Portwise.Domain.Exceptions;
using Portwise.Domain.Models;

namespace Portwise.Application.Preferences;

public sealed class PreferencesAppService(
    IUow uow,
    TimeProvider timeProvider) : IPreferencesAppService
{
    public async Task<PreferencesResponse> GetAsync(CancellationToken cancellationToken)
    {
        var preference = await uow.Get<ApplicationPreference>()
            .SingleOrDefaultAsync(cancellationToken);
        return preference is null
            ? throw ApplicationErrors.Simple(ApplicationErrorCodes.PreferencesNotConfigured)
            : ToResponse(preference);
    }

    public async Task<PreferencesResponse> UpdateAsync(
        UpdatePreferencesRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!ApplicationLanguageCodes.TryParse(request.LanguageCode, out var language)
            || !ApplicationThemeCodes.TryParse(request.ThemeCode, out var theme)
            || request.ExpectedRevision < 1)
        {
            throw ApplicationErrors.Simple(ApplicationErrorCodes.PreferencesValidationFailed);
        }

        var preference = await uow.Get<ApplicationPreference>()
            .SingleOrDefaultAsync(cancellationToken, asNoTracking: false);
        if (preference is null)
        {
            throw ApplicationErrors.Simple(ApplicationErrorCodes.PreferencesNotConfigured);
        }

        if (preference.Revision != request.ExpectedRevision)
        {
            throw ApplicationErrors.Simple(ApplicationErrorCodes.PreferencesRevisionConflict);
        }

        preference.Update(language, theme, timeProvider.GetUtcNow());
        try
        {
            await uow.CommitAsync(cancellationToken);
        }
        catch (UnitOfWorkCommitException exception) when (exception.IsConcurrencyConflict)
        {
            throw ApplicationErrors.Simple(
                ApplicationErrorCodes.PreferencesRevisionConflict,
                exception);
        }

        return ToResponse(preference);
    }

    private static PreferencesResponse ToResponse(ApplicationPreference preference) => new(
        ApplicationLanguageCodes.From(preference.Language),
        ApplicationThemeCodes.From(preference.Theme),
        preference.Revision,
        preference.UpdatedAtUtc);
}
