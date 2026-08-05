namespace FileService.Contracts.Files.Requests;

public record CompleteMultipartUploadRequest(Guid FileId, string UploadId, IReadOnlyList<PartETagDto> PartETags);