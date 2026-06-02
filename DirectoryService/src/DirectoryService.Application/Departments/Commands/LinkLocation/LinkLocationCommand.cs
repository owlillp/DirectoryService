using Core.Abstractions;

namespace DirectoryService.Application.Departments.Commands.LinkLocation;

public record LinkLocationCommand(Guid DepartmentId, Guid LocationId) : ICommand;