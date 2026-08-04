using FileService.Application.Abstractions;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.SharedKernel.HttpCommunications;

namespace FileService.IntegrationTests.Files;

/// <summary>
/// Integration tests for the delete endpoint (DELETE /files/{fileId}).
/// </summary>
public class DeleteAssetTests(IntegrationTestsWebFactory factory) : FileServiceTestsBase(factory)
{
    private const string VIDEOS_BUCKET = "videos";

    [Fact]
    public async Task Delete_of_uploaded_asset_marks_it_deleted()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        Guid fileId = await SeedAssetAsync(MediaStatus.UPLOADED, cancellationToken);
        await InitializeBucketAsync(VIDEOS_BUCKET, cancellationToken);

        // act
        var response = await AppHttpClient.DeleteAsync($"/files/{fileId}", cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);

        await AssertStatusAsync(fileId, MediaStatus.DELETED, cancellationToken);
    }

    [Fact]
    public async Task Delete_of_uploading_asset_marks_it_deleted()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        Guid fileId = await SeedAssetAsync(MediaStatus.UPLOADING, cancellationToken);
        await InitializeBucketAsync(VIDEOS_BUCKET, cancellationToken);

        // act
        var response = await AppHttpClient.DeleteAsync($"/files/{fileId}", cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);

        await AssertStatusAsync(fileId, MediaStatus.DELETED, cancellationToken);
    }

    [Fact]
    public async Task Delete_of_non_existing_file_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var fileId = Guid.NewGuid();

        // act
        var response = await AppHttpClient.DeleteAsync($"/files/{fileId}", cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
    }

    /// <summary>
    /// Seeds a VideoAsset directly into the DB with the given status.
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

            var owner = MediaOwner.ForLocation(Guid.NewGuid()).Value;
            var assetResult = VideoAsset.CreateForUpload(fileId, mediaData, owner);
            Assert.True(assetResult.IsSuccess);

            var asset = assetResult.Value;
            if (status == MediaStatus.UPLOADED)
            {
                asset.MarkUploaded();
            }

            dbContext.MediaAssets.Add(asset);
            await dbContext.SaveChangesAsync(cancellationToken);
        });

        return fileId;
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
