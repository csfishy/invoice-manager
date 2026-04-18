using InvoiceManager.Licensing.Models;

namespace InvoiceManager.Licensing.Services;

public interface ILicenseSignatureVerifier
{
    bool Verify(SignedLicenseDocument document);
}
