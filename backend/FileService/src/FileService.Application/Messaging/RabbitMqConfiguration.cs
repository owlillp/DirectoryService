using Messaging.IntegrationEvents.Files;
using Messaging.IntegrationEvents.Files.Events;
using Wolverine;
using Wolverine.RabbitMQ;

namespace FileService.Application.Messaging;

public static class RabbitMqConfiguration
{
    extension(WolverineOptions options)
    {
        public void ConfigureRabbitMq(string connectionString)
        {
            options.UseRabbitMq(new Uri(connectionString))
                .AutoProvision()
                .EnableWolverineControlQueues()
                .UseQuorumQueues()
                .DeclareExchange(FileEventsRouting.EXCHANGE, exchange =>
                {
                    exchange.ExchangeType = ExchangeType.Topic;
                    exchange.IsDurable = true;
                });

            options.ConfigureFileEventPublishing();
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