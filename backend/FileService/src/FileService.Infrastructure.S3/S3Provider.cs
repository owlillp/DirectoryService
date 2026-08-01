using Amazon.S3;

namespace FileService.Infrastructure.S3;

public interface IS3Provider
{
}

public class S3Provider(IAmazonS3 s3Client) : IS3Provider
{
}