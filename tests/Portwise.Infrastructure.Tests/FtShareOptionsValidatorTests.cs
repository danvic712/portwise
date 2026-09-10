using Portwise.Infrastructure.FtShare;
using Xunit;

namespace Portwise.Infrastructure.Tests;

public sealed class FtShareOptionsValidatorTests
{
    [Fact]
    public void Validate_RejectsInvalidEndpointAndLimits()
    {
        var result = new FtShareOptionsValidator().Validate(
            null,
            new FtShareOptions
            {
                McpEndpoint = "ftp://market.example",
                RequestTimeoutSeconds = 0,
                MaxRetryCount = 6,
                RetryDelayMilliseconds = 10_001
            });

        Assert.True(result.Failed);
        Assert.Equal(4, result.Failures.Count());
    }

    [Fact]
    public void Validate_AcceptsConfiguredEndpointAndLimits()
    {
        var result = new FtShareOptionsValidator().Validate(
            null,
            new FtShareOptions
            {
                McpEndpoint = "https://market.example"
            });

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void OperationTimeoutCoversConfiguredHttpAttemptBudget()
    {
        var options = new FtShareOptions
        {
            McpEndpoint = "https://market.example",
            RequestTimeoutSeconds = 5,
            MaxRetryCount = 2,
            RetryDelayMilliseconds = 10_000
        };

        Assert.Equal(TimeSpan.FromSeconds(15), options.HttpRequestTimeout);
        Assert.Equal(TimeSpan.FromSeconds(60), options.OperationTimeout);
    }
}
