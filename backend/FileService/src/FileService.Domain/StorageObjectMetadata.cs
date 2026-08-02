namespace FileService.Domain;

public sealed record StorageObjectMetadata(
    StorageKey StorageKey,
    long ContentLength,
    string ContentType,
    string ETag,
    string? StorageClass);
