using FileService.Application.Messaging;
using Wolverine.EntityFrameworkCore;

namespace FileService.Infrastructure.Postgres;

public class OutboxService(IDbContextOutbox<FileServiceDbContext> outbox) : IOutboxService
{
    public async Task PublishAsync<T>(T message)
        where T : class => await outbox.PublishAsync(message);

    public async Task FlushAsync() => await outbox.FlushOutgoingMessagesAsync();
}