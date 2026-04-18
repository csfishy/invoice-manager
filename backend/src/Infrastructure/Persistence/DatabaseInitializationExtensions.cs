using InvoiceManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceManager.Infrastructure.Persistence;

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InvoiceManagerDbContext>();

        if (dbContext.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true)
        {
            await dbContext.Database.EnsureCreatedAsync();
        }
        else
        {
            await dbContext.Database.MigrateAsync();
        }

        var seeder = scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>();
        await seeder.SeedAsync();
    }
}
