using System.Net.Http.Json;
using HidroSense.Api.ViewModels;

namespace HidroSense.Tests.Controllers;

public class ConsumptionControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    public ConsumptionControllerTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_ReturnsHttpStatusCode200()
    {
        var client = await _factory.AuthenticatedClientAsync();
        var request = "/api/v1/consumption/readings";

        var response = await client.GetAsync(request);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Post_HighFlowReading_RaisesLeakAlert()
    {
        var client = await _factory.AuthenticatedClientAsync();

        var payload = new CreateConsumptionReadingViewModel
        {
            ConsumerUnitId = 1,
            VolumeLiters = 200,
            FlowRateLpm = 6.0
        };

        var response = await client.PostAsJsonAsync("/api/v1/consumption/readings", payload);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ConsumptionReadingResultViewModel>();
        Assert.True(result!.LeakDetected);
        Assert.NotNull(result.LeakAlertId);
    }
}
