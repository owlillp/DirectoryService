using CSharpFunctionalExtensions;
using FileService.Contracts.Files.Dtos;
using FileService.Contracts.Files.Requests;
using Shared.SharedKernel.Failures;

namespace FileService.Contracts.Communication;

public interface IFileCommunicationService
{
    Task<Result<GetMediaAssetDto, Errors>> GetMediaAssetAsync(GetFilesForEntityRequest request, CancellationToken cancellationToken);

    Task<Result<bool, Errors>> CheckFileExistAsync(Guid fileId, string? assetType, CancellationToken cancellationToken);
}