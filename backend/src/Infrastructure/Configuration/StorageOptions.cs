namespace InvoiceManager.Infrastructure.Configuration;

public sealed class StorageOptions
{
    public const string SectionName = "Uploads";

    public string Path { get; set; } = "/app/data/uploads";
}
