namespace Messaging.IntegrationEvents.Files.Events;

public record VideoCreatedEvent(
    Guid VideoId,
    Guid EntityId,
    string EntityType);