using System.Text.Json;
using System.Text.Json.Serialization;

namespace InvoiceManager.Tests.Infrastructure;

internal static class TestJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
