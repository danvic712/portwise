using System.Text.Json;

namespace Portwise.Infrastructure.Contracts;

public interface IFtShareMcpToolInvoker
{
    Task<JsonElement?> InvokeAsync(
        string toolName,
        IReadOnlyDictionary<string, object?> arguments,
        CancellationToken cancellationToken);
}
