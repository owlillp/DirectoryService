using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using FileService.Application.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.S3;

public class S3BucketInitializationService(
    IAmazonS3 s3Client,
    IOptions<FileStorageOptions> s3Options,
    ILogger<S3BucketInitializationService> logger) : BackgroundService
{
    private readonly FileStorageOptions _fileStorageOptions = s3Options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            if (!_fileStorageOptions.RequiredBuckets.Any())
            {
                logger.LogInformation("S3 bucket initialization service failed: required buckets not specified.");
                throw new ArgumentException("RequiredBuckets not specified.");
            }

            logger.LogInformation("S3 bucket initialization service started with buckets: [{buckets}]", string.Join(", ", _fileStorageOptions.RequiredBuckets));

            Task[] tasks = _fileStorageOptions.RequiredBuckets.Select(bucket => InitializeBucketAsync(bucket, stoppingToken)).ToArray();
            await Task.WhenAll(tasks);

            logger.LogInformation("S3 bucket initialization service successful finished...]");
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("S3 bucket initialization cancelled");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Critical error during S3 bucket initialization");
            throw;
        }
    }

    private async Task InitializeBucketAsync(string bucketName, CancellationToken cancellationToken)
    {
        try
        {
            bool bucketExist = await AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName);
            if (bucketExist)
            {
                logger.LogInformation("Bucket '{bucketName}' already exists", bucketName);
                return;
            }

            logger.LogInformation("Creating bucket '{bucketName}'", bucketName);

            var putBucketRequest = new PutBucketRequest { BucketName = bucketName };
            await s3Client.PutBucketAsync(putBucketRequest, cancellationToken);

            logger.LogInformation("Creating policy from bucket '{bucketName}'", bucketName);

            string policy = S3Helper.BuildPublicReadPolicy(bucketName);
            var putPolicyRequest = new PutBucketPolicyRequest { BucketName = bucketName, Policy = policy };
            await s3Client.PutBucketPolicyAsync(putPolicyRequest, cancellationToken);

            logger.LogInformation("Bucket '{bucketName}' successful creating", bucketName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Critical error during bucket '{bucketName}' initialization", bucketName);
            throw;
        }
    }
}