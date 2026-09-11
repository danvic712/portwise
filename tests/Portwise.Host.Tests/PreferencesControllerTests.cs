using Microsoft.AspNetCore.Mvc;
using Moq;
using Portwise.Application.Preferences.Contracts;
using Portwise.Application.Preferences.Dtos;
using Portwise.Controllers;
using Xunit;

namespace Portwise.Host.Tests;

public sealed class PreferencesControllerTests
{
    [Fact]
    public async Task Get_ReturnsApplicationServiceResponse()
    {
        var expected = new PreferencesResponse(
            "zh-CN",
            "system",
            1,
            DateTimeOffset.UnixEpoch);
        var application = new Mock<IPreferencesAppService>();
        application
            .Setup(x => x.GetAsync(CancellationToken.None))
            .ReturnsAsync(expected);
        var controller = new PreferencesController(application.Object);

        var response = await controller.Get(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task Update_ForwardsRequestAndReturnsApplicationServiceResponse()
    {
        var request = new UpdatePreferencesRequest("en-US", "dark", 1);
        var expected = new PreferencesResponse(
            "en-US",
            "dark",
            2,
            DateTimeOffset.UnixEpoch);
        var application = new Mock<IPreferencesAppService>();
        application
            .Setup(x => x.UpdateAsync(request, CancellationToken.None))
            .ReturnsAsync(expected);
        var controller = new PreferencesController(application.Object);

        var response = await controller.Update(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(expected, ok.Value);
    }
}
