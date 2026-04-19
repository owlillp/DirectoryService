using CSharpFunctionalExtensions;
using Shared.Constants;
using Shared.Failures;

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
            return GeneralErrors.ValueIsRequired(nameof(DepartmentName));
        }

        if (value.Length < LengthConstants.LENGTH_3 || value.Length > LengthConstants.LENGTH_150)
        {
            return GeneralErrors.InvalidLength(nameof(DepartmentName));
        }

        return new DepartmentName(value);
    }
}