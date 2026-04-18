namespace InvoiceManager.Application.Licensing;

public sealed record LicenseFingerprintDto(string FingerprintHash, DateTime GeneratedAtUtc);
