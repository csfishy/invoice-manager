namespace InvoiceManager.Licensing.Models;

public sealed class LicenseRequestPayload
{
    public int Version { get; set; } = 1;
    public string ProductName { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string MachineFingerprintHash { get; set; } = string.Empty;
    public DateTime GeneratedAtUtc { get; set; }
}
