using TimeZoneConverter;

namespace DirectoryService.Infrastructure.Postgres.Seeding;

internal static class SeedingTemplates
{
    public static readonly string[] Countries =
    [
        "Poland", "Germany", "France", "Spain", "Italy", "Netherlands", "Sweden", "Norway",
    ];

    public static readonly string[] Cities =
    [
        "Warsaw", "Berlin", "Paris", "Madrid", "Rome", "Amsterdam", "Stockholm", "Oslo",
        "Gdansk", "Munich", "Lyon", "Valencia", "Milan", "Rotterdam", "Malmo", "Bergen",
    ];

    public static readonly string[] Streets =
    [
        "Central", "Oak", "Maple", "Pine", "Lake", "River", "Hill", "Park",
        "Sunset", "Bridge", "Market", "Station", "Garden", "Forest", "Harbor", "North",
    ];

    public static readonly string[] DepartmentNames =
    [
        "Engineering", "Operations", "Finance", "Sales", "Support", "Marketing", "Security", "HumanResources",
    ];

    public static readonly string[] PositionNames =
    [
        "Engineer", "Manager", "Analyst", "Specialist", "Coordinator", "Lead", "Consultant", "Administrator",
    ];

    public static readonly string[] Timezones = TZConvert.KnownIanaTimeZoneNames
        .Take(SeedingConstants.TIMEZONE_POOL_SIZE)
        .ToArray();

    public static string GetAlphabeticalSuffix(int index)
    {
        int value = index + 1;
        var chars = new Stack<char>();

        while (value > 0)
        {
            value--;
            chars.Push((char)('A' + (value % 26)));
            value /= 26;
        }

        return new string(chars.ToArray());
    }
}
