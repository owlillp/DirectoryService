using Core.Abstractions;

namespace FileService.Application.Features.Queries.GetMediaAsset;

public record GetMediaAssetQuery(Guid FileId) : IQuery;