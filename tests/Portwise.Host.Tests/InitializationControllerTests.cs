using Microsoft.AspNetCore.Mvc;
using Moq;
using Portwise.Application.Initialization.Contracts;
using Portwise.Application.Initialization.Dtos;
using Portwise.Controllers;
using Xunit;

namespace Portwise.Host.Tests;

public sealed class InitializationControllerTests
{
    [Fact]
    public async Task Get_ReturnsInitializationStatus()
    {
        var expected = new InitializationStatusResponse(false, null, null, []);
        var application = new Mock<IInitializationAppService>();
        application
            .Setup(item => item.GetAsync(CancellationToken.None))
            .ReturnsAsync(expected);
        var controller = new InitializationController(application.Object);

        var response = await controller.Get(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task Complete_ForwardsRequestAndReturnsResponse()
    {
        var request = new CompleteInitializationRequest(
            "zh-CN",
            "system",
            "组合",
            null,
            null,
            null,
            null);
        var expected = new CompleteInitializationResponse(
            new InitializationStatusResponse(true, DateTimeOffset.UnixEpoch, null, []));
        var application = new Mock<IInitializationAppService>();
        application
            .Setup(item => item.CompleteAsync(request, CancellationToken.None))
            .ReturnsAsync(expected);
        var controller = new InitializationController(application.Object);

        var response = await controller.Complete(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(expected, ok.Value);
    }
}
