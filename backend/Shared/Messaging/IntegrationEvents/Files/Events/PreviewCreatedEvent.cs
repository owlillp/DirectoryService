namespace Messaging.IntegrationEvents.Files.Events;

public record PreviewCreatedEvent(
    Guid PreviewId,
    Guid EntityId,
    string EntityType);