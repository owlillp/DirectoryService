using Core.Abstractions;
using FileService.Contracts.Files.Requests;

namespace FileService.Application.Features.Commands.StartUpload;

public record StartUploadCommand(StartUploadRequest Request) : ICommand;