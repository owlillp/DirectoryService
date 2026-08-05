namespace FileService.Contracts.Files.Requests;

public record AbortMultipartUploadRequest(Guid FileId, string UploadId);