namespace Shared.Failures;

public record Error
{
    private Error(string code, string message, ErrorType type, string? invalidField = null)
    {
        Code = code;
        Message = message;
        Type = type;
        InvalidField = invalidField;
    }

    public string Code { get; }

    public string Message { get; }

    public ErrorType Type { get; }

    public string? InvalidField { get; }

    public Errors ToErrors()
        => new ([this]);

    public static Error NotFound(string? code, string message, string? invalidField = null)
        => new(code ?? "value.not.found", message, ErrorType.NOT_FOUND, invalidField);

    public static Error Validation(string? code, string message, string? invalidField = null)
        => new(code ?? "value.is.invalid", message, ErrorType.VALIDATION, invalidField);

    public static Error Failure(string? code, string message, string? invalidField = null)
        => new(code ?? "failure", message, ErrorType.FAILURE, invalidField);

    public static Error Conflict(string? code, string message, string? invalidField = null)
        => new(code ?? "value.is.conflict", message, ErrorType.CONFLICT, invalidField);
}