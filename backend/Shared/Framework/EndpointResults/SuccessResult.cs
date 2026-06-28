using Microsoft.AspNetCore.Http;
using Shared.SharedKernel;

namespace Framework.EndpointResults;

public class SuccessResult : IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var envelope = Envelope.Success();

        httpContext.Response.StatusCode = StatusCodes.Status200OK;

        return httpContext.Response.WriteAsJsonAsync(envelope);
    }
}

public class SuccessResult<TValue> : IResult
{
    private readonly TValue _value;

    public SuccessResult(TValue value)
        => _value = value;

    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var envelope = Envelope<TValue>.Success(_value);

        httpContext.Response.StatusCode = StatusCodes.Status200OK;

        return httpContext.Response.WriteAsJsonAsync(envelope);
    }
}