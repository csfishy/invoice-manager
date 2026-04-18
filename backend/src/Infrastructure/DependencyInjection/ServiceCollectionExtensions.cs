using InvoiceManager.Application.Services;
using InvoiceManager.Domain.Users;
using InvoiceManager.Infrastructure.Auth;
using InvoiceManager.Infrastructure.Configuration;
using InvoiceManager.Infrastructure.Persistence;
using InvoiceManager.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceManager.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(x => !string.IsNullOrWhiteSpace(x.SigningKey), "JWT signing key is required.")
            .ValidateOnStart();

        services.AddOptions<SeedOptions>()
            .Bind(configuration.GetSection(SeedOptions.SectionName));

        services.AddOptions<StorageOptions>()
            .Bind(configuration.GetSection(StorageOptions.SectionName))
            .Validate(x => !string.IsNullOrWhiteSpace(x.Path), "Uploads path is required.")
            .ValidateOnStart();

        services.AddOptions<AttachmentOptions>()
            .Bind(configuration.GetSection(AttachmentOptions.SectionName))
            .Validate(x => x.MaxFileSizeBytes > 0, "Attachment max file size must be greater than zero.")
            .Validate(x => !string.IsNullOrWhiteSpace(x.AllowedExtensions), "Allowed attachment extensions are required.")
            .ValidateOnStart();

        services.AddOptions<CorsOptions>()
            .Bind(configuration.GetSection(CorsOptions.SectionName));

        var provider = configuration["Database:Provider"] ?? "Postgres";
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<InvoiceManagerDbContext>(options =>
        {
            if (string.Equals(provider, "Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
                return;
            }

            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBillService, BillService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
        services.AddSingleton<IFileStorageService, FileStorageService>();
        services.AddScoped<DevelopmentDataSeeder>();

        return services;
    }
}
