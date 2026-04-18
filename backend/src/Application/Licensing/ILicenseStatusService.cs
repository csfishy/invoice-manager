namespace InvoiceManager.Application.Licensing;

public interface ILicenseStatusService
{
    Task<LicenseStatusDto> GetCurrentStatusAsync(CancellationToken cancellationToken = default);
}
