using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InvoiceManager.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<InvoiceManagerDbContext>
{
    public InvoiceManagerDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=invoice_manager;Username=invoice_manager;Password=change_me";

        var optionsBuilder = new DbContextOptionsBuilder<InvoiceManagerDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new InvoiceManagerDbContext(optionsBuilder.Options);
    }
}
