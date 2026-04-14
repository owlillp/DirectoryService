using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared.Constants;
using DirectoryService.Domain.Shared.Errors;

namespace DirectoryService.Domain.Departments;

public record DepartmentName
{
    // EF Core
    private DepartmentName() { }

    private DepartmentName(string value) => Value = value;

    public string Value { get; } = string.Empty;

    public static Result<DepartmentName, Error> Create(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
        {
            return Error.Validation("department.name.validation.error", "value cannot be empty");
        }

        if (value.Length < LengthConstants.LENGTH_3 || value.Length > LengthConstants.LENGTH_150)
        {
            return Error.Validation("department.name.validation.error", $"value must be between {LengthConstants.LENGTH_3} and {LengthConstants.LENGTH_150} characters long");
        }

        return new DepartmentName(value);
    }
}