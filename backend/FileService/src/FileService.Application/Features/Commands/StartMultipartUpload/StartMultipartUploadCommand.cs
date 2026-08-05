using Core.Abstractions;
using FileService.Contracts.Files.Requests;

namespace FileService.Application.Features.Commands.StartMultipartUpload;

public record StartMultipartUploadCommand(StartMultipartUploadRequest Request) : ICommand;