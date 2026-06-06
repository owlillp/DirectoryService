using Core.Abstractions;

namespace DirectoryService.Application.Departments.Commands.SoftDeleteDepartment;

public record SoftDeleteDepartmentCommand(Guid DepartmentId) : ICommand;