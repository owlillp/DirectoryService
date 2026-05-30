using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Positions.Commands.SoftDelete;

public record SoftDeletePositionCommand(Guid PositionId) : ICommand;