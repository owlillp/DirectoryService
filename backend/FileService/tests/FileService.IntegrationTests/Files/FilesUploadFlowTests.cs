using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using FileService.Application.Abstractions;
using FileService.Contracts.Files.Dtos;
using FileService.Contracts.Files.Requests;
using FileService.Contracts.Files.Responses;
using FileService.Domain;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.SharedKernel.Failures;
using Shared.SharedKernel.HttpCommunications;

namespace FileService.IntegrationTests.Files;

public class FilesUploadFlowTests(IntegrationTestsWebFactory factory) : FileServiceTestsBase(factory)
{
    private const string VIDEOS_BUCKET = "videos";
    private const string PICTURES_BUCKET = "pictures";

    private static readonly byte[] VideoContent = Encoding.UTF8.GetBytes("fake video content");
    private static readonly byte[] PreviewContent = Encoding.UTF8.GetBytes("fake image content");

    [Fact]
    public async Task Video_StartUpload_CompleteUpload_GetFile_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        await InitializeBucketAsync(VIDEOS_BUCKET, cancellationToken);

        var request = new StartUploadRequest(
            FileName: "video.mp4",
            AssetType: "video",
            ContentType: "video/mp4",
            Size: VideoContent.Length,
            Context: "location",
            ContextId: Guid.NewGuid());

        // act
        var startResponse = await AppHttpClient.PostAsJsonAsync("/files/upload", request, cancellationToken);
        var startResult = await startResponse.HandleResponseAsync<StartUploadResponse>(cancellationToken);

        // assert
        Assert.True(startResult.IsSuccess);
        Guid fileId = startResult.Value.MediaAssetId;
        Assert.NotEqual(Guid.Empty, fileId);
        Assert.False(string.IsNullOrWhiteSpace(startResult.Value.UploadUrl));

        var uploadSucceeded = await UploadContentAsync(startResult.Value.UploadUrl, VideoContent, "video/mp4", cancellationToken);
        Assert.True(uploadSucceeded);

        var completeResponse = await AppHttpClient.PostAsJsonAsync($"/files/{fileId}/complete", new { }, cancellationToken);
        var completeResult = await completeResponse.HandleResponseAsync(cancellationToken);

        Assert.True(completeResult.IsSuccess);

        var getFileResponse = await AppHttpClient.GetAsync($"/files/{fileId}", cancellationToken);
        var getFileResult = await getFileResponse.HandleResponseAsync<GetMediaAssetDto>(cancellationToken);

        Assert.True(getFileResult.IsSuccess);
        Assert.Equal(fileId, getFileResult.Value.Id);
        Assert.Equal(MediaStatus.UPLOADED.ToString().ToLowerInvariant(), getFileResult.Value.Status);
        Assert.False(string.IsNullOrWhiteSpace(getFileResult.Value.Url));

        // downloaded content should match uploaded one
        var downloaded = await DownloadContentAsync(getFileResult.Value.Url, cancellationToken);
        Assert.Equal(VideoContent, downloaded);

