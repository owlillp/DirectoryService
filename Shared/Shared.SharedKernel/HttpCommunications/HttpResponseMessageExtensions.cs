using System.Net.Http.Json;
using System.Text.Json;
using CSharpFunctionalExtensions;
using Shared.SharedKernel.Failures;
using Shared.SharedKernel.Serializations;

namespace Shared.SharedKernel.HttpCommunications;

public static class HttpResponseMessageExtensions
{
    extension(HttpResponseMessage httpResponse)
    {
        public async Task<Result<TResponse, Errors>> HandleResponseAsync<TResponse>(CancellationToken cancellationToken = default)
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

        public async Task<UnitResult<Errors>> HandleResponseAsync(CancellationToken cancellationToken = default)
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
}