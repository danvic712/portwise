using System.Text.Json;
using Portwise.Infrastructure.Contracts;
using Microsoft.Extensions.Options;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Polly.Registry;
using Polly.Timeout;

namespace Portwise.Infrastructure.FtShare;

public sealed class FtShareMcpToolInvoker(
    IOptions<FtShareOptions> options,
    IHttpClientFactory httpClientFactory,
    ResiliencePipelineProvider<string> exchangePipelineProvider,
    TimeProvider timeProvider) : IFtShareMcpToolInvoker
{
    internal const string HttpClientName = "FTShareMcp";

    internal const string ExchangePipelineName = "FTShareMcpExchange";

    public async Task<JsonElement?> InvokeAsync(
        string toolName,
        IReadOnlyDictionary<string, object?> arguments,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolName);
        ArgumentNullException.ThrowIfNull(arguments);

        var currentOptions = options.Value;
        var endpoint = new Uri(currentOptions.McpEndpoint, UriKind.Absolute);
        using var timeoutCancellation = new CancellationTokenSource(
            currentOptions.OperationTimeout,
            timeProvider);
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutCancellation.Token);

        try
        {
            var pipeline = exchangePipelineProvider.GetPipeline(ExchangePipelineName);
            return await pipeline.ExecuteAsync(
                static (state, token) => new ValueTask<JsonElement?>(
                    state.Invoker.InvokeOnceAsync(
                        state.Endpoint,
                        state.Options,
                        state.ToolName,
                        state.Arguments,
                        token)),
                (Invoker: this,
                    Endpoint: endpoint,
                    Options: currentOptions,
                    ToolName: toolName,
                    Arguments: arguments),
                linkedCancellation.Token);
        }
        catch (TimeoutRejectedException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("FTShare MCP tool call timed out.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("FTShare MCP tool call timed out.");
        }
    }

    private async Task<JsonElement?> InvokeOnceAsync(
        Uri endpoint,
        FtShareOptions options,
        string toolName,
        IReadOnlyDictionary<string, object?> arguments,
        CancellationToken cancellationToken)
    {
        await using var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = endpoint,
                TransportMode = HttpTransportMode.StreamableHttp,
                ConnectionTimeout = options.RequestTimeout
            },
            httpClientFactory.CreateClient(HttpClientName),
            ownsHttpClient: false);

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
}
