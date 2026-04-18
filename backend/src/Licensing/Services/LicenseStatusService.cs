using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using InvoiceManager.Application.Licensing;
using InvoiceManager.Licensing.Configuration;
using InvoiceManager.Licensing.Models;
using Microsoft.Extensions.Options;

namespace InvoiceManager.Licensing.Services;

public sealed class LicenseStatusService(
    MachineFingerprintService machineFingerprintService,
    IOptions<LicensingOptions> options) : ILicenseStatusService
{
    private readonly LicensingOptions _options = options.Value;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public Task<string> GetCurrentFingerprintHashAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(machineFingerprintService.GetHashedFingerprint());
    }

    public async Task<LicenseStatusDto> ImportLicenseAsync(Stream content, CancellationToken cancellationToken = default)
    {
        var licensePath = _options.LicenseFilePath;
        var directory = Path.GetDirectoryName(licensePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var fileStream = File.Create(licensePath);
        await content.CopyToAsync(fileStream, cancellationToken);
        await fileStream.FlushAsync(cancellationToken);

        return await GetCurrentStatusAsync(cancellationToken);
    }

    public async Task<LicenseStatusDto> GetCurrentStatusAsync(CancellationToken cancellationToken = default)
    {
        var fingerprintHash = machineFingerprintService.GetHashedFingerprint();
        var checkedAtUtc = DateTime.UtcNow;

        try
        {
            if (string.IsNullOrWhiteSpace(_options.FingerprintSalt))
            {
                return Invalid("ConfigurationError", "Fingerprint salt is missing.", fingerprintHash, checkedAtUtc);
            }

            if (!File.Exists(_options.LicenseFilePath))
            {
                return Invalid("MissingLicense", "License file has not been imported yet.", fingerprintHash, checkedAtUtc);
            }

            var json = await File.ReadAllTextAsync(_options.LicenseFilePath, cancellationToken);
            var document = JsonSerializer.Deserialize<SignedLicenseDocument>(json, JsonOptions);

            if (document is null)
            {
                return Invalid("InvalidLicense", "License file could not be parsed.", fingerprintHash, checkedAtUtc);
            }

            if (!string.Equals(document.ProductName, _options.LicensedProductName, StringComparison.OrdinalIgnoreCase))
            {
                return Invalid("WrongProduct", "License product does not match this application.", fingerprintHash, checkedAtUtc, document);
            }

            if (!VerifySignature(document))
            {
                return Invalid("InvalidSignature", "License signature verification failed.", fingerprintHash, checkedAtUtc, document);
            }

            if (!string.Equals(document.MachineFingerprintHash, fingerprintHash, StringComparison.OrdinalIgnoreCase))
            {
                return Invalid("FingerprintMismatch", "License is not bound to this machine fingerprint.", fingerprintHash, checkedAtUtc, document);
            }

            if (document.ExpiresAtUtc.HasValue && document.ExpiresAtUtc.Value < checkedAtUtc)
            {
                return Invalid("Expired", "License has expired.", fingerprintHash, checkedAtUtc, document);
            }

            return new LicenseStatusDto(
                "Valid",
                true,
                fingerprintHash,
                document.LicenseId,
                document.CustomerName,
                document.IssuedAtUtc,
                document.ExpiresAtUtc,
                document.Features,
                checkedAtUtc,
                "License is valid.");
        }
        catch (Exception ex)
        {
            return Invalid("Error", $"License verification failed: {ex.Message}", fingerprintHash, checkedAtUtc);
        }
    }

    private static bool VerifySignature(SignedLicenseDocument document)
    {
        if (string.IsNullOrWhiteSpace(document.SignatureBase64))
        {
            return false;
        }

        var payload = JsonSerializer.Serialize(new
        {
            document.LicenseId,
            document.CustomerName,
            document.ProductName,
            document.IssuedAtUtc,
            document.ExpiresAtUtc,
            document.MachineFingerprintHash,
            document.Features
        }, JsonOptions);

        using var rsa = RSA.Create();
        rsa.FromXmlString(EmbeddedLicenseKeyProvider.PublicKeyXml);
        var signature = Convert.FromBase64String(document.SignatureBase64);
        return rsa.VerifyData(
            Encoding.UTF8.GetBytes(payload),
            signature,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
    }

    private static LicenseStatusDto Invalid(
        string status,
        string message,
        string fingerprintHash,
        DateTime checkedAtUtc,
        SignedLicenseDocument? document = null)
    {
        return new LicenseStatusDto(
            status,
            false,
            fingerprintHash,
            document?.LicenseId,
            document?.CustomerName,
            document?.IssuedAtUtc,
            document?.ExpiresAtUtc,
            document?.Features?.ToArray() ?? Array.Empty<string>(),
            checkedAtUtc,
            message);
    }
}
