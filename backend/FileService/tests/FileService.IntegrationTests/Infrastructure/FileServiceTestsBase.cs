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
}
