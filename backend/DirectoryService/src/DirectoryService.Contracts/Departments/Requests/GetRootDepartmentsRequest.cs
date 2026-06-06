namespace DirectoryService.Contracts.Departments.Requests;

public record GetRootDepartmentsRequest
{
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 20;
    public int Prefetch { get; init; } = 3;
}