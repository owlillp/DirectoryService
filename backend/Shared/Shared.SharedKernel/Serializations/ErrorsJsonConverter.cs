using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.SharedKernel.Failures;

namespace Shared.SharedKernel.Serializations;

public class ErrorsJsonConverter : JsonConverter<Errors>
{
    public override Errors Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var errors = JsonSerializer.Deserialize<List<Error>>(ref reader, options);

        return errors is null
            ? new Errors([])
            : new Errors(errors);
    }

    public override void Write(Utf8JsonWriter writer, Errors value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value.ToList(), options);
}