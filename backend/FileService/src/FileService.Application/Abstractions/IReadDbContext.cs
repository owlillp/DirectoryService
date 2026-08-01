using FileService.Domain.Assets;

namespace FileService.Application.Abstractions;

public interface IReadDbContext
{
    IQueryable<MediaAsset> MediaAssetsRead { get; }
}