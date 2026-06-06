using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Shared.SharedKernel.Serializations;

namespace Framework.Serializations;

public static class SerializationExtensions
{
    public static IServiceCollection AddJsonOptions(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.SerializerOptions.Converters.Add(new ErrorsJsonConverter());
        });
        return services;
    }
}