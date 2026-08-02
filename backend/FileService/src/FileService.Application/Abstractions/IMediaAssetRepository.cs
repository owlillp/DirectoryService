using CSharpFunctionalExtensions;
using FileService.Domain.Assets;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Abstractions;

public interface IMediaAssetRepository
{
    Task<Result<Guid, Error>> AddAsync(MediaAsset asset, CancellationToken cancellationToken);
}