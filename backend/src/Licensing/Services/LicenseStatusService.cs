using System.Text;
using System.Text.Json;
using InvoiceManager.Application.Licensing;
using InvoiceManager.Domain.Licensing;
using InvoiceManager.Infrastructure.Persistence;
using InvoiceManager.Licensing.Configuration;
using InvoiceManager.Licensing.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InvoiceManager.Licensing.Services;

public sealed class LicenseStatusService(
    MachineFingerprintService machineFingerprintService,
    ILicenseSignatureVerifier signatureVerifier,
    InvoiceManagerDbContext dbContext,
    IOptions<LicensingOptions> options,
    ILogger<LicenseStatusService> logger) : ILicenseStatusService
{
    private readonly LicensingOptions _options = options.Value;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public Task<string> GetCurrentFingerprintHashAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(machineFingerprintService.GetHashedFingerprint());
    }

    public Task<LicenseRequestCodeDto> GetCurrentRequestCodeAsync(CancellationToken cancellationToken = default)
    {
        var payload = new LicenseRequestPayload
        {
            ProductName = _options.LicensedProductName,
            MachineName = machineFingerprintService.GetMachineName(),
            MachineFingerprintHash = machineFingerprintService.GetHashedFingerprint(),
            GeneratedAtUtc = DateTime.UtcNow
        };

        var requestCode = Convert.ToBase64String(Encoding.UTF8.GetBytes(LicensePayloadSerializer.SerializeRequestCode(payload)));

        return Task.FromResult(new LicenseRequestCodeDto(
            requestCode,
            payload.MachineFingerprintHash,
            payload.ProductName,
            payload.MachineName,
            payload.GeneratedAtUtc,
            "base64(json)",
            "Send this request code to the vendor to receive a signed offline license file."));
    }

    public async Task<LicenseStatusDto> ImportLicenseAsync(Stream content, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await content.CopyToAsync(memoryStream, cancellationToken);
        var licenseJson = Encoding.UTF8.GetString(memoryStream.ToArray());

        var status = await EvaluateAsync(licenseJson, persistBinding: false, cancellationToken);
        if (!status.IsValid)
        {
            logger.LogWarning("Rejected license import with status {Status}: {Message}", status.Status, status.Message);
            return status;
        }

        var licensePath = _options.LicenseFilePath;
        var directory = Path.GetDirectoryName(licensePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(licensePath, licenseJson, cancellationToken);
        await EvaluateAsync(licenseJson, persistBinding: true, cancellationToken);

        logger.LogInformation("Imported valid license file into {LicensePath}.", licensePath);
        return await GetCurrentStatusAsync(cancellationToken);
    }

    public async Task<LicenseStatusDto> GetCurrentStatusAsync(CancellationToken cancellationToken = default)
    {
        var fingerprintHash = machineFingerprintService.GetHashedFingerprint();
        var checkedAtUtc = DateTime.UtcNow;

        if (!File.Exists(_options.LicenseFilePath))
        {
            var status = BuildStatus(
                "MissingLicense",
                false,
                fingerprintHash,
                checkedAtUtc,
                "No license file has been imported. Generate a machine request code and obtain a signed license from the vendor.",
                null);

            await PersistBindingAsync(status, null, cancellationToken);
            LogStatus(status);
            return status;
        }

        var json = await File.ReadAllTextAsync(_options.LicenseFilePath, cancellationToken);
        var evaluated = await EvaluateAsync(json, persistBinding: true, cancellationToken);
        LogStatus(evaluated);
        return evaluated;
    }

    private async Task<LicenseStatusDto> EvaluateAsync(string json, bool persistBinding, CancellationToken cancellationToken)
    {
        var fingerprintHash = machineFingerprintService.GetHashedFingerprint();
        var checkedAtUtc = DateTime.UtcNow;
        SignedLicenseDocument? document = null;
        LicenseStatusDto status;

        try
        {
            document = JsonSerializer.Deserialize<SignedLicenseDocument>(json, JsonOptions);
            if (document is null)
            {
                status = BuildStatus(
                    "InvalidLicense",
                    false,
                    fingerprintHash,
                    checkedAtUtc,
                    "License file could not be parsed.",
                    null);
            }
            else if (document.Version <= 0)
            {
                status = BuildStatus(
                    "InvalidLicense",
                    false,
                    fingerprintHash,
                    checkedAtUtc,
                    "License file version is invalid.",
                    document);
            }
            else if (!string.Equals(document.ProductName, _options.LicensedProductName, StringComparison.OrdinalIgnoreCase))
            {
                status = BuildStatus(
                    "WrongProduct",
                    false,
                    fingerprintHash,
                    checkedAtUtc,
                    "License product does not match this application.",
                    document);
            }
            else if (!signatureVerifier.Verify(document))
            {
                status = BuildStatus(
                    "InvalidSignature",
                    false,
                    fingerprintHash,
                    checkedAtUtc,
                    "License signature verification failed.",
                    document);
            }
            else if (!string.Equals(document.MachineFingerprintHash, fingerprintHash, StringComparison.OrdinalIgnoreCase))
            {
                status = BuildStatus(
                    "FingerprintMismatch",
                    false,
                    fingerprintHash,
                    checkedAtUtc,
                    "License is bound to another machine fingerprint.",
                    document);
            }
            else if (document.ExpiresAtUtc.HasValue && document.ExpiresAtUtc.Value <= checkedAtUtc)
            {
                status = BuildStatus(
                    "Expired",
                    false,
                    fingerprintHash,
                    checkedAtUtc,
                    "License has expired.",
                    document);
            }
            else
            {
                status = BuildStatus(
                    "Valid",
                    true,
                    fingerprintHash,
                    checkedAtUtc,
                    "License is valid and activated for this machine.",
                    document);
            }
        }
        catch (Exception exception)
        {
            status = BuildStatus(
                "InvalidLicense",
                false,
                fingerprintHash,
                checkedAtUtc,
                $"License verification failed: {exception.Message}",
                document);
        }

        if (persistBinding)
        {
            await PersistBindingAsync(status, document, cancellationToken);
        }

        return status;
    }

    private async Task PersistBindingAsync(LicenseStatusDto status, SignedLicenseDocument? document, CancellationToken cancellationToken)
    {
        var binding = await dbContext.LicenseBindings
            .OrderByDescending(x => x.LastValidatedAtUtc ?? x.BoundAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (binding is null)
        {
            binding = new LicenseBinding();
            dbContext.LicenseBindings.Add(binding);
        }

        binding.LicenseId = document?.LicenseId ?? status.LicenseId ?? "UNLICENSED";
        binding.CustomerName = document?.CustomerName ?? status.CustomerName ?? "Unlicensed";
        binding.MachineFingerprintHash = status.FingerprintHash;
        binding.BindingStatus = status.Status;
        binding.BoundAtUtc = document?.BoundAtUtc == default ? DateTime.UtcNow : document?.BoundAtUtc ?? DateTime.UtcNow;
        binding.ExpiresAtUtc = status.ExpiresAtUtc;
        binding.LastValidatedAtUtc = status.CheckedAtUtc;
        binding.FeaturesJson = JsonSerializer.Serialize(status.Features, JsonOptions);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private void LogStatus(LicenseStatusDto status)
    {
        if (status.IsValid)
        {
            logger.LogInformation("License validation succeeded. Status {Status}. License {LicenseId}.", status.Status, status.LicenseId);
            return;
        }

        logger.LogWarning("License validation failed. Status {Status}. Message: {Message}", status.Status, status.Message);
    }

    private static LicenseStatusDto BuildStatus(
        string status,
        bool isValid,
        string fingerprintHash,
        DateTime checkedAtUtc,
        string message,
        SignedLicenseDocument? document)
    {
        return new LicenseStatusDto(
            status,
            isValid,
            fingerprintHash,
            document?.LicenseId,
            document?.CustomerName,
            document?.IssuedAtUtc,
            document?.ExpiresAtUtc,
            document?.Features?.ToArray() ?? Array.Empty<string>(),
            checkedAtUtc,
            message,
            !isValid);
    }
}
