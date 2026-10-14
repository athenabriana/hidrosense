using System.Net;
using System.Net.Http.Json;
using HidroSense.Api.ViewModels;

namespace HidroSense.Tests.Controllers;

public class LeakAlertsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    public LeakAlertsControllerTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_ReturnsHttpStatusCode200()
    {
        var client = await _factory.AuthenticatedClientAsync();
        var request = "/api/v1/leak-alerts";

        var response = await client.GetAsync(request);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Resolve_AsOperator_IsForbidden()
    {
        var client = await _factory.AuthenticatedClientAsync("operator", "Operator@123");
        var response = await client.PatchAsJsonAsync("/api/v1/leak-alerts/1/resolve",
            new ResolveLeakAlertViewModel { Note = "tentativa" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
