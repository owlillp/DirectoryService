using System.Net.Http.Json;
using FileService.Application.Abstractions;
using FileService.Contracts.Files.Requests;
using FileService.Contracts.Files.Responses;
using FileService.Domain;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.SharedKernel.Failures;
using Shared.SharedKernel.HttpCommunications;

namespace FileService.IntegrationTests.Files;

/// <summary>
/// Integration tests for the POST /files/upload endpoint
/// that initiates a media upload and returns a presigned upload URL.
/// </summary>
public class StartUploadTests(IntegrationTestsWebFactory factory) : FileServiceTestsBase(factory)
{
    private const string VIDEOS_BUCKET = "videos";

    [Fact]
    public async Task StartUpload_valid_request_creates_uploading_asset_and_returns_upload_url()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var contextId = Guid.NewGuid();

        await InitializeBucketAsync(VIDEOS_BUCKET, cancellationToken);

        var request = new StartUploadRequest(
            FileName: "video.mp4",
            AssetType: "video",
            ContentType: "video/mp4",
            Size: 1024,
            Context: "location",
            ContextId: contextId);

        // act
        var response = await AppHttpClient.PostAsJsonAsync("/files/upload", request, cancellationToken);
        var result = await response.HandleResponseAsync<StartUploadResponse>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.MediaAssetId);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.UploadUrl));

        await ExecuteInDb(async dbContext =>
        {
            var asset = await dbContext.MediaAssets.FirstOrDefaultAsync(ma => ma.Id == result.Value.MediaAssetId, cancellationToken);
            Assert.NotNull(asset);
            Assert.Equal(MediaStatus.UPLOADING, asset!.Status);
            Assert.Equal("location", asset.Owner.Context);
            Assert.Equal(contextId, asset.Owner.EntityId);
            Assert.Equal(1024, asset.MediaData.Size);
            Assert.Equal("video", asset.AssetType.ToString().ToLowerInvariant());
        });
    }

    [Fact]
    public async Task StartUpload_with_invalid_asset_type_should_fail_and_persist_nothing()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var request = new StartUploadRequest(
            FileName: "video.mp4",
            AssetType: "document",
            ContentType: "video/mp4",
            Size: 1024,
            Context: "location",
            ContextId: Guid.NewGuid());

        // act
        var response = await AppHttpClient.PostAsJsonAsync("/files/upload", request, cancellationToken);
        var result = await response.HandleResponseAsync<StartUploadResponse>(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);

        await AssertNoMediaAssetsAsync(cancellationToken);
    }

    [Fact]
    public async Task StartUpload_with_invalid_context_should_fail_and_persist_nothing()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var request = new StartUploadRequest(
            FileName: "video.mp4",
            AssetType: "video",
            ContentType: "video/mp4",
            Size: 1024,
            Context: "invalid_context",
            ContextId: Guid.NewGuid());

        // act
        var response = await AppHttpClient.PostAsJsonAsync("/files/upload", request, cancellationToken);
        var result = await response.HandleResponseAsync<StartUploadResponse>(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);

        await AssertNoMediaAssetsAsync(cancellationToken);
    }

    [Fact]
    public async Task StartUpload_with_non_positive_size_should_fail_and_persist_nothing()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var request = new StartUploadRequest(
            FileName: "video.mp4",
            AssetType: "video",
            ContentType: "video/mp4",
            Size: 0,
            Context: "location",
            ContextId: Guid.NewGuid());

        // act
        var response = await AppHttpClient.PostAsJsonAsync("/files/upload", request, cancellationToken);
        var result = await response.HandleResponseAsync<StartUploadResponse>(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);

        await AssertNoMediaAssetsAsync(cancellationToken);
    }

    [Fact]
    public async Task StartUpload_with_invalid_file_name_should_fail_and_persist_nothing()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var request = new StartUploadRequest(
            FileName: "movie",
            AssetType: "video",
            ContentType: "video/mp4",
            Size: 1024,
            Context: "location",
            ContextId: Guid.NewGuid());

        // act
        var response = await AppHttpClient.PostAsJsonAsync("/files/upload", request, cancellationToken);
        var result = await response.HandleResponseAsync<StartUploadResponse>(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);

        await AssertNoMediaAssetsAsync(cancellationToken);
    }

    [Fact]
    public async Task StartUpload_with_empty_context_id_should_fail_and_persist_nothing()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var request = new StartUploadRequest(
            FileName: "video.mp4",
            AssetType: "video",
            ContentType: "video/mp4",
            Size: 1024,
            Context: "location",
            ContextId: Guid.Empty);

        // act
        var response = await AppHttpClient.PostAsJsonAsync("/files/upload", request, cancellationToken);
        var result = await response.HandleResponseAsync<StartUploadResponse>(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);

        await AssertNoMediaAssetsAsync(cancellationToken);
    }

    private async Task InitializeBucketAsync(string bucketName, CancellationToken cancellationToken)
    {
        await using var scope = Services.CreateAsyncScope();
        var storageProvider = scope.ServiceProvider.GetRequiredService<IFileStorageProvider>();

        var initResult = await storageProvider.InitializeBucketAsync(bucketName, cancellationToken);
        Assert.True(initResult.IsSuccess);
    }

    private async Task AssertNoMediaAssetsAsync(CancellationToken cancellationToken)
    {
        await ExecuteInDb(async dbContext =>
        {
            int count = await dbContext.MediaAssets.CountAsync(cancellationToken);
            Assert.Equal(0, count);
        });
    }
}
