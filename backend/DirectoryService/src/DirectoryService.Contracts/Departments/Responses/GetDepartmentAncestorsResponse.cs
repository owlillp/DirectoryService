using DirectoryService.Contracts.Departments.Dtos;

namespace DirectoryService.Contracts.Departments.Responses;

public record GetDepartmentAncestorsResponse (
    Guid TargetDepartmentId,
    IReadOnlyList<AncestorDepartmentDto> AncestorDepartments);