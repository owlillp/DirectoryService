namespace DirectoryService.Contracts.Departments.Dtos;

public record TopDepartmentByPositionsDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string Path { get; init; } = null!;
    public int PositionsCount { get; init; }
}