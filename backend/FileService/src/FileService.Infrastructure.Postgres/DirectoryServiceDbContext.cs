using FileService.Application.Abstractions;
using FileService.Domain;
using FileService.Domain.Assets;
using Microsoft.EntityFrameworkCore;

namespace FileService.Infrastructure.Postgres;

public class FileServiceDbContext(DbContextOptions<FileServiceDbContext> options) : DbContext(options), IReadDbContext
{
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();

    public IQueryable<MediaAsset> MediaAssetsRead
        => Set<MediaAsset>()
            .Where(ma => ma.Status != MediaStatus.DELETED)
            .AsQueryable()
            .AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(FileServiceDbContext).Assembly);
}