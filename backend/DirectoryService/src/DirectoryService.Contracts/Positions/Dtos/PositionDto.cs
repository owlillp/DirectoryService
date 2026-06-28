namespace DirectoryService.Contracts.Positions.Dtos;

public record PositionDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public List<Guid> DepartmentIds { get; init; } = [];
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}