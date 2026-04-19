namespace DirectoryService.Domain.Departments;

public record DepartmentPath
{
    private const char PATH_SEPARATOR = '/';

    // EF Core
    private DepartmentPath() { }

    private DepartmentPath(string value) => Value = value;

    public string Value { get; } = string.Empty;

    public static DepartmentPath CreateParent(DepartmentIdentifier identifier)
        => new (identifier.Value);

    public DepartmentPath CreateChild(DepartmentIdentifier identifier)
        => new (Value + PATH_SEPARATOR + identifier.Value);
}