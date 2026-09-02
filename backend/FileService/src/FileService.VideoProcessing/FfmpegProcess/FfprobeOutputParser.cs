using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpFunctionalExtensions;
using FileService.Domain;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing.FfmpegProcess;

public static class FfprobeOutputParser
{
    public static Result<VideoMetadata, Error> Parse(string jsonOutput)
    {
        if (string.IsNullOrWhiteSpace(jsonOutput))
        {
            return GeneralErrors.ValueIsInvalid("Empty json");
        }

        FfprobeResponse? response;
        try
        {
            response = JsonSerializer.Deserialize<FfprobeResponse>(jsonOutput);
        }
        catch (Exception ex)
        {
            return GeneralErrors.ValueIsInvalid($"Json parse error: {ex.Message}");
        }

        if (response == null)
        {
            return GeneralErrors.ValueIsInvalid("Empty ffprobe response");
        }

        StreamInfo? streamInfo = response.Streams.FirstOrDefault();
        if (streamInfo == null)
        {
            return GeneralErrors.ValueIsInvalid("No streams found");
        }

        if (streamInfo.Width == null || streamInfo.Height == null)
        {
            return GeneralErrors.ValueIsInvalid("Missing resolution");
        }

        double? durationSeconds = response.Format?.Duration;
        if (durationSeconds == null || durationSeconds <= 0)
        {
            return GeneralErrors.ValueIsInvalid("Missing or invalid duration");
        }

        var duration = TimeSpan.FromSeconds(durationSeconds.Value);

        return VideoMetadata.Create(duration, streamInfo.Width.Value, streamInfo.Height.Value);
    }

    private sealed class FfprobeResponse
    {
        [JsonPropertyName("streams")]
        public List<StreamInfo>? Streams { get; set; }

        [JsonPropertyName("format")]
        public FormatInfo? Format { get; set; }
    }

    private sealed class StreamInfo
    {
        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }
    }

    private sealed class FormatInfo
    {
        [JsonPropertyName("duration")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double? Duration { get; set; }
    }

    private sealed class StringToDoubleConverter : JsonConverter<double?>
    {
        public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string? str = reader.GetString();
                return double.TryParse(str, out double result)
                    ? result
                    : null;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDouble();
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}