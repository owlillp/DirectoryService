using Core.Abstractions;

namespace FileService.Application.Features.Commands.CompleteUpload;

public record CompleteUploadCommand(Guid FileId) : ICommand;