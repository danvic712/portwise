using System.Text.Json;
using Portwise.Infrastructure.Contracts;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Portwise.Infrastructure.FtShare;

public sealed class FtShareMcpToolInvoker(
    IHttpClientFactory httpClientFactory,
    TimeProvider timeProvider) : IFtShareMcpToolInvoker
{
    internal const string HttpClientName = "FTShareMcp";

    public async Task VerifyAsync(
        FtShareOptions options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);
        var endpoint = new Uri(options.McpEndpoint, UriKind.Absolute);
        using var timeoutCancellation = new CancellationTokenSource(
            options.OperationTimeout,
            timeProvider);
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutCancellation.Token);

        try
        {
            await using var transport = CreateTransport(endpoint, options);
            await using var client = await McpClient.CreateAsync(
                transport,
                cancellationToken: linkedCancellation.Token);
            await client.PingAsync(options: null, linkedCancellation.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("FTShare MCP verification timed out.");
        }
    }

    public async Task<JsonElement?> InvokeAsync(
        FtShareOptions options,
        string toolName,
        IReadOnlyDictionary<string, object?> arguments,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolName);
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(options);

        var endpoint = new Uri(options.McpEndpoint, UriKind.Absolute);
        using var timeoutCancellation = new CancellationTokenSource(
            options.OperationTimeout,
            timeProvider);
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutCancellation.Token);

        try
        {
            return await InvokeWithRetryAsync(
                endpoint,
                options,
                toolName,
                arguments,
                linkedCancellation.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("FTShare MCP tool call timed out.");
        }
    }

    private async Task<JsonElement?> InvokeWithRetryAsync(
        Uri endpoint,
        FtShareOptions options,
        string toolName,
        IReadOnlyDictionary<string, object?> arguments,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                return await InvokeOnceAsync(
                    endpoint,
                    options,
                    toolName,
                    arguments,
                    cancellationToken);
            }
            catch (Exception exception) when (
                attempt < options.MaxRetryCount
                && exception is HttpRequestException or IOException
                    or ClientTransportClosedException
                    or FtShareResponseStreamException)
            {
                var multiplier = 1L << Math.Min(attempt, 20);
                var delay = TimeSpan.FromMilliseconds(
                    checked(options.RetryDelayMilliseconds * multiplier));
                await Task.Delay(delay, timeProvider, cancellationToken);
            }
        }
    }

    private async Task<JsonElement?> InvokeOnceAsync(
        Uri endpoint,
        FtShareOptions options,
        string toolName,
        IReadOnlyDictionary<string, object?> arguments,
        CancellationToken cancellationToken)
    {
        await using var transport = CreateTransport(endpoint, options);

        await using var client = await McpClient.CreateAsync(
            transport,
            cancellationToken: cancellationToken);

        var result = await client.CallToolAsync(
            toolName,
            arguments,
            progress: null,
            options: null,
            cancellationToken);

        if (result.IsError == true)
        {
            var message = result.Content
                .OfType<TextContentBlock>()
                .Select(content => content.Text)
                .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text))
                ?? "FTShare MCP tool call failed.";

            throw new McpException(message);
        }

        if (result.StructuredContent is { } structuredContent)
        {
            return structuredContent.Clone();
        }

        foreach (var textContent in result.Content.OfType<TextContentBlock>())
        {
            try
            {
                using var document = JsonDocument.Parse(textContent.Text);
                return document.RootElement.Clone();
            }
            catch (JsonException)
            {
                // A text-only response is not a stock profile payload.
            }
        }

        return null;
    }

    private HttpClientTransport CreateTransport(Uri endpoint, FtShareOptions options) => new(
        new HttpClientTransportOptions
        {
            Endpoint = endpoint,
            TransportMode = HttpTransportMode.StreamableHttp,
            ConnectionTimeout = options.RequestTimeout,
            AdditionalHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Authorization"] = $"Bearer {options.ApiKey}"
            }
        },
        httpClientFactory.CreateClient(HttpClientName),
        ownsHttpClient: false);
}
