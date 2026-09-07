using Npgsql;
using Wolverine;
using Wolverine.ErrorHandling;

namespace Messaging;

public static class WolverineErrorHandlingExtensions
{
    public static void ConfigureStandardErrorPolicies(this WolverineOptions opts)
    {
        opts.Policies.OnException<NpgsqlException>(ex => ex.IsTransient)
            .RetryWithCooldown(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(100))
            .Then.ScheduleRetry(TimeSpan.FromSeconds(3))
            .Then.ScheduleRetry(TimeSpan.FromSeconds(10));

        opts.Policies.OnException<TimeoutException>()
            .RetryWithCooldown(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(100))
            .Then.ScheduleRetry(TimeSpan.FromSeconds(3))
            .Then.ScheduleRetry(TimeSpan.FromSeconds(15));

        opts.Policies.OnException<HttpRequestException>()
            .RetryWithCooldown(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(100))
            .Then.ScheduleRetry(TimeSpan.FromSeconds(3))
            .Then.ScheduleRetry(TimeSpan.FromSeconds(15));

        opts.Policies.OnException<TaskCanceledException>(ex => ex.InnerException is TimeoutException)
            .RetryWithCooldown(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(100))
            .Then.ScheduleRetry(TimeSpan.FromSeconds(3))
            .Then.ScheduleRetry(TimeSpan.FromSeconds(15));
    }
}