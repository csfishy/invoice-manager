using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using InvoiceManager.Application.Licensing;
using InvoiceManager.Licensing.Models;
using InvoiceManager.Tests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace InvoiceManager.Tests;

public sealed class LicensingAndStartupTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task LicenseStatus_WhenNoLicenseImported_IsMissingLicense()
    {
        var auth = await TestAuthHelper.LoginAsAdminAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var payload = await _client.GetFromJsonAsync<LicenseStatusDto>("/api/license/status", TestJson.Options);

        Assert.NotNull(payload);
        Assert.False(payload!.IsValid);
        Assert.Equal("MissingLicense", payload.Status);
        Assert.False(string.IsNullOrWhiteSpace(payload.FingerprintHash));
        Assert.True(payload.RequiresActivation);
    }

    [Fact]
    public async Task LicenseRequestCode_IsReturnedForAdmin()
    {
        var auth = await TestAuthHelper.LoginAsAdminAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var payload = await _client.GetFromJsonAsync<LicenseRequestCodeDto>("/api/license/request-code", TestJson.Options);

        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.RequestCode));
        Assert.False(string.IsNullOrWhiteSpace(payload.FingerprintHash));
    }

    [Fact]
    public async Task ImportingInvalidLicense_ReturnsInvalidSignatureStatus()
    {
        var auth = await TestAuthHelper.LoginAsAdminAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var requestCode = await _client.GetFromJsonAsync<LicenseRequestCodeDto>("/api/license/request-code", TestJson.Options);
        Assert.NotNull(requestCode);

        var invalidLicense = new SignedLicenseDocument
        {
            Version = 1,
            LicenseId = "LIC-INVALID-001",
            CustomerName = "Invalid License Customer",
            ProductName = requestCode!.ProductName,
            IssuedAtUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(30),
            MachineFingerprintHash = requestCode.FingerprintHash,
            BoundAtUtc = DateTime.UtcNow,
            Features = ["Bills", "Dashboard"],
            SignatureBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("invalid-signature"))
        };

        using var formData = new MultipartFormDataContent();
        var json = JsonSerializer.Serialize(invalidLicense);
        formData.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(json)), "file", "invalid-license.json");

        var response = await _client.PostAsync("/api/license/import", formData);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<LicenseStatusDto>(TestJson.Options);
        Assert.NotNull(payload);
        Assert.False(payload!.IsValid);
        Assert.Equal("InvalidSignature", payload.Status);
    }

    [Fact]
    public async Task AuthenticatedBusinessEndpoint_IsRejectedWhenLicenseIsMissing()
    {
        var previousValue = Environment.GetEnvironmentVariable("Licensing__AllowUnlicensedDevelopmentMode");

        try
        {
            Environment.SetEnvironmentVariable("Licensing__AllowUnlicensedDevelopmentMode", "false");

            await using var strictFactory = new TestApiFactory();
            await strictFactory.InitializeAsync();
            using var client = strictFactory.CreateClient();
            var auth = await TestAuthHelper.LoginAsAdminAsync(client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

            var response = await client.GetAsync("/api/dashboard/summary");

            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
        }
        finally
        {
            Environment.SetEnvironmentVariable("Licensing__AllowUnlicensedDevelopmentMode", previousValue);
        }
    }

    [Fact]
    public async Task StartupFails_WhenImportedLicenseIsInvalid()
    {
        var previousProvider = Environment.GetEnvironmentVariable("Database__Provider");
        var previousConnection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        var previousIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer");
        var previousAudience = Environment.GetEnvironmentVariable("Jwt__Audience");
        var previousSigningKey = Environment.GetEnvironmentVariable("Jwt__SigningKey");
        var previousUploads = Environment.GetEnvironmentVariable("Uploads__Path");
        var previousLicensePath = Environment.GetEnvironmentVariable("Licensing__LicenseFilePath");
        var previousSalt = Environment.GetEnvironmentVariable("Licensing__FingerprintSalt");
        var previousProduct = Environment.GetEnvironmentVariable("Licensing__LicensedProductName");
        var previousAllowUnlicensed = Environment.GetEnvironmentVariable("Licensing__AllowUnlicensedDevelopmentMode");

        try
        {
            var databasePath = Path.Combine(Path.GetTempPath(), $"invoice-manager-invalid-license-{Guid.NewGuid():N}.db");
            var uploadsPath = Path.Combine(Path.GetTempPath(), $"invoice-manager-invalid-license-uploads-{Guid.NewGuid():N}");
            var licensePath = Path.Combine(Path.GetTempPath(), $"invoice-manager-invalid-license-{Guid.NewGuid():N}.json");

            Environment.SetEnvironmentVariable("Database__Provider", "Sqlite");
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", $"Data Source={databasePath}");
            Environment.SetEnvironmentVariable("Jwt__Issuer", "test");
            Environment.SetEnvironmentVariable("Jwt__Audience", "test");
            Environment.SetEnvironmentVariable("Jwt__SigningKey", "test-signing-key-12345678901234567890");
            Environment.SetEnvironmentVariable("Uploads__Path", uploadsPath);
            Environment.SetEnvironmentVariable("Licensing__LicenseFilePath", licensePath);
            Environment.SetEnvironmentVariable("Licensing__FingerprintSalt", "invalid-license-startup-salt");
            Environment.SetEnvironmentVariable("Licensing__LicensedProductName", "Invoice Manager");
            Environment.SetEnvironmentVariable("Licensing__AllowUnlicensedDevelopmentMode", "false");

            Directory.CreateDirectory(uploadsPath);
            await File.WriteAllTextAsync(licensePath, """
                {
                  "version": 1,
                  "licenseId": "LIC-STARTUP-INVALID",
                  "customerName": "Broken Startup Customer",
                  "productName": "Invoice Manager",
                  "issuedAtUtc": "2026-04-18T00:00:00Z",
                  "expiresAtUtc": "2026-12-31T00:00:00Z",
                  "machineFingerprintHash": "not-the-current-machine",
                  "boundAtUtc": "2026-04-18T00:00:00Z",
                  "features": ["Bills"],
                  "signature": "aW52YWxpZA=="
                }
                """);

            var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Production");
            });

            var exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                using var client = factory.CreateClient();
                await client.GetAsync("/api/health");
            });

            Assert.Contains("invalid license", exception.ToString(), StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Environment.SetEnvironmentVariable("Database__Provider", previousProvider);
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", previousConnection);
            Environment.SetEnvironmentVariable("Jwt__Issuer", previousIssuer);
            Environment.SetEnvironmentVariable("Jwt__Audience", previousAudience);
            Environment.SetEnvironmentVariable("Jwt__SigningKey", previousSigningKey);
            Environment.SetEnvironmentVariable("Uploads__Path", previousUploads);
            Environment.SetEnvironmentVariable("Licensing__LicenseFilePath", previousLicensePath);
            Environment.SetEnvironmentVariable("Licensing__FingerprintSalt", previousSalt);
            Environment.SetEnvironmentVariable("Licensing__LicensedProductName", previousProduct);
            Environment.SetEnvironmentVariable("Licensing__AllowUnlicensedDevelopmentMode", previousAllowUnlicensed);
        }
    }

    [Fact]
    public async Task StartupFails_WhenJwtSigningKeyIsMissing()
    {
        var previousProvider = Environment.GetEnvironmentVariable("Database__Provider");
        var previousConnection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        var previousIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer");
        var previousAudience = Environment.GetEnvironmentVariable("Jwt__Audience");
        var previousSigningKey = Environment.GetEnvironmentVariable("Jwt__SigningKey");
        var previousUploads = Environment.GetEnvironmentVariable("Uploads__Path");
        var previousLicensePath = Environment.GetEnvironmentVariable("Licensing__LicenseFilePath");
        var previousSalt = Environment.GetEnvironmentVariable("Licensing__FingerprintSalt");
        var previousProduct = Environment.GetEnvironmentVariable("Licensing__LicensedProductName");

        try
        {
            var databasePath = Path.Combine(Path.GetTempPath(), $"invoice-manager-broken-{Guid.NewGuid():N}.db");
            Environment.SetEnvironmentVariable("Database__Provider", "Sqlite");
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", $"Data Source={databasePath}");
            Environment.SetEnvironmentVariable("Jwt__Issuer", "test");
            Environment.SetEnvironmentVariable("Jwt__Audience", "test");
            Environment.SetEnvironmentVariable("Jwt__SigningKey", "");
            Environment.SetEnvironmentVariable("Uploads__Path", Path.Combine(Path.GetTempPath(), "invoice-manager-broken-uploads"));
            Environment.SetEnvironmentVariable("Licensing__LicenseFilePath", Path.Combine(Path.GetTempPath(), "invoice-manager-broken-license.json"));
            Environment.SetEnvironmentVariable("Licensing__FingerprintSalt", "broken-salt");
            Environment.SetEnvironmentVariable("Licensing__LicensedProductName", "Invoice Manager");

            var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
            });

            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                using var client = factory.CreateClient();
                await client.GetAsync("/api/health");
            });
        }
        finally
        {
            Environment.SetEnvironmentVariable("Database__Provider", previousProvider);
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", previousConnection);
            Environment.SetEnvironmentVariable("Jwt__Issuer", previousIssuer);
            Environment.SetEnvironmentVariable("Jwt__Audience", previousAudience);
            Environment.SetEnvironmentVariable("Jwt__SigningKey", previousSigningKey);
            Environment.SetEnvironmentVariable("Uploads__Path", previousUploads);
            Environment.SetEnvironmentVariable("Licensing__LicenseFilePath", previousLicensePath);
            Environment.SetEnvironmentVariable("Licensing__FingerprintSalt", previousSalt);
            Environment.SetEnvironmentVariable("Licensing__LicensedProductName", previousProduct);
        }
    }
}
