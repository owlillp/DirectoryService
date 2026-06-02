namespace Core.Extensions;

public static class TimeOnlyExtensions
{
    public static TimeSpan GetTimeUntil(this TimeOnly source, DateTime? date = null)
    {
        var now = date ?? DateTime.UtcNow;
        var nextRun = now.Date + source.ToTimeSpan();

        if (nextRun <= now)
        {
            nextRun = nextRun.AddDays(1);
        }

        return nextRun - now;
    }
}