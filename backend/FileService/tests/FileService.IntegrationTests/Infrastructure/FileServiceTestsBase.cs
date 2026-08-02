using FileService.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.IntegrationTests.Infrastructure;

public class FileServiceTestsBase(IntegrationTestsWebFactory factory)
    : IClassFixture<IntegrationTestsWebFactory>, IAsyncLifetime
{
    private readonly Func<Task> _resetDatabaseAsync = factory.ResetDatabaseAsync;

    protected HttpClient AppHttpClient { get; init; } = factory.CreateClient();

    protected IServiceProvider Services { get; init; } = factory.Services;

    public Task InitializeAsync()
        => Task.CompletedTask;

    public async Task DisposeAsync()
        => await _resetDatabaseAsync();

    protected async Task<T> ExecuteInDb<T>(Func<FileServiceDbContext, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();
        return await action(dbContext);
    }

    protected async Task ExecuteInDb(Func<FileServiceDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();
        await action(dbContext);
    }
}
