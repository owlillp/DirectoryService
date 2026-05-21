using System.Net.Http.Json;
using System.Text.Json;
using CSharpFunctionalExtensions;
using Shared.EndpointResults;
using Shared.Failures;
using Shared.Serializations;

namespace Shared.HttpCommunication;

public static class HttpResponseMessageExtensions
{
    public static async Task<Result<TResponse, Errors>> HandleResponseAsync<TResponse>(
        this HttpResponseMessage httpResponse,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpResponse.Content.ReadFromJsonAsync<Envelope<TResponse>>(JsonOptionsProvider.Options, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                return response?.Errors ?? Error.Failure("http.error", "Error while reading http response");
            }

            if (response == null)
            {
                return Error.Failure("http.error", "Error while reading http response").ToErrors();
            }

            if (response is { IsFailure: true, Errors: not null })
            {
                return response.Errors;
            }

            if (response.Result == null)
            {
                return Error.Failure("http.error", "Error while reading http response").ToErrors();
            }

            return response.Result;
        }
        catch(Exception ex)
        {
            return Error.Failure("http.error", $"Unexpected error while reading http response: {ex.Message}").ToErrors();
        }
    }

    public static async Task<UnitResult<Errors>> HandleResponseAsync(
        this HttpResponseMessage httpResponse,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpResponse.Content.ReadFromJsonAsync<Envelope>(JsonOptionsProvider.Options, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                return response?.Errors ?? Error.Failure("http.error", "Error while reading http response");
            }

            if (response == null)
            {
                return Error.Failure("http.error", "Error while reading http response").ToErrors();
            }

            if (response is { IsFailure: true, Errors: not null })
            {
                return response.Errors;
            }

            return UnitResult.Success<Errors>();
        }
        catch(Exception ex)
        {
            return Error.Failure("http.error", $"Unexpected error while reading http response: {JsonSerializer.Serialize(ex)}").ToErrors();
        }
    }
}