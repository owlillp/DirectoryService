using Shared.SharedKernel.Failures;

namespace FileService.Domain;

public static class FileErrors
{
    public static Error BucketNotFound(string? bucketName = null)
    {
        string name = bucketName ?? string.Empty;
        return Error.NotFound("no.such.bucket", $"Bucket {name} not found");
    }

    public static Error UploadNotFound(string? uploadId = null)
    {
        string id = uploadId is null ? string.Empty : $"with ID {uploadId} ";
        return Error.NotFound("upload.not.found", $"Upload session {id}not found");
    }

    public static Error ObjectNotFound(string? objectKey = null)
    {
        string key = objectKey is null ? string.Empty : $"with key {objectKey} ";
        return Error.NotFound("no.such.bucket", $"Object {key}not found");
    }

    public static Error Forbidden()
        => Error.Failure("access.denied", "Access denied");

    public static Error ValidationFailed(string? reason = null)
    {
        string message = "Incorrect values in request";
        if (!string.IsNullOrWhiteSpace(reason))
            message += $" {reason}";

        return Error.Validation("validation.failed", message);
    }

    public static Error InternalServerError()
        => Error.Failure("internal.server.error", "Internal server error");

    public static Error OperationCanceled()
        => Error.Failure("operation.canceled", "Operation canceled");

    public static Error NetworkIssue()
        => Error.Failure("network.issue", "Network issue in process");

    public static Error Unknown()
        => Error.Failure("unknown.error", "Unknown error");
}