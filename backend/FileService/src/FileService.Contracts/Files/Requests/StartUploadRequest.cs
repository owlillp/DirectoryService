namespace FileService.Contracts.Files.Requests;

public record StartUploadRequest(
    string FileName,
    string AssetType,
    string ContentType,
    long Size,
    string Context,
    Guid ContextId );