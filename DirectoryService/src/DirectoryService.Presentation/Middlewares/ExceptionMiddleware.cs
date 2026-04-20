using DirectoryService.Application.Exceptions;
using DirectoryService.Presentation.EndpointResults;
using Shared.Failures;

namespace DirectoryService.Presentation.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception e)
        {
            await HandleExceptionAsync(context, e);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(
            exception,
            "Unhandled exception with message: {message} of method: {method} for path: {path}",
            exception.Message, context.Request.Method, context.Request.Path);

        (int statusCode, Error error) = exception switch
        {
            NotFoundException ex => (StatusCodes.Status404NotFound, ex.Error),
            BadRequestException ex => (StatusCodes.Status400BadRequest, ex.Error),
            FailureException ex => (StatusCodes.Status500InternalServerError, ex.Error),
            ConflictException ex => (StatusCodes.Status409Conflict, ex.Error),
            _ => (StatusCodes.Status500InternalServerError, GeneralErrors.Failure())
        };

        var envelope = Envelope.Failure(error);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(envelope);
    }
}

public static class ExceptionMiddlewareExtension
{
    public static IApplicationBuilder UseExceptionMiddleware(this WebApplication app)
        => app.UseMiddleware<ExceptionMiddleware>();
}