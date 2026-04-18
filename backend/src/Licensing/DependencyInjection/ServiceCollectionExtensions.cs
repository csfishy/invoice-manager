using InvoiceManager.Application.Licensing;
using InvoiceManager.Licensing.Configuration;
using InvoiceManager.Licensing.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceManager.Licensing.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLicensingServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<LicensingOptions>()
            .Bind(configuration.GetSection(LicensingOptions.SectionName))
            .Validate(x => !string.IsNullOrWhiteSpace(x.LicenseFilePath), "License file path is required.")
            .Validate(x => !string.IsNullOrWhiteSpace(x.FingerprintSalt), "Fingerprint salt is required.")
            .ValidateOnStart();

        services.AddSingleton<MachineFingerprintService>();
        services.AddSingleton<ILicenseSignatureVerifier, RsaLicenseSignatureVerifier>();
        services.AddSingleton<ILicenseStatusService, LicenseStatusService>();
        services.AddHostedService<LicenseStartupValidationHostedService>();
        return services;
    }
}
