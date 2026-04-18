using System.Text.Json.Serialization;
using Shared.Failures;

namespace DirectoryService.Presentation.EndpointResults;

public record Envelope
{
    public object? Result { get; }

    public Errors? Errors { get; }

    public DateTime TimeGenerated { get; }

    public bool IsFailure => Errors != null || (Errors != null && Errors.Any());

    public bool IsSuccess => !IsFailure;

    [JsonConstructor]
    private Envelope(object? result, Errors? errors)
    {
        Result = result;
        Errors = errors;
        TimeGenerated = DateTime.UtcNow;
    }

    public static Envelope Success(object? result)
        => new(result, null);

    public static Envelope Failure(Errors? errors)
        => new(null, errors);
}

public record Envelope<T>
{
    public T? Result { get; }

    public Errors? Errors { get; }

    public DateTime TimeGenerated { get; }

    public bool IsFailure => Errors != null || (Errors != null && Errors.Any());

    public bool IsSuccess => !IsFailure;

    [JsonConstructor]
    private Envelope(T? result, Errors? errors)
    {
        Result = result;
        Errors = errors;
        TimeGenerated = DateTime.UtcNow;
    }

    public static Envelope<T> Success(T? result)
        => new(result, null);

    public static Envelope<T> Failure(Errors? errors)
        => new(default, errors);
}