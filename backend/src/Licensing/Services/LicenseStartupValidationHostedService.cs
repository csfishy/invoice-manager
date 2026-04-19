using InvoiceManager.Application.Licensing;
using InvoiceManager.Licensing.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InvoiceManager.Licensing.Services;

public sealed class LicenseStartupValidationHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<LicensingOptions> options,
    IHostEnvironment hostEnvironment,
    ILogger<LicenseStartupValidationHostedService> logger) : IHostedService
{
    private readonly LicensingOptions _options = options.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var licenseStatusService = scope.ServiceProvider.GetRequiredService<ILicenseStatusService>();
        var status = await licenseStatusService.GetCurrentStatusAsync(cancellationToken);
        logger.LogInformation(
            "Startup license validation completed with status {Status}. Message: {Message}",
            status.Status,
            status.Message);

        if (_options.AllowUnlicensedDevelopmentMode && hostEnvironment.IsDevelopment())
        {
            return;
        }

        if (File.Exists(_options.LicenseFilePath) && !status.IsValid)
        {
            throw new InvalidOperationException($"Startup rejected due to invalid license state '{status.Status}': {status.Message}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
