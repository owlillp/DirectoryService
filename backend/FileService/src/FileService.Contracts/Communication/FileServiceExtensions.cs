using System.Net;
using FileService.Contracts.Communication.HttpCommunication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace FileService.Contracts.Communication;

public static class FileServiceExtensions
{
    public static IServiceCollection AddFileServiceHttpCommunication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileServiceOptions>(configuration.GetSection(nameof(FileServiceOptions)));

        services.AddHttpClient<IFileCommunicationService, FileHttpClient>((sp, config) =>
        {
            var options = sp.GetRequiredService<IOptions<FileServiceOptions>>().Value;

            config.BaseAddress = new Uri(options.Url);
        })
        .AddResilienceHandler("file-service", static (builder, context) =>
        {
            var options = context.ServiceProvider.GetRequiredService<IOptions<FileServiceOptions>>().Value;
            var attemptTimeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

            builder.AddTimeout(attemptTimeout);
            builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(400),
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutRejectedException>()
                    .HandleResult(r => IsTransientHttpStatus(r.StatusCode)),
            });

            builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                MinimumThroughput = 4,
                SamplingDuration = TimeSpan.FromSeconds(10),
                FailureRatio = 0.5,
                BreakDuration = TimeSpan.FromSeconds(15),
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(r => IsTransientHttpStatus(r.StatusCode)),
            });
        });

        return services;
    }

    private static bool IsTransientHttpStatus(HttpStatusCode statusCode)
        => statusCode is
            HttpStatusCode.RequestTimeout or
            HttpStatusCode.InternalServerError or
            HttpStatusCode.BadGateway or
            HttpStatusCode.ServiceUnavailable or
            HttpStatusCode.GatewayTimeout;
}