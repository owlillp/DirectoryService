using CSharpFunctionalExtensions;
using Shared.Failures;

namespace DirectoryService.Domain.Departments;

public record DepartmentPath
{
    private const char PATH_SEPARATOR = '.';

    // EF Core
    private DepartmentPath() { }

    private DepartmentPath(string value) => Value = value;

    public string Value { get; } = string.Empty;

    public static Result<DepartmentPath, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired(nameof(Value));
        }

        return new DepartmentPath(value);
    }

    public static DepartmentPath CreateParent(DepartmentIdentifier identifier)
        => new (identifier.Value);

    public DepartmentPath CreateChild(DepartmentIdentifier identifier)
        => new (Value + PATH_SEPARATOR + identifier.Value);

    public bool StartWith(DepartmentPath path)
        => Value.StartsWith(path.Value);

    public short GetDepth()
        => (short)(Value.Split('.').Length - 1);
}