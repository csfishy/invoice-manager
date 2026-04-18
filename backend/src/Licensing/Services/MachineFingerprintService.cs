using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using InvoiceManager.Licensing.Configuration;
using Microsoft.Extensions.Options;

namespace InvoiceManager.Licensing.Services;

public sealed class MachineFingerprintService(IOptions<LicensingOptions> options)
{
    private readonly LicensingOptions _options = options.Value;

    public string GetHashedFingerprint()
    {
        var components = GetFingerprintComponents();
        var rawFingerprint = string.Join("|", components);
        var salted = $"{_options.FingerprintSalt}|{rawFingerprint}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(salted));
        return Convert.ToHexString(bytes);
    }

    public string GetMachineName() => Environment.MachineName;

    private IReadOnlyCollection<string> GetFingerprintComponents()
    {
        var components = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var value in GetPlatformSpecificIdentifiers())
        {
            AddComponent(components, value);
        }

        foreach (var address in NetworkInterface.GetAllNetworkInterfaces()
                     .Where(x => x.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                     .Select(x => x.GetPhysicalAddress().ToString()))
        {
            AddComponent(components, address);
        }

        AddComponent(components, RuntimeInformation.OSArchitecture.ToString());
        AddComponent(components, RuntimeInformation.ProcessArchitecture.ToString());

        return components
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IEnumerable<string> GetPlatformSpecificIdentifiers()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            foreach (var value in QueryWindowsHardwareIdentifiers())
            {
                yield return value;
            }

            yield break;
        }

        if (File.Exists("/etc/machine-id"))
        {
            yield return File.ReadAllText("/etc/machine-id");
        }

        yield return Environment.MachineName;
        yield return Environment.ProcessorCount.ToString();
    }

    [SupportedOSPlatform("windows")]
    private static IEnumerable<string> QueryWindowsHardwareIdentifiers()
    {
        foreach (var query in new[]
                 {
                     ("Win32_ComputerSystemProduct", "UUID"),
                     ("Win32_BaseBoard", "SerialNumber"),
                     ("Win32_BIOS", "SerialNumber"),
                     ("Win32_Processor", "ProcessorId"),
                     ("Win32_DiskDrive", "SerialNumber")
                 })
        {
            foreach (var result in ExecuteWmiQuery(query.Item1, query.Item2))
            {
                yield return result;
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private static IEnumerable<string> ExecuteWmiQuery(string className, string propertyName)
    {
        var results = new List<string>();

        try
        {
            using var searcher = new ManagementObjectSearcher($"SELECT {propertyName} FROM {className}");
            using var collection = searcher.Get();

            foreach (var item in collection)
            {
                if (item is ManagementObject managementObject)
                {
                    var value = managementObject[propertyName]?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        results.Add(value);
                    }
                }
            }
        }
        catch
        {
            return Array.Empty<string>();
        }

        return results;
    }

    private static void AddComponent(ISet<string> components, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var normalized = value.Trim().ToUpperInvariant();
        components.Add(normalized);
    }
}
