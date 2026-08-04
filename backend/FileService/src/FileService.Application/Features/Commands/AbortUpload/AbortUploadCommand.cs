using Core.Abstractions;

namespace FileService.Application.Features.Commands.AbortUpload;

public record AbortUploadCommand(Guid FileId) : ICommand;