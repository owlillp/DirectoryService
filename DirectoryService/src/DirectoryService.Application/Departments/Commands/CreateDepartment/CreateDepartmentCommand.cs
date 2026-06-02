using Core.Abstractions;
using DirectoryService.Contracts.Departments.Requests;

namespace DirectoryService.Application.Departments.Commands.CreateDepartment;

public record CreateDepartmentCommand(CreateDepartmentRequest Request) : ICommand;