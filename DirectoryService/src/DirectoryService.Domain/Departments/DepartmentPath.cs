using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared.Errors;

namespace DirectoryService.Domain.Departments;

public record DepartmentPath
{
    // EF Core
    private DepartmentPath() { }

    private DepartmentPath(string value) => Value = value;

    public string Value { get; } = string.Empty;

    public static Result<DepartmentPath, Error> Create(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
        {
            return Error.Validation("department.path.validation.error", "value cannot be empty");
        }

        return new DepartmentPath(value);
    }
}