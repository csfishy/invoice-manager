namespace InvoiceManager.Domain.Licensing;

public sealed class LicenseBinding
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string LicenseId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string MachineFingerprintHash { get; set; } = string.Empty;
    public string BindingStatus { get; set; } = string.Empty;
    public DateTime BoundAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAtUtc { get; set; }
    public DateTime? LastValidatedAtUtc { get; set; }
    public string FeaturesJson { get; set; } = "[]";
}
