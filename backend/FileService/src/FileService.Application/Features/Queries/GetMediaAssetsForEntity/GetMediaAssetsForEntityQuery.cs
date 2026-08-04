using Core.Abstractions;
using FileService.Contracts.Files.Requests;

namespace FileService.Application.Features.Queries.GetMediaAssetsForEntity;

public record GetMediaAssetsForEntityQuery(GetFilesForEntityRequest Request) : IQuery;