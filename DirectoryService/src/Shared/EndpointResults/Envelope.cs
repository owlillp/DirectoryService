using System.Text.Json.Serialization;
using Shared.Failures;

namespace Shared.EndpointResults;

public record Envelope
{
    public Errors? Errors { get; }

    public DateTime TimeGenerated { get; }

    public bool IsFailure => Errors != null && Errors.Any();

    public bool IsSuccess => !IsFailure;

    [JsonConstructor]
    private Envelope(Errors? errors = null, DateTime timeGenerated = default)
    {
        Errors = errors;
        TimeGenerated = timeGenerated == default
            ? DateTime.UtcNow
            : timeGenerated;
    }

    public static Envelope Success()
        => new();

    public static Envelope Failure(Errors? errors)
        => new(errors);
}

public record Envelope<T>
{
    public T? Result { get; }

    public Errors? Errors { get; }

    public DateTime TimeGenerated { get; }

    public bool IsFailure => Errors != null && Errors.Any();

    public bool IsSuccess => !IsFailure;

    [JsonConstructor]
    private Envelope(T? result, Errors? errors, DateTime timeGenerated = default)
    {
        Result = result;
        Errors = errors;
        TimeGenerated = timeGenerated == default
            ? DateTime.UtcNow
            : timeGenerated;
    }

    public static Envelope<T> Success(T? result)
        => new(result, null);

    public static Envelope<T> Failure(Errors? errors)
        => new(default, errors);
}