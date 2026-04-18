using InvoiceManager.Application.Licensing;
using Microsoft.Extensions.Configuration;

namespace InvoiceManager.Licensing.Services;

public sealed class DemoLicenseStatusService(IConfiguration configuration) : ILicenseStatusService
{
    public Task<LicenseStatusDto> GetCurrentStatusAsync(CancellationToken cancellationToken = default)
    {
        var licensePath = configuration["Licensing:LicenseFilePath"] ?? "not-configured";
        var fingerprintSalt = configuration["Licensing:FingerprintSalt"] ?? "missing";
        var pseudoFingerprint = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes($"{licensePath}:{fingerprintSalt}")))[..16];

        var result = new LicenseStatusDto(
            Status: "Scaffold",
            IsValid: false,
            FingerprintHash: pseudoFingerprint,
            CheckedAtUtc: DateTime.UtcNow,
            Message: "Starter licensing service in place. Replace with real offline signature verification."
        );

        return Task.FromResult(result);
    }
}
