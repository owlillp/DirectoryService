using Messaging.IntegrationEvents.Directories;
using Messaging.IntegrationEvents.Files;
using Messaging.IntegrationEvents.Files.Events;
using Wolverine;
using Wolverine.RabbitMQ;

namespace FileService.Application.Messaging;

public static class RabbitMqConfiguration
{
    private const string FILE_HARD_DELETES_QUEUE = "file.hard-delete";

    extension(WolverineOptions options)
    {
        public void ConfigureRabbitMq(string connectionString)
        {
            options.UseRabbitMq(new Uri(connectionString))
                .AutoProvision()
                .EnableWolverineControlQueues()
                .UseQuorumQueues()
                .DeclareExchange(DirectoryEventRouting.EXCHANGE, exchange =>
                {
                    exchange.ExchangeType = ExchangeType.Fanout;
                    exchange.IsDurable = true;
                })
                .DeclareExchange(FileEventsRouting.EXCHANGE, exchange =>
                {
                    exchange.ExchangeType = ExchangeType.Topic;
                    exchange.IsDurable = true;
                });

            options.ConfigureDirectoryEventsListeners();
            options.ConfigureFileEventPublishing();
        }

        private void ConfigureDirectoryEventsListeners()
        {
            options.ListenToRabbitQueue(FILE_HARD_DELETES_QUEUE, queue =>
            {
                queue.BindExchange(DirectoryEventRouting.EXCHANGE);
            });
        }

        private void ConfigureFileEventPublishing()
        {
            options.PublishMessagesToRabbitMqExchange<VideoCreatedEvent>(
                FileEventsRouting.EXCHANGE,
                m => FileEventsRouting.RoutingKeys.VideoCreated(m.EntityType))
                .UseDurableOutbox();

            options.PublishMessagesToRabbitMqExchange<VideoDeletedEvent>(
                    FileEventsRouting.EXCHANGE,
                    m => FileEventsRouting.RoutingKeys.VideoDeleted(m.EntityType))
                .UseDurableOutbox();

            options.PublishMessagesToRabbitMqExchange<PreviewCreatedEvent>(
                    FileEventsRouting.EXCHANGE,
                    m => FileEventsRouting.RoutingKeys.PreviewCreated(m.EntityType))
                .UseDurableOutbox();

            options.PublishMessagesToRabbitMqExchange<PreviewCreatedEvent>(
                    FileEventsRouting.EXCHANGE,
                    m => FileEventsRouting.RoutingKeys.PreviewCreated(m.EntityType))
                .UseDurableOutbox();
        }
    }
}