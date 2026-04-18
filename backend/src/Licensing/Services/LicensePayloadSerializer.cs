using System.Text.Json;
using InvoiceManager.Licensing.Models;

namespace InvoiceManager.Licensing.Services;

internal static class LicensePayloadSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static string SerializeLicensePayload(SignedLicenseDocument document)
    {
        return JsonSerializer.Serialize(new
        {
            document.Version,
            document.LicenseId,
            document.CustomerName,
            document.ProductName,
            document.IssuedAtUtc,
            document.ExpiresAtUtc,
            document.MachineFingerprintHash,
            document.BoundAtUtc,
            document.Features
        }, JsonOptions);
    }

    public static string SerializeRequestCode(LicenseRequestPayload payload)
    {
        return JsonSerializer.Serialize(payload, JsonOptions);
    }
}
