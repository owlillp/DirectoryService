namespace FileService.Contracts.Files.Requests;

public record GetFilesForEntityRequest(string Context, Guid EntityId);