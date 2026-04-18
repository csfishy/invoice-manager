namespace InvoiceManager.Application.Licensing;

public sealed record LicenseRequestCodeDto(
    string RequestCode,
    string FingerprintHash,
    string ProductName,
    string MachineName,
    DateTime GeneratedAtUtc,
    string Format,
    string Message);
