using Core.Abstractions;
using DirectoryService.Contracts.Locations.Requests;

namespace DirectoryService.Application.Locations.Commands.CreateLocation;

public record CreateLocationCommand(CreateLocationRequest Request) : ICommand;