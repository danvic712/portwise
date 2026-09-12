using Microsoft.AspNetCore.Mvc;
using Moq;
using Portwise.Application.Inference.Contracts;
using Portwise.Application.Inference.Dtos;
using Portwise.Controllers;
using Xunit;

namespace Portwise.Host.Tests;

public sealed class InferenceControllerTests
{
    [Fact]
    public async Task GetProviders_ReturnsApplicationResponse()
    {
        var expected = new InferenceProvidersResponse([]);
        var application = new Mock<IInferenceConfigurationAppService>();
        application
            .Setup(service => service.GetProvidersAsync(CancellationToken.None))
            .ReturnsAsync(expected);
        var controller = new InferenceController(application.Object);

        var response = await controller.GetProviders(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task UpdateRoutes_ForwardsRequestAndReturnsApplicationResponse()
    {
        var request = new UpdateInferenceRoutesRequest([]);
        var expected = new InferenceRoutesResponse([]);
        var application = new Mock<IInferenceConfigurationAppService>();
        application
            .Setup(service => service.UpdateRoutesAsync(request, CancellationToken.None))
            .ReturnsAsync(expected);
        var controller = new InferenceController(application.Object);

        var response = await controller.UpdateRoutes(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(expected, ok.Value);
    }
}
