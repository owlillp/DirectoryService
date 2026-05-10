using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Postgres.Seeding.Seeders;

public class DepartmentsSeeder(DirectoryServiceDbContext dbContext, ILogger<DepartmentsSeeder> logger) : ISeeder
{
    private int _seededDepartmentsCount;

    public int Order => SeedingConstants.DEPARTMENTS_ORDER;

    public async Task SeedAsync()
    {
        logger.LogInformation("Start seeding departments...");

        await SeedRootDepartmentsAsync();
        await SeedNestedDepartmentsAsync();

        logger.LogInformation("Finish seeding departments");
    }

    private async Task SeedRootDepartmentsAsync()
    {
        int rootCount = Math.Min(SeedingConstants.ROOT_DEPARTMENTS_COUNT, SeedingConstants.MAX_TOTAL_DEPARTMENTS);

        foreach (var batch in Enumerable.Range(0, rootCount).Chunk(SeedingConstants.BATCH_SIZE))
        {
            List<Location> locations = await dbContext.Locations.AsNoTracking().ToListAsync();
            var departments = batch
                .Select(index => CreateRootDepartment(index, locations))
                .ToList();

            await dbContext.Departments.AddRangeAsync(departments);
            await dbContext.SaveChangesAsync();
            dbContext.ChangeTracker.Clear();
            _seededDepartmentsCount += departments.Count;
        }
    }

    private async Task SeedNestedDepartmentsAsync()
    {
        int globalIndex = SeedingConstants.ROOT_DEPARTMENTS_COUNT;

        for (short depth = 1; depth <= SeedingConstants.NESTED_DEPARTMENT_LEVELS_COUNT; depth++)
        {
            if (_seededDepartmentsCount >= SeedingConstants.MAX_TOTAL_DEPARTMENTS)
            {
                break;
            }

            List<Location> locations = await dbContext.Locations.AsNoTracking().ToListAsync();
            List<Department> parents = await dbContext.Departments
                .AsNoTracking()
                .Where(d => d.Depth == depth - 1)
                .ToListAsync();

            if (parents.Count == 0)
            {
                break;
            }

            var levelChildren = new List<Department>();
            foreach (Department parent in parents)
            {
                int childCount = Random.Shared.Next(
                    SeedingConstants.MIN_CHILD_DEPARTMENTS_PER_PARENT,
                    SeedingConstants.MAX_CHILD_DEPARTMENTS_PER_PARENT + 1);

                for (int i = 0; i < childCount; i++)
                {
                    if (_seededDepartmentsCount + levelChildren.Count >= SeedingConstants.MAX_TOTAL_DEPARTMENTS)
                    {
                        break;
                    }

                    levelChildren.Add(CreateChildDepartment(globalIndex++, parent, locations, depth));
                }

                if (_seededDepartmentsCount + levelChildren.Count >= SeedingConstants.MAX_TOTAL_DEPARTMENTS)
                {
                    break;
                }
            }

            foreach (Department[] batch in levelChildren.Chunk(SeedingConstants.BATCH_SIZE))
            {
                await dbContext.Departments.AddRangeAsync(batch);
                await dbContext.SaveChangesAsync();
                dbContext.ChangeTracker.Clear();
                _seededDepartmentsCount += batch.Length;
            }
        }
    }

    private Department CreateRootDepartment(int index, List<Location> locations)
    {
        var departmentId = new DepartmentId(Guid.NewGuid());
        var locationLinks = CreateDepartmentLocations(departmentId, locations);

        string suffix = SeedingTemplates.GetAlphabeticalSuffix(index);
        string nameRaw = $"{SeedingTemplates.DepartmentNames[index % SeedingTemplates.DepartmentNames.Length]} {suffix}";
        string identifierRaw = $"dept{suffix.ToLowerInvariant()}";

        var name = DepartmentName.Create(nameRaw).Value;
        var identifier = DepartmentIdentifier.Create(identifierRaw).Value;

        return Department.CreateParent(name, identifier, locationLinks, departmentId).Value;
    }

    private Department CreateChildDepartment(int index, Department parent, List<Location> locations, short depth)
    {
        var departmentId = new DepartmentId(Guid.NewGuid());
        var locationLinks = CreateDepartmentLocations(departmentId, locations);

        string suffix = SeedingTemplates.GetAlphabeticalSuffix(index);
        string depthToken = SeedingTemplates.GetAlphabeticalSuffix(depth - 1).ToLowerInvariant();
        string nameRaw = $"Sub {SeedingTemplates.DepartmentNames[index % SeedingTemplates.DepartmentNames.Length]} {suffix}";
        string identifierRaw = $"lvl{depthToken}{suffix.ToLowerInvariant()}";

        var name = DepartmentName.Create(nameRaw).Value;
        var identifier = DepartmentIdentifier.Create(identifierRaw).Value;

        return Department.CreateChild(name, identifier, parent, locationLinks, departmentId).Value;
    }

    private List<DepartmentLocation> CreateDepartmentLocations(DepartmentId departmentId, List<Location> locations)
    {
        int count = Random.Shared.Next(
            SeedingConstants.MIN_LOCATIONS_PER_DEPARTMENT,
            SeedingConstants.MAX_LOCATIONS_PER_DEPARTMENT + 1);

        return locations
            .OrderBy(_ => Random.Shared.Next())
            .Take(count)
            .Select(location => new DepartmentLocation(departmentId, location.Id))
            .ToList();
    }
}
