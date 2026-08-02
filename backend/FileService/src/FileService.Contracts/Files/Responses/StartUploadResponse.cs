namespace FileService.Contracts.Files.Responses;

public record StartUploadResponse(Guid MediaAssetId, string UploadUrl);