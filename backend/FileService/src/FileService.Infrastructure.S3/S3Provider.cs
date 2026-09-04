using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Application.Models;
using FileService.Contracts.Files.Dtos;
using FileService.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.SharedKernel.Failures;

namespace FileService.Infrastructure.S3;

public class S3Provider(
    IAmazonS3 s3Client,
    ILogger<S3Provider> logger,
    IOptions<FileStorageOptions> s3Options) : IFileStorageProvider
{
    private readonly FileStorageOptions _fileStorageOptions = s3Options.Value;
    private readonly SemaphoreSlim _requestsSemaphore = new (s3Options.Value.MaxConcurrentRequests);

    public async Task<Result<string, Error>> GenerateUploadUrlAsync(StorageKey storageKey, MediaData mediaData, bool useExternal = true)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = storageKey.Location,
                Key = storageKey.Key,
                ContentType = mediaData.ContentType.Value,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddMinutes(_fileStorageOptions.UploadExpirationMinutes),
                Protocol = _fileStorageOptions.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
            };
            string? url = await s3Client.GetPreSignedURLAsync(request);
            return useExternal
                ? ReplaceEndpoint(url)
                : url;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during generate upload url from file with key: {key}", storageKey.Key);
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey storageKey, bool useExternal = true)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = storageKey.Location,
                Key = storageKey.Key,
                Verb = HttpVerb.GET,
                Protocol = _fileStorageOptions.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
                Expires = DateTime.UtcNow.AddHours(_fileStorageOptions.DownloadExpirationHours),
            };
            string? url = await s3Client.GetPreSignedURLAsync(request);
            return useExternal
                ? ReplaceEndpoint(url)
                : url;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during generate download url from file with key: {key}", storageKey.Key);
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<IReadOnlyList<MediaUrl>, Error>> GenerateDownloadUrlsAsync(
        IEnumerable<StorageKey> storageKeys,
        CancellationToken cancellationToken,
        bool useExternal = true)
    {
        try
        {
            var tasks = storageKeys.Select(async storageKey =>
            {
                await _requestsSemaphore.WaitAsync(cancellationToken);

                try
                {
                    var request = new GetPreSignedUrlRequest
                    {
                        BucketName = storageKey.Location,
                        Key = storageKey.Key,
                        Verb = HttpVerb.GET,
                        Protocol = _fileStorageOptions.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
                        Expires = DateTime.UtcNow.AddHours(_fileStorageOptions.DownloadExpirationHours),
                    };
                    string? url = await s3Client.GetPreSignedURLAsync(request);
                    if (useExternal)
                    {
                        url = ReplaceEndpoint(url);
                    }

                    return new MediaUrl(storageKey, url);
                }
                finally
                {
                    _requestsSemaphore.Release();
                }
            });

            return await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during generate download urls");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<UnitResult<Error>> DeleteFileAsync(StorageKey storageKey, CancellationToken cancellationToken)
    {
        try
        {
            var request = new DeleteObjectRequest { BucketName = storageKey.Location, Key = storageKey.Key, };
            await s3Client.DeleteObjectAsync(request, cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during delete file with key: {key}", storageKey.Key);
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<StorageObjectMetadata, Error>> GetAssetMetadataAsync(StorageKey storageKey, CancellationToken cancellationToken)
    {
        try
        {
            var request = new GetObjectMetadataRequest { BucketName = storageKey.Location, Key = storageKey.Key, };
            var response = await s3Client.GetObjectMetadataAsync(request, cancellationToken);
            return new StorageObjectMetadata(
                storageKey,
                response.ContentLength,
                response.ContentType,
                response.ETag,
                response.StorageClass);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during get file metadata with key: {key}", storageKey.Key);
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<UnitResult<Error>> InitializeBucketAsync(string bucketName, CancellationToken cancellationToken)
    {
        try
        {
            bool bucketExist = await AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName);
            if (bucketExist)
            {
                return UnitResult.Success<Error>();
            }

            var putBucketRequest = new PutBucketRequest { BucketName = bucketName };
            await s3Client.PutBucketAsync(putBucketRequest, cancellationToken);

            string policy = S3Helper.BuildPublicReadPolicy(bucketName);
            var putPolicyRequest = new PutBucketPolicyRequest { BucketName = bucketName, Policy = policy };
            await s3Client.PutBucketPolicyAsync(putPolicyRequest, cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during bucket initialization with name: {bucketName}", bucketName);
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string, Error>> StartMultipartUploadAsync(
        StorageKey storageKey,
        string contentType,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new InitiateMultipartUploadRequest
            {
                BucketName = storageKey.Location, Key = storageKey.Key, ContentType = contentType,
            };
            var response = await s3Client.InitiateMultipartUploadAsync(request, cancellationToken);
            return response.UploadId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during start multipart upload");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<UnitResult<Error>> AbortMultipartUploadAsync(
        StorageKey storageKey,
        string uploadId,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new AbortMultipartUploadRequest
            {
                BucketName = storageKey.Location,
                Key = storageKey.Key,
                UploadId = uploadId,
            };

            await s3Client.AbortMultipartUploadAsync(request, cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during abort upload url");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<IReadOnlyList<ChunkUploadUrl>, Error>> GenerateAllChunksUploadUrlsAsync(
        StorageKey storageKey,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken,
        bool useExternal = true)
    {
        try
        {
            var tasks = Enumerable.Range(1, totalChunks).Select(async partNumber =>
            {
                await _requestsSemaphore.WaitAsync(cancellationToken);
                try
                {
                    var request = new GetPreSignedUrlRequest
                    {
                        BucketName = storageKey.Location,
                        Key = storageKey.Key,
                        Verb = HttpVerb.PUT,
                        UploadId = uploadId,
                        PartNumber = partNumber,
                        Expires = DateTime.UtcNow.AddMinutes(_fileStorageOptions.UploadExpirationMinutes),
                        Protocol = _fileStorageOptions.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
                    };
                    string? url = await s3Client.GetPreSignedURLAsync(request);
                    if (useExternal)
                    {
                        url = ReplaceEndpoint(url);
                    }

                    return new ChunkUploadUrl(partNumber, url);
                }
                finally
                {
                    _requestsSemaphore.Release();
                }
            });

            ChunkUploadUrl[] results = await Task.WhenAll(tasks);
            return results;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during generating all chunks upload urls");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string, Error>> CompleteMultipartUploadAsync(
        StorageKey storageKey,
        string uploadId,
        IReadOnlyList<PartETagDto> partETags,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new CompleteMultipartUploadRequest
            {
                BucketName = storageKey.Location,
                Key = storageKey.Key,
                UploadId = uploadId,
                PartETags = partETags.Select(p => new PartETag
                    {
                        ETag = p.ETag,
                        PartNumber = p.PartNumber,
                    })
                    .ToList(),
            };
            var response = await s3Client.CompleteMultipartUploadAsync(request, cancellationToken);
            return response.Key;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during complete multipart upload");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<UnitResult<Error>> UploadFileAsync(
        StorageKey storageKey,
        FileStream fileStream,
        string contentType,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = storageKey.Location,
                Key = storageKey.Value,
                InputStream = fileStream,
                ContentType = contentType ?? "application/octet-stream",
            };

            await s3Client.PutObjectAsync(request, cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during upload file to {key}", storageKey.Value);
            return S3ErrorMapper.ToError(ex);
        }
    }

    private string ReplaceEndpoint(string presignedUrl)
        => presignedUrl.Replace(_fileStorageOptions.Endpoint, _fileStorageOptions.ExternalEndpoint, StringComparison.OrdinalIgnoreCase);
}