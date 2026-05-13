using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using DirectoryService.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Infrastructure;

public class DirectoryServiceTestsBase(IntegrationTestsWebFactory factory)
    : IClassFixture<IntegrationTestsWebFactory>, IAsyncLifetime
{
    private readonly Func<Task> _resetDatabaseAsync = factory.ResetDatabaseAsync;

    protected HttpClient AppHttpClient { get; init; } = factory.CreateClient();

    protected IServiceProvider Services { get; init; } = factory.Services;

    public Task InitializeAsync()
        => Task.CompletedTask;

    public async Task DisposeAsync()
        => await _resetDatabaseAsync();

    protected async Task<T> ExecuteInDb<T>(Func<DirectoryServiceDbContext, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

        return await action(dbContext);
    }

    protected async Task ExecuteInDb(Func<DirectoryServiceDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

        await action(dbContext);
    }

    protected async Task<Location> CreateLocationAsync(
        string name = "test_location",
        string country = "test_country",
        string city = "test_city",
        string street = "test_street",
        int postalCode = 101,
        int buildingNumber = 1,
        string timeZone = "Europe/Moscow",
        bool isActive = true,
        Guid? id = null)
    {
        return await ExecuteInDb(async dbContext =>
        {
            var locationId = new LocationId(id ?? Guid.NewGuid());
            var locationName = LocationName.Create(name).Value;
            var locationTimeZone = LocationTimezone.Create(timeZone).Value;
            var locationAddress = LocationAddress.Create(
                country,
                city,
                street,
                postalCode,
                buildingNumber)
                .Value;

            var location = Location.Create(locationName, locationAddress, locationTimeZone, locationId);

            location.SetActive(isActive);

            dbContext.Locations.Add(location);
            await dbContext.SaveChangesAsync();

            return location;
        });
    }

    protected async Task<Department> CreateDepartmentAsync(
        IEnumerable<Guid> locationIds,
        string name = "test_name",
        string identifier = "test",
        Department? parent = null,
        bool isActive = true,
        Guid? id = null)
    {
        var locationsList = locationIds.Select(l => new LocationId(l)).ToList();

        return await ExecuteInDb(async dbContext =>
        {
            var departmentId = new DepartmentId(id ?? Guid.NewGuid());
            var departmentName = DepartmentName.Create(name).Value;
            var departmentIdentifier = DepartmentIdentifier.Create(identifier).Value;
            var departmentLocations = locationsList.Select(l => new DepartmentLocation(departmentId, l));

            var department = parent == null
                ? Department.CreateParent(
                    departmentName,
                    departmentIdentifier,
                    departmentLocations,
                    departmentId).Value
                : Department.CreateChild(
                    departmentName,
                    departmentIdentifier,
                    parent,
                    departmentLocations,
                    departmentId).Value;

            department.SetActive(isActive);

            dbContext.Departments.Add(department);
            await dbContext.SaveChangesAsync();

            return department;
        });
    }

    protected async Task<Position> CreatePositionAsync(
        IEnumerable<Guid> departmentsIds,
        string name = "test_name",
        Guid? id = null)
    {
        var departmentsList = departmentsIds.Select(d => new DepartmentId(d)).ToList();

        return await ExecuteInDb(async dbContext =>
        {

            var positionId = new PositionId(id ?? Guid.NewGuid());
            var positionsName = PositionName.Create(name).Value;

            var position = Position.Create(
                positionsName,
                null,
                departmentsList.Select(d => new DepartmentPosition(d, positionId)));

            dbContext.Positions.Add(position);
            await dbContext.SaveChangesAsync();

            return position;
        });
    }
}