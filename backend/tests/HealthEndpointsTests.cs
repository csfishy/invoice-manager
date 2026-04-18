using System.Net;
using System.Net.Http.Json;
using InvoiceManager.Application.Health;
using InvoiceManager.Application.Licensing;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace InvoiceManager.Tests;

public sealed class HealthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<HealthStatusDto>();
        Assert.NotNull(payload);
        Assert.Equal("ok", payload!.Status);
    }

    [Fact]
    public async Task LicenseStatusEndpoint_ReturnsPayload()
    {
        var response = await _client.GetAsync("/api/license/status");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<LicenseStatusDto>();
        Assert.NotNull(payload);
        Assert.Equal("Scaffold", payload!.Status);
    }
}
