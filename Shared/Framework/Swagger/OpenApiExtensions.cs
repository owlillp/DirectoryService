using Microsoft.Extensions.DependencyInjection;
using NJsonSchema;

namespace Framework.Swagger;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApi(this IServiceCollection services, string title, string version)
    {
        services.AddOpenApiDocument(settings =>
        {
            settings.Title = title;
            settings.Version = version;
            settings.SchemaSettings.SchemaType = SchemaType.OpenApi3;
            settings.SchemaSettings.GenerateEnumMappingDescription = true;
            settings.SchemaSettings.SchemaProcessors.Add(new EnvelopeSchemaProcessor());
        });
        return services;
    }
}