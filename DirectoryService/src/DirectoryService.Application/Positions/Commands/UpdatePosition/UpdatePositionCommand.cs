using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Positions.Requests;

namespace DirectoryService.Application.Positions.Commands.UpdatePosition;

public record UpdatePositionCommand(Guid PositionId, UpdatePositionRequest Request) : ICommand;