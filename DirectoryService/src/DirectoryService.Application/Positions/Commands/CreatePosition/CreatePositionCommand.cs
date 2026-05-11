using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Positions;
using DirectoryService.Contracts.Positions.Requests;

namespace DirectoryService.Application.Positions.Commands.CreatePosition;

public record CreatePositionCommand(CreatePositionRequest Request) : ICommand;