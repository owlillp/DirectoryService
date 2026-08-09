namespace FileService.Application.Models;

public record CacheOptions
{
    public int LocalCacheExpirationMinutes { get; init; } = 5;
    public int CacheExpirationMinutes { get; init; } = 30;
    public bool EnableRedisCache { get; init; } = true;
}