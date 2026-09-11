using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ModelContextProtocol;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Portwise.Infrastructure.Contracts;
using Portwise.Infrastructure.FtShare;
using Polly.Registry;
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
                    "Host=localhost;Port=5432;Database=portwise;Username=portwise;Password=portwise",
                ["FtShare:McpEndpoint"] = "https://market.example",
                ["FtShare:RequestTimeoutSeconds"] = "30",
                ["FtShare:MaxRetryCount"] = "2",
                ["FtShare:RetryDelayMilliseconds"] = "250"
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
    public async Task NamedClientUsesFactoryAndRetriesTransientResponses()
    {
        var attempts = 0;
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] =
                    "Host=localhost;Port=5432;Database=portwise;Username=portwise;Password=portwise",
                ["FtShare:McpEndpoint"] = "https://market.example",
                ["FtShare:RequestTimeoutSeconds"] = "5",
                ["FtShare:MaxRetryCount"] = "1",
                ["FtShare:RetryDelayMilliseconds"] = "0"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton(TimeProvider.System);
        services.AddPortwiseInfrastructure(configuration);
        services
            .AddHttpClient(FtShareMcpToolInvoker.HttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => new SequenceHandler(() =>
                ++attempts == 1
                    ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                    : new HttpResponseMessage(HttpStatusCode.OK)));

        using var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient(FtShareMcpToolInvoker.HttpClientName);

        using var response = await client.PostAsync(
            "https://market.example/mcp",
            content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, attempts);
        Assert.Equal(Timeout.InfiniteTimeSpan, client.Timeout);
    }

    [Fact]
    public async Task NamedClientDoesNotRetryNonTransientResponses()
    {
        var attempts = 0;
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] =
                    "Host=localhost;Port=5432;Database=portwise;Username=portwise;Password=portwise",
                ["FtShare:McpEndpoint"] = "https://market.example",
                ["FtShare:RequestTimeoutSeconds"] = "5",
                ["FtShare:MaxRetryCount"] = "2",
                ["FtShare:RetryDelayMilliseconds"] = "0"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddPortwiseInfrastructure(configuration);
        services
            .AddHttpClient(FtShareMcpToolInvoker.HttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => new SequenceHandler(() =>
            {
                attempts++;
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }));

        using var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient(FtShareMcpToolInvoker.HttpClientName);

        using var response = await client.PostAsync(
            "https://market.example/mcp",
            content: null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(1, attempts);
    }

    [Fact]
    public async Task ExchangePipelineRetriesClassifiedResponseStreamFailures()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] =
                    "Host=localhost;Port=5432;Database=portwise;Username=portwise;Password=portwise",
                ["FtShare:McpEndpoint"] = "https://market.example",
                ["FtShare:MaxRetryCount"] = "1",
                ["FtShare:RetryDelayMilliseconds"] = "0"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddPortwiseInfrastructure(configuration);

        using var serviceProvider = services.BuildServiceProvider();
        var pipeline = serviceProvider
            .GetRequiredService<ResiliencePipelineProvider<string>>()
            .GetPipeline(FtShareMcpToolInvoker.ExchangePipelineName);
        var attempts = 0;

        var result = await pipeline.ExecuteAsync(_ =>
        {
            if (++attempts == 1)
            {
                throw new FtShareResponseStreamException(
                    "stream interrupted",
                    new IOException("connection reset"));
            }

            return new ValueTask<string>("ok");
        });

        Assert.Equal("ok", result);
        Assert.Equal(2, attempts);
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

        var result = await invoker.InvokeAsync(
            "get_stock_profile",
            new Dictionary<string, object?> { ["security_code"] = "600000" },
            CancellationToken.None);

        Assert.True(result.HasValue);
        Assert.Equal(42, result.Value.GetProperty("value").GetInt32());
        Assert.Equal(2, handler.Count("server/discover"));
        Assert.Equal(2, handler.Count("tools/call"));
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

        var exception = await Assert.ThrowsAsync<McpException>(() => invoker.InvokeAsync(
            "get_stock_profile",
            new Dictionary<string, object?>(),
            CancellationToken.None));

        Assert.Equal("business failure", exception.Message);
        Assert.Equal(1, handler.Count("tools/call"));
    }

    private static ServiceProvider BuildServiceProvider(
        HttpMessageHandler handler,
        int maxRetryCount)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] =
                    "Host=localhost;Port=5432;Database=portwise;Username=portwise;Password=portwise",
                ["FtShare:McpEndpoint"] = "https://market.example/mcp",
                ["FtShare:RequestTimeoutSeconds"] = "5",
                ["FtShare:MaxRetryCount"] = maxRetryCount.ToString(),
                ["FtShare:RetryDelayMilliseconds"] = "0"
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

        public int Count(string method)
            => invocationCounts.TryGetValue(method, out var count) ? count : 0;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
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
