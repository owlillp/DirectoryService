using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.Commands.UnlinkLocation;

public record UnlinkLocationCommand(Guid DepartmentId, Guid LocationId) : ICommand;