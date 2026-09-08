using System.Runtime.ExceptionServices;
using System.Text.Json;
using Portwise.Infrastructure.Contracts;
using Microsoft.Extensions.Options;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Portwise.Infrastructure.FtShare;

public sealed class FtShareMcpToolInvoker(
    IOptions<FtShareOptions> options,
    TimeProvider timeProvider) : IFtShareMcpToolInvoker
{
    public async Task<JsonElement?> InvokeAsync(
        string toolName,
        IReadOnlyDictionary<string, object?> arguments,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolName);
        ArgumentNullException.ThrowIfNull(arguments);

        var currentOptions = options.Value;
        var endpoint = new Uri(currentOptions.McpEndpoint, UriKind.Absolute);

        Exception? lastTransientException = null;
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                return await InvokeOnceAsync(
                    endpoint,
                    currentOptions,
                    toolName,
                    arguments,
                    cancellationToken);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                lastTransientException = new TimeoutException(
                    "FTShare MCP 工具调用超时。");
            }
            catch (Exception exception) when (IsTransient(exception))
            {
                lastTransientException = exception;
            }

            if (attempt >= currentOptions.MaxRetryCount)
            {
                ExceptionDispatchInfo.Capture(lastTransientException!).Throw();
            }

            await Task.Delay(
                CalculateRetryDelay(currentOptions.RetryDelay, attempt),
                timeProvider,
                cancellationToken);
        }
    }

    private async Task<JsonElement?> InvokeOnceAsync(
        Uri endpoint,
        FtShareOptions options,
        string toolName,
        IReadOnlyDictionary<string, object?> arguments,
        CancellationToken cancellationToken)
    {
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = endpoint,
                TransportMode = HttpTransportMode.StreamableHttp,
                ConnectionTimeout = options.RequestTimeout
            });

        using var timeoutCancellation = new CancellationTokenSource(
            options.RequestTimeout,
            timeProvider);
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutCancellation.Token);

        await using var client = await McpClient.CreateAsync(
            transport,
            cancellationToken: linkedCancellation.Token);

        var result = await client.CallToolAsync(
            toolName,
            arguments,
            progress: null,
            options: null,
            linkedCancellation.Token);

        if (result.IsError == true)
        {
            var message = result.Content
                .OfType<TextContentBlock>()
                .Select(content => content.Text)
                .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text))
                ?? "FTShare MCP 工具调用失败。";

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

    private static bool IsTransient(Exception exception)
        => exception is HttpRequestException or IOException or TimeoutException;

    private static TimeSpan CalculateRetryDelay(TimeSpan baseDelay, int attempt)
    {
        var multiplier = 1 << Math.Min(attempt, 4);
        return TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * multiplier);
    }
}
