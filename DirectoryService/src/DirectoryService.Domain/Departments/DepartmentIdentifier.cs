using CSharpFunctionalExtensions;
using Shared.Constants;
using Shared.Failures;

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
            return GeneralErrors.ValueIsRequired(nameof(DepartmentIdentifier));
        }

        if (value.Length < LengthConstants.LENGTH_3 || value.Length > LengthConstants.LENGTH_150)
        {
            return GeneralErrors.InvalidLength(nameof(DepartmentIdentifier));
        }

        if (!value.All(char.IsAsciiLetter))
        {
            return GeneralErrors.InvalidCharacters(nameof(DepartmentIdentifier));
        }

        return new DepartmentIdentifier(value);
    }
}