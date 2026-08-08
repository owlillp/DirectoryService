using CSharpFunctionalExtensions;
using FileService.Contracts.Files.Dtos;
using FileService.Contracts.Files.Requests;
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
            var response = await httpClient.GetAsync($"files/entity?Context={request.Context}&EntityId={request.EntityId}", cancellationToken);
            return await response.HandleResponseAsync<GetMediaAssetDto>(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting media asset fro entity with id: {entityId}",  request.EntityId);
            return Error.Failure("server.internal", "Failed to get media asset for entity").ToErrors();
        }
    }
}