namespace Messaging.IntegrationEvents.Directories;

public static class DirectoryEventRouting
{
    public const string EXCHANGE = "directory-events";

    public static class RoutingKeys
    {
        public static string DepartmentDeleted() => "deleted.department";

        public static string LocationDeleted() => "deleted.location";

        public static string PositionDeleted() => "deleted.position";
    }
}