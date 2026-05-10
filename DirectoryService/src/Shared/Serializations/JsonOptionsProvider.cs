using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Serializations;

public static class JsonOptionsProvider
{
    public static readonly JsonSerializerOptions Options =
        new(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new ErrorsJsonConverter(),
                new JsonStringEnumConverter(),
            },
        };
}