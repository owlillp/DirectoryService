using FileService.Contracts.Files.Dtos;

namespace FileService.Contracts.Files.Responses;

public record StartMultipartUploadResponse(
    Guid MediaAssetId,
    string UploadId,
    IReadOnlyList<ChunkUploadUrl> ChunkUploadUrls,
    int ChunkSize);