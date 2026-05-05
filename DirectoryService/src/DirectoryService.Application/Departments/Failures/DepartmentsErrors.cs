using Shared.Failures;

namespace DirectoryService.Application.Departments.Failures;

public static class DepartmentsErrors
{
    public static Error Inactive(Guid departmentId)
        => Error.Validation("department.inactive", $"Department with id: {departmentId} is inactive");

    public static Error CyclicHierarchy(Guid? departmentId = null, string? invalidField = null)
    {
        string message = departmentId.HasValue
            ? $"Department with id: {departmentId.Value} is cyclic"
            : "Cyclic hierarchy from department";

        return Error.Conflict("department.cyclic.hierarchy", message, invalidField);
    }

    public static Error SelfRepeat(Guid? departmentId = null)
    {
        string message = departmentId.HasValue
            ? $"Self repeated department id: {departmentId}"
            : "Self repeated department id";

        return Error.Validation("department.self.repeated", message);
    }
}