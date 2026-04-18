using System.Security.Cryptography;
using System.Text;
using InvoiceManager.Licensing.Models;

namespace InvoiceManager.Licensing.Services;

public sealed class RsaLicenseSignatureVerifier : ILicenseSignatureVerifier
{
    public bool Verify(SignedLicenseDocument document)
    {
        if (string.IsNullOrWhiteSpace(document.SignatureBase64))
        {
            return false;
        }

        var payload = LicensePayloadSerializer.SerializeLicensePayload(document);

        using var rsa = RSA.Create();
        rsa.FromXmlString(EmbeddedLicenseKeyProvider.PublicKeyXml);
        var signature = Convert.FromBase64String(document.SignatureBase64);

        return rsa.VerifyData(
            Encoding.UTF8.GetBytes(payload),
            signature,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
    }
}
