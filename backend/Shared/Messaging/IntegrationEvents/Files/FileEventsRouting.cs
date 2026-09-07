namespace Messaging.IntegrationEvents.Files;

public static class FileEventsRouting
{
    public const string EXCHANGE = "file-events";

    public static class RoutingKeys
    {
        public const string ALL_DEPARTMENT_EVENTS = "*.*.department";

        public static string VideoCreated(string entityType) => $"video.created.{entityType.ToLowerInvariant()}";

        public static string VideoDeleted(string entityType) => $"video.deleted.{entityType.ToLowerInvariant()}";

        public static string PreviewCreated(string entityType) => $"preview.created.{entityType.ToLowerInvariant()}";

        public static string PreviewDeleted(string entityType) => $"preview.deleted.{entityType.ToLowerInvariant()}";
    }
}