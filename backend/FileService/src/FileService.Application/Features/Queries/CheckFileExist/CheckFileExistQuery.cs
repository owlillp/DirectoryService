using Core.Abstractions;

namespace FileService.Application.Features.Queries.CheckFileExist;

public record CheckFileExistQuery(Guid FileId, string? AssetType) : IQuery;