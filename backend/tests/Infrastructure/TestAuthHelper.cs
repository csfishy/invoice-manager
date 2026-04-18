using System.Net.Http.Json;
using InvoiceManager.Application.Auth;

namespace InvoiceManager.Tests.Infrastructure;

internal static class TestAuthHelper
{
    public static async Task<AuthResponseDto> LoginAsAdminAsync(HttpClient client)
        => await LoginAsync(client, "admin", "change_me_now");

    public static async Task<AuthResponseDto> LoginAsOperatorAsync(HttpClient client)
        => await LoginAsync(client, "operator", "operator123!");

    public static async Task<AuthResponseDto> LoginAsViewerAsync(HttpClient client)
        => await LoginAsync(client, "viewer", "viewer123!");

    private static async Task<AuthResponseDto> LoginAsync(HttpClient client, string username, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username,
            password
        });

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AuthResponseDto>(TestJson.Options);
        return payload ?? throw new InvalidOperationException("Missing auth payload.");
    }
}
