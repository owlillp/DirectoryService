using CSharpFunctionalExtensions;
using FileService.Domain;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Abstractions;

public interface IS3Provider
{
    Task<Result<string, Error>> GenerateUploadUrlAsync(StorageKey storageKey, MediaData mediaData, CancellationToken cancellationToken);

    Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey storageKey);

    Task<UnitResult<Error>> DeleteAssetAsync(StorageKey storageKey, CancellationToken cancellationToken);

    Task<Result<IDictionary<string, string>, Error>> GetAssetMetadataAsync(StorageKey storageKey, CancellationToken cancellationToken);

    Task<UnitResult<Error>> InitializeBucketAsync(string bucketName, CancellationToken cancellationToken);

    /*Task<Result<string, Error>> StartMultipartUploadAsync(
        string bucketName,
        string key,
        string contentType,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<string>, Error>> GenerateAllChunksUploadUrlsAsync(
        string bucketName,
        string key,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken);

    Task<Result<string, Error>> CompleteMultipartUploadAsync(
        string bucketName,
        string key,
        string uploadId,
        IReadOnlyList<PartETagDto> partETags,
        CancellationToken cancellationToken);*/
}