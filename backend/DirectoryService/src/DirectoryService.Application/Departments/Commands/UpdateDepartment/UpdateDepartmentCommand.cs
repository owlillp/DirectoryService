using Core.Abstractions;
using DirectoryService.Contracts.Departments.Requests;

namespace DirectoryService.Application.Departments.Commands.UpdateDepartment;

public record UpdateDepartmentCommand(Guid DepartmentId, UpdateDepartmentRequest Request) : ICommand;