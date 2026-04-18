using InvoiceManager.Application.Health;
using InvoiceManager.Application.Licensing;
using InvoiceManager.Infrastructure.DependencyInjection;
using InvoiceManager.Licensing.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureServices();
builder.Services.AddLicensingServices();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/api/health", () =>
{
    return Results.Ok(new HealthStatusDto("ok", "invoice-manager-api", DateTime.UtcNow));
});

app.MapGet("/api/health/license", async (ILicenseStatusService licenseStatusService, CancellationToken cancellationToken) =>
{
    var status = await licenseStatusService.GetCurrentStatusAsync(cancellationToken);
    return Results.Ok(status);
});

app.MapGet("/api/license/status", async (ILicenseStatusService licenseStatusService, CancellationToken cancellationToken) =>
{
    var status = await licenseStatusService.GetCurrentStatusAsync(cancellationToken);
    return Results.Ok(status);
});

app.Run();

public partial class Program;
