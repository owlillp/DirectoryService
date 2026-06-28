namespace DirectoryService.Contracts.Departments.Dtos;

public record DepartmentDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Identifier { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public Guid? ParentId { get; init; }
    public int Depth { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<Guid> LocationIds { get; init; } = [];
    public List<Guid> PositionIds { get; init; } = [];
}