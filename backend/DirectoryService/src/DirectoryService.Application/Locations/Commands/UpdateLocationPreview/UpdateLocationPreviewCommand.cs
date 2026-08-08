using Core.Abstractions;

namespace DirectoryService.Application.Locations.Commands.UpdateLocationPreview;

public record UpdateLocationPreviewCommand(Guid LocationId, Guid? PreviewId) : ICommand;