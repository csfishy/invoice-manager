namespace InvoiceManager.Licensing.Configuration;

public sealed class LicensingOptions
{
    public const string SectionName = "Licensing";

    public string LicenseFilePath { get; set; } = "/app/data/license/license.json";
    public string FingerprintSalt { get; set; } = string.Empty;
    public string LicensedProductName { get; set; } = "Invoice Manager";
}
