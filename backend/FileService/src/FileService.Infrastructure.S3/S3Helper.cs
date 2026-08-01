namespace FileService.Infrastructure.S3;

public static class S3Helper
{
    public static string BuildPublicReadPolicy(string bucketName) =>
        $$"""
          {
              "Version": "2012-10-17",
              "Statement": [
                    {
                        "Effect": "Allow",
                        "Principal": {"AWS": ["*"]},
                        "Action": ["s3:GetObject"],
                        "Resource": ["arn:aws:s3:::{{bucketName}}/*"]
                    }
              ]
          }                          
          """;
}