namespace DirectoryService.Infrastructure.Postgres;

public readonly struct Constants
{
    public static readonly string DATABASE_CONNECTION_STRING = "DirectoryServiceDb";

    public static readonly string BACKGROUND_CLEANUP_SERVICE_OPTIONS_SECTION = "CleanupBackgroundServices";
    public static readonly string DEPARTMENTS_CLEANUP_OPTIONS_SECTION = "CleanupServices:Departments";
    public static readonly string POSITIONS_CLEANUP_OPTIONS_SECTION = "CleanupServices:Locations";
    public static readonly string LOCATIONS_CLEANUP_OPTIONS_SECTION = "CleanupServices:Positions";
}