using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Locations.Commands.SoftDelete;

public record SoftDeleteLocationCommand(Guid LocationId) : ICommand;