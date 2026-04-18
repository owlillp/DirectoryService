namespace Shared.Failures;

public static class GeneralErrors
{
    public static Error NotFound(Guid? id = null)
    {
        string message = id == null
            ? "Record from id: not found"
            : $"Record from id: {id.ToString()} not found";

        return Error.NotFound("record.not.found", message);
    }

    public static Error ValueIsInvalid(string? name = null,  string? reason = null, string? field = null)
    {
        string message = reason == null
            ? $"{name ?? "Value"} is invalid"
            : $"{name ?? "Value"} is invalid for reason: {reason}";

        return Error.Validation("value.is.invalid", message, field);
    }

    public static Error InvalidLength(string? name = null, string? field = null)
        => ValueIsInvalid(name, "Invalid length", field);

    public static Error NegativeValue(string? name = null, string? field = null)
        => ValueIsInvalid(name, "Negative value", field);

    public static Error InvalidIANACode(string? name = null, string? field = null)
        => ValueIsInvalid(name, "Invalid IANA code", field);

    public static Error InvalidCharacters(string? name = null, string? field = null)
        => ValueIsInvalid(name, "Invalid characters", field);

    public static Error ValueIsRequired(string name)
    {
        string message = string.IsNullOrWhiteSpace(name)
            ? "Any value is required"
            : $"Value {name} is required";

        return Error.Validation("value.is.required", message);
    }

    public static Error FieldIsRequired(string name, string field)
    {
        string message = string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(field)
            ? "Any field is required"
            : $"Field {name}.{field} is required";

        return Error.Validation("value.is.required", message);
    }

    public static Error Failure(string? message = null)
    {
        message ??= "Something went wrong";
        return Error.Failure("server.failure", message);
    }
}