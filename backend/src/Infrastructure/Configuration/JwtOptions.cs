namespace InvoiceManager.Infrastructure.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "invoice-manager";
    public string Audience { get; set; } = "invoice-manager-admin";
    public string SigningKey { get; set; } = string.Empty;
    public int LifetimeMinutes { get; set; } = 480;
}
