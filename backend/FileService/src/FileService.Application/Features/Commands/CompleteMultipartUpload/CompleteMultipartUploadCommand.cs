using Core.Abstractions;
using FileService.Contracts.Files.Requests;

namespace FileService.Application.Features.Commands.CompleteMultipartUpload;

public record CompleteMultipartUploadCommand(CompleteMultipartUploadRequest Request) : ICommand;