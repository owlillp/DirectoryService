using CSharpFunctionalExtensions;
using FileService.Application.Models;
using FileService.Domain;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Abstractions;

public interface IFileStorageProvider
{
    Task<Result<string, Error>> GenerateUploadUrlAsync(StorageKey storageKey, MediaData mediaData, CancellationToken cancellationToken);

    Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey storageKey);

    Task<Result<IReadOnlyList<MediaUrl>, Error>> GenerateDownloadUrlsAsync(IEnumerable<StorageKey> storageKeys, CancellationToken cancellationToken);

    Task<UnitResult<Error>> DeleteFileAsync(StorageKey storageKey, CancellationToken cancellationToken);

    Task<Result<StorageObjectMetadata, Error>> GetAssetMetadataAsync(StorageKey storageKey, CancellationToken cancellationToken);

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