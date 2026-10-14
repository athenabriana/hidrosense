using System.Net.Http.Headers;
using System.Net.Http.Json;
using HidroSense.Api.ViewModels;

namespace HidroSense.Tests;

internal static class AuthHelper
{
    public static async Task<HttpClient> AuthenticatedClientAsync(
        this CustomWebApplicationFactory factory, string username = "admin", string password = "Admin@123")
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginViewModel { Username = username, Password = password });
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<TokenViewModel>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token!.AccessToken);
        return client;
    }
}
