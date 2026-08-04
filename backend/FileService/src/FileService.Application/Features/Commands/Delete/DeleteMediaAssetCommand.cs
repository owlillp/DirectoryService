using Core.Abstractions;

namespace FileService.Application.Features.Commands.Delete;

public record DeleteMediaAssetCommand(Guid FileId) : ICommand;