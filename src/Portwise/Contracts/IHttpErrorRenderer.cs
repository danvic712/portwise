using Portwise.Application.Localization;

namespace Portwise.Contracts;

public interface IHttpErrorRenderer
{
    ValueTask<bool> RenderAsync(
        HttpContext httpContext,
        LocalizedApplicationError error,
        CancellationToken cancellationToken);
}
