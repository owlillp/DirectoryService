using Core.Abstractions;

namespace FileService.Application.Features.Queries.GetFile;

public record GetFileQuery(Guid FileId) : IQuery;