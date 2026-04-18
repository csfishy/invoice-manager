using System.Text.Json.Serialization;

namespace InvoiceManager.Licensing.Models;

public sealed class SignedLicenseDocument
{
    public int Version { get; set; } = 1;
    public string LicenseId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public DateTime IssuedAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public string MachineFingerprintHash { get; set; } = string.Empty;
    public DateTime BoundAtUtc { get; set; }
    public List<string> Features { get; set; } = new();

    [JsonPropertyName("signature")]
    public string SignatureBase64 { get; set; } = string.Empty;
}
