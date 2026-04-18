namespace InvoiceManager.Application.Licensing;

public sealed record LicenseStatusDto(
    string Status,
    bool IsValid,
    string FingerprintHash,
    string? LicenseId,
    string? CustomerName,
    DateTime? IssuedAtUtc,
    DateTime? ExpiresAtUtc,
    IReadOnlyCollection<string> Features,
    DateTime CheckedAtUtc,
    string Message);
