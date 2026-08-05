using Core.Abstractions;
using FileService.Contracts.Files.Requests;

namespace FileService.Application.Features.Commands.AbortMultipartUpload;

public record AbortMultipartUploadCommand(AbortMultipartUploadRequest Request) : ICommand;