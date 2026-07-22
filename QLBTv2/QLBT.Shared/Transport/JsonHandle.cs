using System.Text.Json;

namespace QLBT.Shared.Transport
{
    public class JsonHandle
    {
        public static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }
}
