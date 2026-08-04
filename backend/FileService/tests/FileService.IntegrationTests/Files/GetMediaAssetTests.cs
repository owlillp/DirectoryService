using System.Net.Http.Json;
using FileService.Contracts.Files.Dtos;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel.Failures;
using Shared.SharedKernel.HttpCommunications;

namespace FileService.IntegrationTests.Files;

/// <summary>
/// Integration tests for the GET /files/{fileId:guid} endpoint
/// that returns a single media asset by id.
/// </summary>
public class GetMediaAssetTests(IntegrationTestsWebFactory factory) : FileServiceTestsBase(factory)
{
    private static readonly Guid ContextId = Guid.NewGuid();

    [Fact]
    public async Task GetMediaAsset_uploaded_file_returns_details_and_download_url()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        Guid fileId = await SeedAssetAsync(MediaStatus.UPLOADED, cancellationToken);

        // act
        var getFileResponse = await AppHttpClient.GetAsync($"/files/{fileId}", cancellationToken);
        var getFileResult = await getFileResponse.HandleResponseAsync<GetMediaAssetDto>(cancellationToken);

        // assert
        Assert.True(getFileResult.IsSuccess);

        var dto = getFileResult.Value;
        Assert.NotNull(dto);
        Assert.Equal(fileId, dto.Id);
        Assert.Equal(MediaStatus.UPLOADED.ToString().ToLowerInvariant(), dto.Status);
        Assert.Equal(AssetType.VIDEO.ToString().ToLowerInvariant(), dto.AssetType);
        Assert.Equal("location", dto.Context);
        Assert.Equal(ContextId, dto.ContextId);
        Assert.NotEqual(default, dto.CreatedAt);
        Assert.NotEqual(default, dto.UpdatedAt);
        Assert.False(string.IsNullOrWhiteSpace(dto.Url));
        Assert.Contains("videos", dto.Url);
    }

    [Fact]
    public async Task GetMediaAsset_uploading_file_returns_details_without_url()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        Guid fileId = await SeedAssetAsync(MediaStatus.UPLOADING, cancellationToken);

        // act
        var getFileResponse = await AppHttpClient.GetAsync($"/files/{fileId}", cancellationToken);
        var getFileResult = await getFileResponse.HandleResponseAsync<GetMediaAssetDto>(cancellationToken);

        // assert
        Assert.True(getFileResult.IsSuccess);

        var dto = getFileResult.Value;
        Assert.NotNull(dto);
        Assert.Equal(fileId, dto.Id);
        Assert.Equal(MediaStatus.UPLOADING.ToString().ToLowerInvariant(), dto.Status);
        Assert.Null(dto.Url);
    }

    [Fact]
    public async Task GetMediaAsset_failed_file_returns_details_without_url()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        Guid fileId = await SeedAssetAsync(MediaStatus.FAILED, cancellationToken);

        // act
        var getFileResponse = await AppHttpClient.GetAsync($"/files/{fileId}", cancellationToken);
        var getFileResult = await getFileResponse.HandleResponseAsync<GetMediaAssetDto>(cancellationToken);

        // assert
        Assert.True(getFileResult.IsSuccess);

        var dto = getFileResult.Value;
        Assert.NotNull(dto);
        Assert.Equal(fileId, dto.Id);
        Assert.Equal(MediaStatus.FAILED.ToString().ToLowerInvariant(), dto.Status);
        Assert.Null(dto.Url);
    }

    [Fact]
    public async Task GetMediaAsset_of_non_existing_file_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var fileId = Guid.NewGuid();

        // act
        var getFileResponse = await AppHttpClient.GetAsync($"/files/{fileId}", cancellationToken);
        var getFileResult = await getFileResponse.HandleResponseAsync<GetMediaAssetDto>(cancellationToken);

        // assert
        Assert.True(getFileResult.IsFailure);
        Assert.NotNull(getFileResult.Error);
        Assert.Contains(getFileResult.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task GetMediaAsset_deleted_file_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        Guid fileId = await SeedAssetAsync(MediaStatus.DELETED, cancellationToken);

        // act
        var getFileResponse = await AppHttpClient.GetAsync($"/files/{fileId}", cancellationToken);
        var getFileResult = await getFileResponse.HandleResponseAsync<GetMediaAssetDto>(cancellationToken);

        // assert
        Assert.True(getFileResult.IsFailure);
        Assert.NotNull(getFileResult.Error);
        Assert.Contains(getFileResult.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task GetMediaAsset_with_non_guid_route_segment_is_not_resolved()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        // act
        var getFileResponse = await AppHttpClient.GetAsync("/files/not-a-guid", cancellationToken);
        var getFileResult = await getFileResponse.HandleResponseAsync<GetMediaAssetDto>(cancellationToken);

        // assert
        Assert.True(getFileResult.IsFailure);
    }

    /// <summary>
    /// Seeds a VideoAsset directly into the DB with the given status and returns its id.
    /// </summary>
    private async Task<Guid> SeedAssetAsync(MediaStatus status, CancellationToken cancellationToken)
    {
        var fileId = Guid.NewGuid();

        await ExecuteInDb(async dbContext =>
        {
            var mediaData = MediaData.Create(
                    FileName.Create("movie.mp4").Value,
                    ContentType.Create("video/mp4").Value,
                    1024,
                    4)
                .Value;

            var owner = MediaOwner.ForLocation(ContextId).Value;
            var assetResult = VideoAsset.CreateForUpload(fileId, mediaData, owner);
            Assert.True(assetResult.IsSuccess);

            var asset = assetResult.Value;
            switch (status)
            {
                case MediaStatus.UPLOADED:
                    asset.MarkUploaded();
                    break;
                case MediaStatus.FAILED:
                    asset.MarkFailed();
                    break;
                case MediaStatus.DELETED:
                    asset.MarkDeleted();
                    break;
                case MediaStatus.UPLOADING:
                default:
                    break;
            }

            dbContext.MediaAssets.Add(asset);
            await dbContext.SaveChangesAsync(cancellationToken);
        });

        return fileId;
    }
}
