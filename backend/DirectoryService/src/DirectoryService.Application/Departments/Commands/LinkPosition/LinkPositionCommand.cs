using Core.Abstractions;

namespace DirectoryService.Application.Departments.Commands.LinkPosition;

public record LinkPositionCommand(Guid DepartmentId, Guid PositionId) : ICommand;