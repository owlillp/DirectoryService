using CSharpFunctionalExtensions;
using Shared.Failures;

namespace DirectoryService.Domain.Departments;

public record DepartmentPath
{
    private const char PATH_SEPARATOR = '.';
    private const string DELETED_PREFIX = "deleted_";

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

    public DepartmentPath AddSoftDeletePrefix()
    {
        string[] pathParts = Value.Split(PATH_SEPARATOR);
        if (!pathParts[^1].StartsWith(DELETED_PREFIX))
        {
            pathParts[^1] = DELETED_PREFIX + pathParts[^1];
        }

        string pathValue = string.Join(PATH_SEPARATOR, pathParts);
        return new DepartmentPath(pathValue);
    }
}