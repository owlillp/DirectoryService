using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Departments;

public record DepartmentName
{
    public const short MIN_LENGTH = 3;
    public const short MAX_LENGTH = 150;

    public string Value { get; }

    private DepartmentName(string value) => Value = value;

    public static Result<DepartmentName, Error> Create(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
        {
            return Error.Validation("department.name.validation.error", "value cannot be empty");
        }

        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
        {
            return Error.Validation("department.name.validation.error", $"value must be between {MIN_LENGTH} and {MAX_LENGTH} characters long");
        }

        return new DepartmentName(value);
    }
}