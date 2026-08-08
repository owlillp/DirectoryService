using CSharpFunctionalExtensions;
using FileService.Contracts.Files.Dtos;
using FileService.Contracts.Files.Requests;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;
using Shared.SharedKernel.HttpCommunications;

namespace FileService.Contracts.Communication.HttpCommunication;

internal sealed class FileHttpClient(
    ILogger<FileHttpClient> logger,
    HttpClient httpClient) : IFileCommunicationService
{
    public async Task<Result<GetMediaAssetDto, Errors>> GetMediaAssetAsync(GetFilesForEntityRequest request, CancellationToken cancellationToken)
    {
        try
        {
            string uri = QueryHelpers.AddQueryString(
                "files/entity",
                new Dictionary<string, string?>
                {
                    [nameof(GetFilesForEntityRequest.Context)] = request.Context,
                    [nameof(GetFilesForEntityRequest.EntityId)] = request.EntityId.ToString(),
                });
            var response = await httpClient.GetAsync(uri, cancellationToken);
            return await response.HandleResponseAsync<GetMediaAssetDto>(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting media asset for entity with id: {entityId}",  request.EntityId);
            return Error.Failure("server.internal", "Failed to get media asset for entity").ToErrors();
        }
    }

    public async Task<Result<bool, Errors>> CheckFileExistAsync(Guid fileId, string? mediaType, CancellationToken cancellationToken)
    {
        try
        {
            string uri = QueryHelpers.AddQueryString(
                $"files/{fileId}/exist",
                new Dictionary<string, string?>
                {
                    ["mediaType"] = mediaType,
                });
            var response = await httpClient.GetAsync(uri, cancellationToken);
            return await response.HandleResponseAsync<bool>(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error check exist media asset with id: {fileId}",  fileId);
            return Error.Failure("server.internal", "Failed to check exist file").ToErrors();
        }
    }
}