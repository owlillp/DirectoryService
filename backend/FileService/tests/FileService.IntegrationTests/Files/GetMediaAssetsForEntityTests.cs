using FileService.Contracts.Files.Responses;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.IntegrationTests.Infrastructure;
using Shared.SharedKernel.Failures;
using Shared.SharedKernel.HttpCommunications;

namespace FileService.IntegrationTests.Files;

/// <summary>
/// Integration tests for the GET /files/entity endpoint
/// that returns all media assets belonging to a given entity.
/// </summary>
public class GetMediaAssetsForEntityTests(IntegrationTestsWebFactory factory) : FileServiceTestsBase(factory)
{
    private const string Context = "location";

    [Fact]
    public async Task GetMediaAssetsForEntity_of_entity_returns_all_its_assets()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var entityId = Guid.NewGuid();

        Guid uploadedId = await SeedAssetAsync(entityId, MediaStatus.UPLOADED, cancellationToken);
        Guid uploadingId = await SeedAssetAsync(entityId, MediaStatus.UPLOADING, cancellationToken);

        // act
        var response = await AppHttpClient.GetAsync($"/files/entity?Context={Context}&EntityId={entityId}", cancellationToken);
        var result = await response.HandleResponseAsync<GetFilesForEntityResponse>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(Context, result.Value.Context);
        Assert.Equal(entityId, result.Value.Id);
        Assert.Equal(2, result.Value.FileInfos.Length);

        var dtoIds = result.Value.FileInfos.Select(fi => fi.Id).ToHashSet();
        Assert.Contains(uploadedId, dtoIds);
        Assert.Contains(uploadingId, dtoIds);

        var uploadedDto = result.Value.FileInfos.First(fi => fi.Id == uploadedId);
        Assert.Equal(MediaStatus.UPLOADED.ToString().ToLowerInvariant(), uploadedDto.Status);
        Assert.False(string.IsNullOrWhiteSpace(uploadedDto.Url));

        var uploadingDto = result.Value.FileInfos.First(fi => fi.Id == uploadingId);
        Assert.Equal(MediaStatus.UPLOADING.ToString().ToLowerInvariant(), uploadingDto.Status);
        Assert.Null(uploadingDto.Url);
    }

    [Fact]
    public async Task GetMediaAssetsForEntity_of_unknown_entity_returns_empty_list()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var entityId = Guid.NewGuid();

        // act
        var response = await AppHttpClient.GetAsync($"/files/entity?Context={Context}&EntityId={entityId}", cancellationToken);
        var result = await response.HandleResponseAsync<GetFilesForEntityResponse>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(entityId, result.Value.Id);
        Assert.Empty(result.Value.FileInfos);
    }

    [Fact]
    public async Task GetMediaAssetsForEntity_filters_assets_by_entity()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var firstEntityId = Guid.NewGuid();
        var secondEntityId = Guid.NewGuid();

        Guid firstId = await SeedAssetAsync(firstEntityId, MediaStatus.UPLOADED, cancellationToken);
        Guid secondId = await SeedAssetAsync(secondEntityId, MediaStatus.UPLOADED, cancellationToken);

        // act
        var response = await AppHttpClient.GetAsync($"/files/entity?Context={Context}&EntityId={firstEntityId}", cancellationToken);
        var result = await response.HandleResponseAsync<GetFilesForEntityResponse>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        var dtoIds = result.Value.FileInfos.Select(fi => fi.Id).ToHashSet();
        Assert.Contains(firstId, dtoIds);
        Assert.DoesNotContain(secondId, dtoIds);
    }

    [Fact]
    public async Task GetMediaAssetsForEntity_excludes_deleted_assets()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var entityId = Guid.NewGuid();

        Guid liveId = await SeedAssetAsync(entityId, MediaStatus.UPLOADING, cancellationToken);
        await SeedAssetAsync(entityId, MediaStatus.DELETED, cancellationToken);

        // act
        var response = await AppHttpClient.GetAsync($"/files/entity?Context={Context}&EntityId={entityId}", cancellationToken);
        var result = await response.HandleResponseAsync<GetFilesForEntityResponse>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.FileInfos);
        Assert.Equal(liveId, result.Value.FileInfos[0].Id);
    }

    [Fact]
    public async Task GetMediaAssetsForEntity_with_missing_entity_id_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        // act
        var response = await AppHttpClient.GetAsync($"/files/entity?Context={Context}", cancellationToken);
        var result = await response.HandleResponseAsync<GetFilesForEntityResponse>(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    /// <summary>
    /// Seeds a VideoAsset directly into the DB with the given status and owner entity id.
    /// </summary>
    private async Task<Guid> SeedAssetAsync(Guid entityId, MediaStatus status, CancellationToken cancellationToken)
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

            var owner = MediaOwner.ForLocation(entityId).Value;
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
                    // asset is created in UPLOADING state and stays as-is
                    break;
            }

            dbContext.MediaAssets.Add(asset);
            await dbContext.SaveChangesAsync(cancellationToken);
        });

        return fileId;
    }
}
