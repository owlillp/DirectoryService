namespace Messaging.IntegrationEvents.Files.Events;

public record PreviewDeletedEvent(
    Guid PreviewId,
    Guid EntityId,
    string EntityType);