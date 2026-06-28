namespace DirectoryService.Contracts.Positions.Requests;

public record CreatePositionRequest(
    string Name,
    string? Description,
    IEnumerable<Guid> DepartmentIds);