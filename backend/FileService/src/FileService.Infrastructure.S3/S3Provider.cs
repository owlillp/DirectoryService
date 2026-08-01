using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.SharedKernel.Failures;

namespace FileService.Infrastructure.S3;

public class S3Provider(
    IAmazonS3 s3Client,
    ILogger<S3Provider> logger,
    IOptions<S3Options> s3Options) : IS3Provider
{
    private readonly S3Options _s3Options = s3Options.Value;

    public async Task<Result<string, Error>> StartMultipartUploadAsync(string bucketName, string key, string contentType)
    {
        try
        {
            var request = new InitiateMultipartUploadRequest
            {
                BucketName = bucketName, Key = key, ContentType = contentType,
            };
            var response = await s3Client.InitiateMultipartUploadAsync(request);
            return response.UploadId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during start multipart upload");
            return S3ErrorMapper.ToError(ex);
        }
    }
}