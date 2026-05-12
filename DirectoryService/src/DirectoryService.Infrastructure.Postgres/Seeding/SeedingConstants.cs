namespace DirectoryService.Infrastructure.Postgres.Seeding;

internal static class SeedingConstants
{
    public const int BATCH_SIZE = 50;

    public const int CLEANUP_ORDER = 0;
    public const int LOCATIONS_ORDER = 1;
    public const int POSITIONS_ORDER = 2;
    public const int DEPARTMENTS_ORDER = 3;
    public const int DEPARTMENT_POSITIONS_ORDER = 4;

    public const int LOCATIONS_COUNT = 100;
    public const int POSITIONS_COUNT = 50;
    public const int ROOT_DEPARTMENTS_COUNT = 12;
    public const int NESTED_DEPARTMENT_LEVELS_COUNT = 4;
    public const int MIN_CHILD_DEPARTMENTS_PER_PARENT = 0;
    public const int MAX_CHILD_DEPARTMENTS_PER_PARENT = 3;
    public const int MAX_TOTAL_DEPARTMENTS = 200;
    public const int MIN_LOCATIONS_PER_DEPARTMENT = 1;
    public const int MAX_LOCATIONS_PER_DEPARTMENT = 3;
    public const int MIN_POSITIONS_PER_DEPARTMENT = 2;
    public const int MAX_POSITIONS_PER_DEPARTMENT = 20;
    public const int TIMEZONE_POOL_SIZE = 100;
}
