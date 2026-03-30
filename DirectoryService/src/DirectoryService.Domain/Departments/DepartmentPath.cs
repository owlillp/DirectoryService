using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Departments;

public record DepartmentPath
{
    private DepartmentPath(string value) => Value = value;

    public string Value { get; }

    public static Result<DepartmentPath, Error> Create(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
        {
            return Error.Validation("department.path.validation.error", "value cannot be empty");
        }

        return new DepartmentPath(value);
    }
}