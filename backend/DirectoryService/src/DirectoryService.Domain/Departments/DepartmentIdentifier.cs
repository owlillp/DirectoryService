using Core.Constants;
using CSharpFunctionalExtensions;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Domain.Departments;

public record DepartmentIdentifier
{
    private const char SEPARATOR = '_';

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

        if (!value.All(c => char.IsAsciiLetter(c) || c == SEPARATOR))
        {
            return GeneralErrors.InvalidCharacters(nameof(DepartmentIdentifier));
        }

        return new DepartmentIdentifier(value);
    }
}