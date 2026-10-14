using System.Net;
using System.Net.Http.Json;
using HidroSense.Api.Models;
using HidroSense.Api.ViewModels;

namespace HidroSense.Tests.Controllers;

public class ConsumerUnitsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    public ConsumerUnitsControllerTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_ReturnsHttpStatusCode200()
    {
        var client = await _factory.AuthenticatedClientAsync();
        var request = "/api/v1/consumer-units";

        var response = await client.GetAsync(request);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Get_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/consumer-units");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Post_AsAdmin_CreatesUnitAndReturns201()
    {
        var client = await _factory.AuthenticatedClientAsync();
        var payload = new CreateConsumerUnitViewModel
        {
            Code = $"TST-{Guid.NewGuid():N}".Substring(0, 10),
            Name = "Unidade de Teste",
            Type = ConsumerUnitType.Commercial,
            Region = "Zona Teste",
            IdleFlowThresholdLpm = 1.0,
            MonthlyBudgetLiters = 50000
        };

        var response = await client.PostAsJsonAsync("/api/v1/consumer-units", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
