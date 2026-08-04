using FileService.Contracts.Files.Dtos;

namespace FileService.Contracts.Files.Responses;

public record GetFilesForEntityResponse(string Context, Guid Id, GetMediaAssetsDto[] FileInfos);