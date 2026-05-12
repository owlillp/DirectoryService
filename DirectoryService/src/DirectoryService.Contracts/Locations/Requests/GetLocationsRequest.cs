using DirectoryService.Contracts.Common;

namespace DirectoryService.Contracts.Locations.Requests;

public record GetLocationsRequest(
    Guid[]? DepartmentIds,
    string? Search,
    bool? IsActive,
    string? SortBy = "name",
    string? SortDirection = "asc",
    PaginationRequest? Pagination = null);