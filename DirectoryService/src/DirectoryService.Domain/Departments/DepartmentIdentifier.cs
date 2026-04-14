using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared.Constants;
using DirectoryService.Domain.Shared.Errors;

namespace DirectoryService.Domain.Departments;

public record DepartmentIdentifier
{
    // EF Core
    private DepartmentIdentifier() { }

    private DepartmentIdentifier(string value) => Value = value;

    public string Value { get; } = string.Empty;

    public static Result<DepartmentIdentifier, Error> Create(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
        {
            return Error.Validation("department.identifier.validation.error", "value cannot be empty");
        }

        if (value.Length < LengthConstants.LENGTH_3 || value.Length > LengthConstants.LENGTH_150)
        {
            return Error.Validation("department.identifier.validation.error", $"value must be between {LengthConstants.LENGTH_3} and {LengthConstants.LENGTH_150} characters long");
        }

        if (!value.All(char.IsAsciiLetter))
        {
            return Error.Validation("department.identifier.validation.error", "value must consist only of latin letters");
        }

        return new DepartmentIdentifier(value);
    }
}