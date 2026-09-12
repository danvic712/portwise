using System.Text.Json;
using Portwise.Infrastructure.FtShare;

namespace Portwise.Infrastructure.Contracts;

public interface IFtShareMcpToolInvoker
{
    Task VerifyAsync(FtShareOptions options, CancellationToken cancellationToken);

    Task<JsonElement?> InvokeAsync(
        FtShareOptions options,
        string toolName,
        IReadOnlyDictionary<string, object?> arguments,
        CancellationToken cancellationToken);
}
