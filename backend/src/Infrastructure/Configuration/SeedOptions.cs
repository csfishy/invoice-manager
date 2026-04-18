namespace InvoiceManager.Infrastructure.Configuration;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string DefaultAdminUsername { get; set; } = "admin";
    public string DefaultAdminPassword { get; set; } = "change_me_now";
}
