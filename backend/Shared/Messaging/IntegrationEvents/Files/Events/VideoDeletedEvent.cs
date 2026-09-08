namespace Messaging.IntegrationEvents.Files.Events;

public record VideoDeletedEvent(
    Guid VideoId,
    Guid EntityId,
    string EntityType);