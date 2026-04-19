using System.Reflection;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http.Metadata;
using Shared.Failures;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace DirectoryService.Presentation.EndpointResults;

public class EndpointResult<T> : IResult, IEndpointMetadataProvider
{
    private readonly IResult _result;

    public EndpointResult(Result<T, Error> result)
    {
        _result = result.IsSuccess
            ? new SuccessResult<T>(result.Value)
            : new FailureResult(result.Error);
    }

    public EndpointResult(Result<T, Errors> result)
    {
        _result = result.IsSuccess
            ? new SuccessResult<T>(result.Value)
            : new FailureResult(result.Error);
    }

    public Task ExecuteAsync(HttpContext httpContext)
        => _result.ExecuteAsync(httpContext);

    public static implicit operator EndpointResult<T>(Result<T, Error> result) => new (result);

    public static implicit operator EndpointResult<T>(Result<T, Errors> result) => new (result);

    public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(builder);

        builder.Metadata.Add(new ProducesResponseTypeMetadata(200, typeof(Envelope<T>), ["application/json"]));

        builder.Metadata.Add(new ProducesResponseTypeMetadata(500, typeof(Envelope<T>), ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(400, typeof(Envelope<T>), ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(404, typeof(Envelope<T>), ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(401, typeof(Envelope<T>), ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(403, typeof(Envelope<T>), ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(409, typeof(Envelope<T>), ["application/json"]));
    }
}