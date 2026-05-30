using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations.Requests;

namespace DirectoryService.Application.Locations.Commands.UpdateLocation;

public record UpdateLocationCommand(Guid LocationId, UpdateLocationRequest Request) : ICommand;