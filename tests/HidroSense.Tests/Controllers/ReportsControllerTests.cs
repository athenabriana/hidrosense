using HidroSense.Api.ViewModels;
using System.Net.Http.Json;

namespace HidroSense.Tests.Controllers;

public class ReportsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    public ReportsControllerTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_ReturnsHttpStatusCode200()
    {
        var client = await _factory.AuthenticatedClientAsync();
        var request = "/api/v1/reports/water-impact";

        var response = await client.GetAsync(request);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Get_ReturnsRegionAggregation()
    {
        var client = await _factory.AuthenticatedClientAsync();
        var report = await client.GetFromJsonAsync<WaterImpactReportViewModel>(
            "/api/v1/reports/water-impact");

        Assert.NotNull(report);
        Assert.NotEmpty(report!.Regions);
    }
}
