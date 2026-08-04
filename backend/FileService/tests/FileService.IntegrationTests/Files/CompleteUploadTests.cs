using System.Net.Http.Headers;
using System.Net.Http.Json;
using FileService.Application.Abstractions;
using FileService.Contracts.Files.Requests;
using FileService.Contracts.Files.Responses;
using FileService.Domain;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.SharedKernel.HttpCommunications;

namespace FileService.IntegrationTests.Files;

/// <summary>
/// Integration tests for the complete upload endpoint (POST /files/{fileId}/complete).
/// </summary>
public class CompleteUploadTests(IntegrationTestsWebFactory factory) : FileServiceTestsBase(factory)
{
    private const string VIDEOS_BUCKET = "videos";
    private const string CONTENT_TYPE = "video/mp4";

    [Fact]
    public async Task Complete_upload_of_asset_with_matching_content_marks_it_uploaded()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var (fileId, uploadUrl) = await StartUploadAsync(1024, "video.mp4", cancellationToken);
        await UploadContentAsync(uploadUrl, 1024, cancellationToken);

        // act
        var response = await AppHttpClient.PostAsJsonAsync($"/files/{fileId}/complete", new { }, cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        await AssertStatusAsync(fileId, MediaStatus.UPLOADED, cancellationToken);
    }

    [Fact]
    public async Task Complete_upload_without_uploading_content_marks_it_failed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var (fileId, _) = await StartUploadAsync(1024, "video.mp4", cancellationToken);

        // act
        var response = await AppHttpClient.PostAsJsonAsync($"/files/{fileId}/complete", new { }, cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        await AssertStatusAsync(fileId, MediaStatus.FAILED, cancellationToken);
    }

    [Fact]
    public async Task Complete_upload_with_content_size_mismatch_marks_it_failed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var (fileId, uploadUrl) = await StartUploadAsync(1024, "video.mp4", cancellationToken);

        await UploadContentAsync(uploadUrl, 512, cancellationToken);

        // act
        var response = await AppHttpClient.PostAsJsonAsync($"/files/{fileId}/complete", new { }, cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        await AssertStatusAsync(fileId, MediaStatus.FAILED, cancellationToken);
    }

    [Fact]
    public async Task Complete_upload_of_non_existing_file_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var fileId = Guid.NewGuid();

        // act
        var response = await AppHttpClient.PostAsJsonAsync($"/files/{fileId}/complete", new { }, cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
    }

    /// <summary>
    /// Initiates an upload via the upload endpoint and returns the asset id and presigned upload url.
    /// </summary>
    private async Task<(Guid FileId, string UploadUrl)> StartUploadAsync(
        long size,
        string fileName,
        CancellationToken cancellationToken)
    {
        await InitializeBucketAsync(VIDEOS_BUCKET, cancellationToken);

        var request = new StartUploadRequest(
            FileName: fileName,
            AssetType: "video",
            ContentType: CONTENT_TYPE,
            Size: size,
            Context: "location",
            ContextId: Guid.NewGuid());

        var response = await AppHttpClient.PostAsJsonAsync("/files/upload", request, cancellationToken);
        var result = await response.HandleResponseAsync<StartUploadResponse>(cancellationToken);

        Assert.True(result.IsSuccess);
        return (result.Value.MediaAssetId, result.Value.UploadUrl);
    }

    /// <summary>
    /// PUTs the given amount of bytes to the provided presigned upload url.
    /// </summary>
    private async Task UploadContentAsync(string uploadUrl, long size, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();
        using var content = new ByteArrayContent(new byte[size]);
        content.Headers.ContentType = new MediaTypeHeaderValue(CONTENT_TYPE);

        var response = await httpClient.PutAsync(uploadUrl, content, cancellationToken);
        Assert.True(response.IsSuccessStatusCode, $"{response.StatusCode} {response.ReasonPhrase}");
    }

    private async Task InitializeBucketAsync(string bucketName, CancellationToken cancellationToken)
    {
        await using var scope = Services.CreateAsyncScope();
        var storageProvider = scope.ServiceProvider.GetRequiredService<IFileStorageProvider>();

        var initResult = await storageProvider.InitializeBucketAsync(bucketName, cancellationToken);
        Assert.True(initResult.IsSuccess);
    }

    private async Task AssertStatusAsync(Guid fileId, MediaStatus expected, CancellationToken cancellationToken)
    {
        await ExecuteInDb(async dbContext =>
        {
            var asset = await dbContext.MediaAssets.FirstOrDefaultAsync(ma => ma.Id == fileId, cancellationToken);
            Assert.NotNull(asset);
            Assert.Equal(expected, asset!.Status);
        });
    }
}
