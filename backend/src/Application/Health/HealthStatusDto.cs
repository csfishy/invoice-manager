namespace InvoiceManager.Application.Health;

public sealed record HealthStatusDto(string Status, string Service, DateTime UtcTime);
