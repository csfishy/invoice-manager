using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Net.NetworkInformation;
using InvoiceManager.Licensing.Configuration;
using Microsoft.Extensions.Options;

namespace InvoiceManager.Licensing.Services;

public sealed class MachineFingerprintService(IOptions<LicensingOptions> options)
{
    private readonly LicensingOptions _options = options.Value;

    public string GetHashedFingerprint()
    {
        var components = new List<string>
        {
            Environment.MachineName,
            RuntimeInformation.OSDescription,
            RuntimeInformation.OSArchitecture.ToString(),
            RuntimeInformation.ProcessArchitecture.ToString(),
            Environment.ProcessorCount.ToString()
        };

        components.AddRange(NetworkInterface.GetAllNetworkInterfaces()
            .Where(x => x.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .Select(x => x.GetPhysicalAddress().ToString())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x));

        if (File.Exists("/etc/machine-id"))
        {
            components.Add(File.ReadAllText("/etc/machine-id").Trim());
        }

        var rawFingerprint = string.Join("|", components.Where(x => !string.IsNullOrWhiteSpace(x)));
        var salted = $"{_options.FingerprintSalt}|{rawFingerprint}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(salted));
        return Convert.ToHexString(bytes);
    }
}