        await ExecuteInDb(async dbContext =>
        {
            var asset = await dbContext.MediaAssets.FirstOrDefaultAsync(ma => ma.Id == fileId, cancellationToken);
            Assert.NotNull(asset);
            Assert.Equal(MediaStatus.UPLOADED, asset!.Status);
            Assert.Equal("video", asset.AssetType.ToString().ToLowerInvariant());
        });
    }

    [Fact]
    public async Task Preview_StartUpload_CompleteUpload_GetFile_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        await InitializeBucketAsync(PICTURES_BUCKET, cancellationToken);

        var request = new StartUploadRequest(
            FileName: "preview.png",
            AssetType: "preview",
            ContentType: "image/png",
            Size: PreviewContent.Length,
            Context: "department",
            ContextId: Guid.NewGuid());

        // act
        var startResponse = await AppHttpClient.PostAsJsonAsync("/files/upload", request, cancellationToken);
        var startResult = await startResponse.HandleResponseAsync<StartUploadResponse>(cancellationToken);

        // assert
        Assert.True(startResult.IsSuccess);
        Guid fileId = startResult.Value.MediaAssetId;

        var uploadSucceeded = await UploadContentAsync(startResult.Value.UploadUrl, PreviewContent, "image/png", cancellationToken);
        Assert.True(uploadSucceeded);

        var completeResponse = await AppHttpClient.PostAsJsonAsync($"/files/{fileId}/complete", new { }, cancellationToken);
        var completeResult = await completeResponse.HandleResponseAsync(cancellationToken);

        Assert.True(completeResult.IsSuccess);

        var getFileResponse = await AppHttpClient.GetAsync($"/files/{fileId}", cancellationToken);
        var getFileResult = await getFileResponse.HandleResponseAsync<GetMediaAssetDto>(cancellationToken);

        Assert.True(getFileResult.IsSuccess);
        Assert.Equal(fileId, getFileResult.Value.Id);
        Assert.Equal(MediaStatus.UPLOADED.ToString().ToLowerInvariant(), getFileResult.Value.Status);
        Assert.False(string.IsNullOrWhiteSpace(getFileResult.Value.Url));

        var downloaded = await DownloadContentAsync(getFileResult.Value.Url, cancellationToken);
        Assert.Equal(PreviewContent, downloaded);

        await ExecuteInDb(async dbContext =>
        {
            var asset = await dbContext.MediaAssets.FirstOrDefaultAsync(ma => ma.Id == fileId, cancellationToken);
            Assert.NotNull(asset);
            Assert.Equal(MediaStatus.UPLOADED, asset!.Status);
            Assert.Equal("preview", asset.AssetType.ToString().ToLowerInvariant());
        });
    }

    [Fact]
    public async Task StartUpload_with_invalid_asset_type_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var request = new StartUploadRequest(
            FileName: "video.mp4",
            AssetType: "document",
            ContentType: "video/mp4",
            Size: 10,
            Context: "location",
            ContextId: Guid.NewGuid());

        // act
        var startResponse = await AppHttpClient.PostAsJsonAsync("/files/upload", request, cancellationToken);
        var startResult = await startResponse.HandleResponseAsync<StartUploadResponse>(cancellationToken);

        // assert
        Assert.True(startResult.IsFailure);
        Assert.NotNull(startResult.Error);
        Assert.Contains(startResult.Error, e => e.Type == ErrorType.VALIDATION);

        await AssertNoMediaAssetsAsync(cancellationToken);
    }

    [Fact]
    public async Task StartUpload_with_invalid_context_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var request = new StartUploadRequest(
            FileName: "video.mp4",
            AssetType: "video",
            ContentType: "video/mp4",
            Size: 10,
            Context: "invalid_context",
            ContextId: Guid.NewGuid());

        // act
        var startResponse = await AppHttpClient.PostAsJsonAsync("/files/upload", request, cancellationToken);
        var startResult = await startResponse.HandleResponseAsync<StartUploadResponse>(cancellationToken);

        // assert
        Assert.True(startResult.IsFailure);
        Assert.NotNull(startResult.Error);
        Assert.Contains(startResult.Error, e => e.Type == ErrorType.VALIDATION);

        await AssertNoMediaAssetsAsync(cancellationToken);
    }

    [Fact]
    public async Task StartUpload_with_invalid_size_should_fail()
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
        var startResponse = await AppHttpClient.PostAsJsonAsync("/files/upload", request, cancellationToken);
        var startResult = await startResponse.HandleResponseAsync<StartUploadResponse>(cancellationToken);

        // assert
        Assert.True(startResult.IsFailure);
        Assert.NotNull(startResult.Error);
        Assert.Contains(startResult.Error, e => e.Type == ErrorType.VALIDATION);

        await AssertNoMediaAssetsAsync(cancellationToken);
    }

    [Fact]
    public async Task GetFile_before_complete_returns_metadata_without_url()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        Guid fileId = await StartUploadOnlyAsync(cancellationToken);

        // act
        var getFileResponse = await AppHttpClient.GetAsync($"/files/{fileId}", cancellationToken);
        var getFileResult = await getFileResponse.HandleResponseAsync<GetMediaAssetDto>(cancellationToken);

        // assert
        Assert.True(getFileResult.IsSuccess);
        Assert.Equal(fileId, getFileResult.Value.Id);
        Assert.Equal(MediaStatus.UPLOADING.ToString().ToLowerInvariant(), getFileResult.Value.Status);
        Assert.Null(getFileResult.Value.Url);

        await ExecuteInDb(async dbContext =>
        {
            var asset = await dbContext.MediaAssets.FirstOrDefaultAsync(ma => ma.Id == fileId, cancellationToken);
            Assert.NotNull(asset);
            Assert.Equal(MediaStatus.UPLOADING, asset!.Status);
        });
    }

    [Fact]
    public async Task CompleteUpload_without_uploading_content_should_fail_and_mark_failed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        await InitializeBucketAsync(VIDEOS_BUCKET, cancellationToken);

        Guid fileId = await StartUploadOnlyAsync(cancellationToken);

        // act (no content uploaded to the presigned url)
        var completeResponse = await AppHttpClient.PostAsJsonAsync($"/files/{fileId}/complete", new { }, cancellationToken);
        var completeResult = await completeResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(completeResult.IsFailure);

        await ExecuteInDb(async dbContext =>
        {
            var asset = await dbContext.MediaAssets.FirstOrDefaultAsync(ma => ma.Id == fileId, cancellationToken);
            Assert.NotNull(asset);
            Assert.Equal(MediaStatus.FAILED, asset!.Status);
        });
    }

    [Fact]
    public async Task CompleteUpload_of_non_existing_file_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var fileId = Guid.NewGuid();

        // act
        var completeResponse = await AppHttpClient.PostAsJsonAsync($"/files/{fileId}/complete", new { }, cancellationToken);
        var completeResult = await completeResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(completeResult.IsFailure);
        Assert.NotNull(completeResult.Error);
        Assert.Contains(completeResult.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task GetFile_of_non_existing_file_should_fail()
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

    private static async Task<bool> UploadContentAsync(
        string uploadUrl,
        byte[] content,
        string contentType,
        CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();

        using var uploadContent = new ByteArrayContent(content);
        uploadContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        var uploadResponse = await httpClient.PutAsync(uploadUrl, uploadContent, cancellationToken);
        return uploadResponse.IsSuccessStatusCode;
    }

    private static async Task<byte[]> DownloadContentAsync(string downloadUrl, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();

        var downloadResponse = await httpClient.GetAsync(downloadUrl, cancellationToken);
        Assert.True(downloadResponse.IsSuccessStatusCode);

        return await downloadResponse.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    private async Task<Guid> StartUploadOnlyAsync(CancellationToken cancellationToken)
    {
        var request = new StartUploadRequest(
            FileName: "video.mp4",
            AssetType: "video",
            ContentType: "video/mp4",
            Size: VideoContent.Length,
            Context: "location",
            ContextId: Guid.NewGuid());

        var startResponse = await AppHttpClient.PostAsJsonAsync("/files/upload", request, cancellationToken);
        var startResult = await startResponse.HandleResponseAsync<StartUploadResponse>(cancellationToken);

        Assert.True(startResult.IsSuccess);

        return startResult.Value.MediaAssetId;
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
