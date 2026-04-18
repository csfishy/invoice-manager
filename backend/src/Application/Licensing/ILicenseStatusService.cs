namespace InvoiceManager.Application.Licensing;

public interface ILicenseStatusService
{
    Task<LicenseStatusDto> GetCurrentStatusAsync(CancellationToken cancellationToken = default);
    Task<string> GetCurrentFingerprintHashAsync(CancellationToken cancellationToken = default);
    Task<LicenseStatusDto> ImportLicenseAsync(Stream content, CancellationToken cancellationToken = default);
}
