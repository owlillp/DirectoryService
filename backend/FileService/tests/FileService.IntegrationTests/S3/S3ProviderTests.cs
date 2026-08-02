using System.Net.Http.Headers;
using FileService.Application.Abstractions;
using FileService.Domain;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.IntegrationTests.S3;

public class S3ProviderTests(IntegrationTestsWebFactory factory) : FileServiceTestsBase(factory)
{
    private const string BUCKET_NAME = "videos";

    [Fact]
    public async Task S3Provider_should_upload_read_metadata_download_and_delete_object()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var storageKeyResult = StorageKey.Create(BUCKET_NAME, null, Guid.NewGuid().ToString());
        Assert.True(storageKeyResult.IsSuccess);
        var storageKey = storageKeyResult.Value;

        var fileNameResult = FileName.Create("video.mp4");
        Assert.True(fileNameResult.IsSuccess);
        var contentTypeResult = ContentType.Create("video/mp4");
        Assert.True(contentTypeResult.IsSuccess);
        var mediaDataResult = MediaData.Create(fileNameResult.Value, contentTypeResult.Value, 4, 1);
        Assert.True(mediaDataResult.IsSuccess);

        byte[] content = "test"u8.ToArray();

        await using var scope = Services.CreateAsyncScope();
        var s3Provider = scope.ServiceProvider.GetRequiredService<IFileStorageProvider>();

        using var httpClient = new HttpClient();

        // initialize bucket
        var initResult = await s3Provider.InitializeBucketAsync(BUCKET_NAME, cancellationToken);
        Assert.True(initResult.IsSuccess);

        // generate upload url
        var uploadUrlResult = await s3Provider.GenerateUploadUrlAsync(storageKey, mediaDataResult.Value, cancellationToken);
        Assert.True(uploadUrlResult.IsSuccess);
        string uploadUrl = uploadUrlResult.Value;

        // upload a small file
        using var uploadContent = new ByteArrayContent(content);
        uploadContent.Headers.ContentType = new MediaTypeHeaderValue(contentTypeResult.Value.Value);
        var uploadResponse = await httpClient.PutAsync(uploadUrl, uploadContent, cancellationToken);
        Assert.True(uploadResponse.IsSuccessStatusCode, $"{uploadResponse.StatusCode} {uploadResponse.ReasonPhrase}");

        // read metadata
        var metadataResult = await s3Provider.GetAssetMetadataAsync(storageKey, cancellationToken);
        Assert.True(metadataResult.IsSuccess);

        // generate download url and download the file
        var downloadUrlResult = await s3Provider.GenerateDownloadUrlAsync(storageKey);
        Assert.True(downloadUrlResult.IsSuccess);
        Assert.Equal(System.Net.HttpStatusCode.OK, uploadResponse.StatusCode);
        string downloadUrl = downloadUrlResult.Value;

        var downloadResponse = await httpClient.GetAsync(downloadUrl, cancellationToken);
        Assert.True(downloadResponse.IsSuccessStatusCode);
        byte[] downloaded = await downloadResponse.Content.ReadAsByteArrayAsync(cancellationToken);
        Assert.Equal(content, downloaded);

        // delete object
        var deleteResult = await s3Provider.DeleteAssetAsync(storageKey, cancellationToken);
        Assert.True(deleteResult.IsSuccess);
    }
}
