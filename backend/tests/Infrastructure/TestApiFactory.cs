using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace InvoiceManager.Tests.Infrastructure;

public sealed class TestApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private string? _databasePath;
    private string? _uploadsPath;
    private string? _licensePath;
    private readonly Dictionary<string, string?> _previousEnvironmentValues = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, _) => { });
    }

    public Task InitializeAsync()
    {
        _databasePath ??= Path.Combine(Path.GetTempPath(), $"invoice-manager-tests-{Guid.NewGuid():N}.db");
        _uploadsPath ??= Path.Combine(Path.GetTempPath(), $"invoice-manager-tests-uploads-{Guid.NewGuid():N}");
        _licensePath ??= Path.Combine(Path.GetTempPath(), $"invoice-manager-tests-license-{Guid.NewGuid():N}.json");

        SetEnvironmentVariable("Database__Provider", "Sqlite");
        SetEnvironmentVariable("ConnectionStrings__DefaultConnection", $"Data Source={_databasePath}");
        SetEnvironmentVariable("Jwt__Issuer", "invoice-manager-tests");
        SetEnvironmentVariable("Jwt__Audience", "invoice-manager-tests");
        SetEnvironmentVariable("Jwt__SigningKey", "integration-tests-signing-key-1234567890");
        SetEnvironmentVariable("Jwt__LifetimeMinutes", "120");
        SetEnvironmentVariable("Seed__DefaultAdminUsername", "admin");
        SetEnvironmentVariable("Seed__DefaultAdminPassword", "change_me_now");
        SetEnvironmentVariable("Uploads__Path", _uploadsPath);
        SetEnvironmentVariable("Licensing__LicenseFilePath", _licensePath);
        SetEnvironmentVariable("Licensing__FingerprintSalt", "integration-test-salt");
        SetEnvironmentVariable("Licensing__LicensedProductName", "Invoice Manager");
        SetEnvironmentVariable("Licensing__AllowUnlicensedDevelopmentMode", Environment.GetEnvironmentVariable("Licensing__AllowUnlicensedDevelopmentMode") ?? "true");
        SetEnvironmentVariable("Cors__AllowedOrigins", "http://localhost:3000");

        return Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();

        foreach (var pair in _previousEnvironmentValues)
        {
            Environment.SetEnvironmentVariable(pair.Key, pair.Value);
        }

        if (!string.IsNullOrWhiteSpace(_databasePath) && File.Exists(_databasePath))
        {
            TryDeleteFile(_databasePath);
        }

        if (!string.IsNullOrWhiteSpace(_uploadsPath) && Directory.Exists(_uploadsPath))
        {
            Directory.Delete(_uploadsPath, true);
        }

        if (!string.IsNullOrWhiteSpace(_licensePath))
        {
            var directory = Path.GetDirectoryName(_licensePath);
            if (File.Exists(_licensePath))
            {
                TryDeleteFile(_licensePath);
            }

            if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory) && !Directory.EnumerateFileSystemEntries(directory).Any())
            {
                TryDeleteDirectory(directory);
            }
        }
    }

    private void SetEnvironmentVariable(string key, string value)
    {
        if (!_previousEnvironmentValues.ContainsKey(key))
        {
            _previousEnvironmentValues[key] = Environment.GetEnvironmentVariable(key);
        }

        Environment.SetEnvironmentVariable(key, value);
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            Directory.Delete(path, true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
