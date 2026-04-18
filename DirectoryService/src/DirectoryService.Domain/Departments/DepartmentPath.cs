using CSharpFunctionalExtensions;
using Shared.Failures;

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
            return GeneralErrors.ValueIsRequired(nameof(DepartmentPath));
        }

        return new DepartmentPath(value);
    }
}