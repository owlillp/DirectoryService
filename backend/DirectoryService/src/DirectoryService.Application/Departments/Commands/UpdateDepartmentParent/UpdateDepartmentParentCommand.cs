using Core.Abstractions;
using DirectoryService.Contracts.Departments.Requests;

namespace DirectoryService.Application.Departments.Commands.UpdateDepartmentParent;

public record UpdateDepartmentParentCommand(Guid DepartmentId, UpdateDepartmentParentRequest Request) : ICommand;