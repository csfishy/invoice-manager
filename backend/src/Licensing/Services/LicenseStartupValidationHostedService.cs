using InvoiceManager.Application.Licensing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InvoiceManager.Licensing.Services;

public sealed class LicenseStartupValidationHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<LicenseStartupValidationHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var licenseStatusService = scope.ServiceProvider.GetRequiredService<ILicenseStatusService>();
        var status = await licenseStatusService.GetCurrentStatusAsync(cancellationToken);
        logger.LogInformation(
            "Startup license validation completed with status {Status}. Message: {Message}",
            status.Status,
            status.Message);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
