namespace InvoiceManager.Application.Licensing;

public sealed record LicenseStatusDto(
    string Status,
    bool IsValid,
    string FingerprintHash,
    DateTime CheckedAtUtc,
    string Message);
