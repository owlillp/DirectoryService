using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Postgres;

public class DirectoryServiceDbContext(DbContextOptions<DirectoryServiceDbContext> options) : DbContext(options), IReadDbContext
{
    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Location> Locations => Set<Location>();

    public DbSet<Position> Positions => Set<Position>();

    public DbSet<DepartmentLocation> DepartmentLocations => Set<DepartmentLocation>();

    public DbSet<DepartmentPosition> DepartmentPositions => Set<DepartmentPosition>();

    public IQueryable<Department> DepartmentsRead
        => Set<Department>()
            .Where(d => d.IsActive)
            .AsQueryable()
            .AsNoTracking();

    public IQueryable<DepartmentLocation> DepartmentLocationsRead
        => Set<DepartmentLocation>()
            .AsQueryable()
            .AsNoTracking();

    public IQueryable<DepartmentPosition> DepartmentPositionsRead
        => Set<DepartmentPosition>()
            .AsQueryable()
            .AsNoTracking();

    public IQueryable<Location> LocationsRead
        => Set<Location>()
            .Where(l => l.IsActive)
            .AsQueryable()
            .AsNoTracking();

    public IQueryable<Position> PositionsRead
        => Set<Position>()
            .Where(p => p.IsActive)
            .AsQueryable()
            .AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("ltree");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DirectoryServiceDbContext).Assembly);
    }
}