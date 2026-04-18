using InvoiceManager.Application.Licensing;
using InvoiceManager.Licensing.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceManager.Licensing.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLicensingServices(this IServiceCollection services)
    {
        services.AddSingleton<ILicenseStatusService, DemoLicenseStatusService>();
        return services;
    }
}
