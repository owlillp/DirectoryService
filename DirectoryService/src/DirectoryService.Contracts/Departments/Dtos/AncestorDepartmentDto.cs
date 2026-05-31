namespace DirectoryService.Contracts.Departments.Dtos;

public record AncestorDepartmentDto
{
    public Guid Id { get; init; }
    public Guid? ParentId { get; init; }
    public string Name { get; init; } = null!;
    public string Identifier { get; init; } = null!;
    public string Path { get; init; } = null!;
    public int Depth { get; init; }
}