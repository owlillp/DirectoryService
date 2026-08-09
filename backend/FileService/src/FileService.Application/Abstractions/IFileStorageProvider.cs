using CSharpFunctionalExtensions;
using FileService.Application.Models;
using FileService.Contracts.Files.Dtos;
using FileService.Domain;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Abstractions;

public interface IFileStorageProvider
{
    Task<Result<string, Error>> GenerateUploadUrlAsync(StorageKey storageKey, MediaData mediaData, bool useExternal = true);

    Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey storageKey, bool useExternal = true);

    Task<Result<IReadOnlyList<MediaUrl>, Error>> GenerateDownloadUrlsAsync(
        IEnumerable<StorageKey> storageKeys,
        CancellationToken cancellationToken,
        bool useExternal = true);

    Task<UnitResult<Error>> DeleteFileAsync(StorageKey storageKey, CancellationToken cancellationToken);

    Task<Result<StorageObjectMetadata, Error>> GetAssetMetadataAsync(StorageKey storageKey, CancellationToken cancellationToken);

    Task<UnitResult<Error>> InitializeBucketAsync(string bucketName, CancellationToken cancellationToken);

    Task<Result<string, Error>> StartMultipartUploadAsync(StorageKey storageKey, string contentType, CancellationToken cancellationToken);

    Task<UnitResult<Error>> AbortMultipartUploadAsync(StorageKey key, string uploadId, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<ChunkUploadUrl>, Error>> GenerateAllChunksUploadUrlsAsync(
        StorageKey storageKey,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken,
        bool useExternal = true);

    Task<Result<string, Error>> CompleteMultipartUploadAsync(
        StorageKey storageKey,
        string uploadId,
        IReadOnlyList<PartETagDto> partETags,
        CancellationToken cancellationToken);
}