using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.Commands.UnlinkPosition;

public record UnlinkPositionCommand(Guid DepartmentId, Guid PositionId) : ICommand;