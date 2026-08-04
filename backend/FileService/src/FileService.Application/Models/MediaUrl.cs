using FileService.Domain;

namespace FileService.Application.Models;

public record MediaUrl(StorageKey StorageKey, string PresignedUrl);