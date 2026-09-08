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
}
