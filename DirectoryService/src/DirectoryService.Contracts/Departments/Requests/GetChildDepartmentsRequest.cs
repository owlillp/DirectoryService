namespace DirectoryService.Contracts.Departments.Requests;

public record GetChildDepartmentsRequest
{
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 20;
}