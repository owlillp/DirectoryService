namespace DirectoryService.Contracts.Positions;

public record CreatePositionRequest(
    string Name,
    string? Description,
    IEnumerable<Guid> DepartmentIds);