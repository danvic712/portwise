using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ModelContextProtocol;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Portwise.Infrastructure.Contracts;
using Portwise.Infrastructure.FtShare;
using Xunit;

namespace Portwise.Infrastructure.Tests;

public sealed class FtShareHttpClientRegistrationTests
{
    [Fact]
    public void NamedClientCanBeCreatedWithTheDefaultAttemptTimeout()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] =
                    "Host=localhost;Port=5432;Database=portwise;Username=portwise;Password=portwise"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddPortwiseInfrastructure(configuration);

        using var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient(FtShareMcpToolInvoker.HttpClientName);

        Assert.Equal(Timeout.InfiniteTimeSpan, client.Timeout);
    }

    [Fact]
    public async Task ResponseStreamHandlerClassifiesBodyReadFailures()
    {
        using var client = new HttpClient(
            new FtShareResponseStreamHandler
            {
                InnerHandler = new SequenceHandler(() => new HttpResponseMessage
                {
                    Content = new StreamContent(new ThrowingReadStream())
                })
            });

        using var response = await client.GetAsync(
            "https://market.example/mcp",
            HttpCompletionOption.ResponseHeadersRead);
        await using var stream = await response.Content.ReadAsStreamAsync();

        await Assert.ThrowsAsync<FtShareResponseStreamException>(async () =>
        {
            await stream.ReadExactlyAsync(new byte[8]);
        });
    }

    [Fact]
    public async Task InvokerRetriesTheCompleteExchangeWhenResponseStreamIsInterrupted()
    {
        var handler = new McpScriptHandler((method, invocation) =>
        {
            if (method == "tools/call" && invocation == 1)
            {
                return CreateJsonResponse(new ThrowingReadStreamContent());
            }

            return method switch
            {
                "server/discover" => CreateJsonResponse(
                    "{\"supportedVersions\":[\"2026-07-28\"],\"capabilities\":{}}"),
                "tools/call" => CreateJsonResponse(
                    "{\"content\":[{\"type\":\"text\",\"text\":\"ok\"}],\"structuredContent\":{\"value\":42}}"),
                _ => new HttpResponseMessage(HttpStatusCode.NoContent)
            };
        });
        using var serviceProvider = BuildServiceProvider(handler, maxRetryCount: 1);
        using var scope = serviceProvider.CreateScope();
        var invoker = scope.ServiceProvider.GetRequiredService<IFtShareMcpToolInvoker>();
        var options = CreateOptions(maxRetryCount: 1);

        var result = await invoker.InvokeAsync(
            options,
            "get_stock_profile",
            new Dictionary<string, object?> { ["security_code"] = "600000" },
            CancellationToken.None);

        Assert.True(result.HasValue);
        Assert.Equal(42, result.Value.GetProperty("value").GetInt32());
        Assert.Equal(2, handler.Count("server/discover"));
        Assert.Equal(2, handler.Count("tools/call"));
        Assert.All(handler.AuthorizationHeaders, header => Assert.Equal("Bearer test-key", header));
    }

    [Fact]
    public async Task InvokerDoesNotRetryMcpBusinessErrors()
    {
        var handler = new McpScriptHandler((method, _) => method switch
        {
            "server/discover" => CreateJsonResponse(
                "{\"supportedVersions\":[\"2026-07-28\"],\"capabilities\":{}}"),
            "tools/call" => CreateJsonResponse(
                "{\"isError\":true,\"content\":[{\"type\":\"text\",\"text\":\"business failure\"}]}"),
            _ => new HttpResponseMessage(HttpStatusCode.NoContent)
        });
        using var serviceProvider = BuildServiceProvider(handler, maxRetryCount: 2);
        using var scope = serviceProvider.CreateScope();
        var invoker = scope.ServiceProvider.GetRequiredService<IFtShareMcpToolInvoker>();
        var options = CreateOptions(maxRetryCount: 2);

        var exception = await Assert.ThrowsAsync<McpException>(() => invoker.InvokeAsync(
            options,
            "get_stock_profile",
            new Dictionary<string, object?>(),
            CancellationToken.None));

        Assert.Equal("business failure", exception.Message);
        Assert.Equal(1, handler.Count("tools/call"));
    }

    private static FtShareOptions CreateOptions(int maxRetryCount) => new()
    {
        McpEndpoint = "https://market.example/mcp",
        ApiKey = "test-key",
        RequestTimeoutSeconds = 5,
        MaxRetryCount = maxRetryCount,
        RetryDelayMilliseconds = 0
    };

    private static ServiceProvider BuildServiceProvider(
        HttpMessageHandler handler,
        int maxRetryCount)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] =
                    "Host=localhost;Port=5432;Database=portwise;Username=portwise;Password=portwise"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton(TimeProvider.System);
        services.AddPortwiseInfrastructure(configuration);
        services
            .AddHttpClient(FtShareMcpToolInvoker.HttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        return services.BuildServiceProvider();
    }

    private static HttpResponseMessage CreateJsonResponse(string json)
        => new(HttpStatusCode.OK)
        {
            Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json")
        };

    private static HttpResponseMessage CreateJsonResponse(HttpContent content)
    {
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
    }

    private sealed class SequenceHandler(Func<HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(responseFactory());
    }

    private sealed class ThrowingReadStream : Stream
    {
        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count)
            => throw new IOException("connection reset");

        public override int Read(Span<byte> buffer)
            => throw new IOException("connection reset");

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
            => ValueTask.FromException<int>(new IOException("connection reset"));

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();
    }

    private sealed class ThrowingReadStreamContent : HttpContent
    {
        protected override Task SerializeToStreamAsync(
            Stream stream,
            TransportContext? context)
            => throw new IOException("connection reset");

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }

    private sealed class McpScriptHandler(
        Func<string, int, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        private readonly Dictionary<string, int> invocationCounts = new(StringComparer.Ordinal);

        public List<string?> AuthorizationHeaders { get; } = [];

        public int Count(string method)
            => invocationCounts.TryGetValue(method, out var count) ? count : 0;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            AuthorizationHeaders.Add(request.Headers.Authorization?.ToString());
            var requestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(requestBody);
            var method = document.RootElement.GetProperty("method").GetString()!;
            invocationCounts[method] = Count(method) + 1;
            var response = responseFactory(method, invocationCounts[method]);
            if (response.Content is not null)
            {
                var requestId = document.RootElement.TryGetProperty("id", out var id)
                    ? id.GetRawText()
                    : null;
                if (requestId is not null && response.Content is StringContent stringContent)
                {
                    var result = await stringContent.ReadAsStringAsync(cancellationToken);
                    stringContent.Dispose();
                    response.Content = new StringContent(
                        $"{{\"jsonrpc\":\"2.0\",\"id\":{requestId},\"result\":{result}}}",
                        Encoding.UTF8,
                        "application/json");
                }
            }

            return response;
        }
    }
}
