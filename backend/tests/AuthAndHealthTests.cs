using System.Net;
using System.Net.Http.Json;
using InvoiceManager.Application.Auth;
using InvoiceManager.Application.Health;
using InvoiceManager.Tests.Infrastructure;
using Xunit;

namespace InvoiceManager.Tests;

public sealed class AuthAndHealthTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/health");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<HealthStatusDto>(TestJson.Options);
        Assert.NotNull(payload);
        Assert.Equal("ok", payload!.Status);
    }

    [Fact]
    public async Task Login_ReturnsJwtTokenAndRole()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "admin",
            password = "change_me_now"
        });

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<AuthResponseDto>(TestJson.Options);
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.AccessToken));
        Assert.Equal("Admin", payload.Role.ToString());
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/dashboard/summary");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
