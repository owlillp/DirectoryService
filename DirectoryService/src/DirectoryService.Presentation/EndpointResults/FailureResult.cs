using Shared.Failures;

namespace DirectoryService.Presentation.EndpointResults;

public class FailureResult : IResult
{
    private readonly Errors _errors;

    public FailureResult(Errors errors)
        => _errors = errors;

    public FailureResult(Error error)
        => _errors = error;

    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (!_errors.Any())
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

            return httpContext.Response.WriteAsJsonAsync(Envelope.Failure(_errors));
        }

        var distinctErrorTypes = _errors
            .Select(e => e.Type)
            .Distinct()
            .ToList();

        int statusCode = distinctErrorTypes.Count > 1
            ? StatusCodes.Status500InternalServerError
            : GetStatusCodeFromErrorType(distinctErrorTypes.First());

        var envelope = Envelope.Failure(_errors);
        httpContext.Response.StatusCode = statusCode;

        return httpContext.Response.WriteAsJsonAsync(envelope);
    }

    private int GetStatusCodeFromErrorType(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.VALIDATION => StatusCodes.Status400BadRequest,
            ErrorType.NOT_FOUND => StatusCodes.Status404NotFound,
            ErrorType.FAILURE => StatusCodes.Status500InternalServerError,
            ErrorType.CONFLICT => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}