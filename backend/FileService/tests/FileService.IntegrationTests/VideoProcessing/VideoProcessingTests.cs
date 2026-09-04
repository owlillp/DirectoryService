using System.Net;
using System.Net.Http.Json;
using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Contracts.Files.Dtos;
using FileService.Contracts.Files.Requests;
using FileService.Contracts.Files.Responses;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.Domain.MediaProcessing;
using FileService.IntegrationTests.Infrastructure;
using FileService.VideoProcessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.SharedKernel.HttpCommunications;
using CompleteMultipartUploadRequest = FileService.Contracts.Files.Requests.CompleteMultipartUploadRequest;

namespace FileService.IntegrationTests.VideoProcessing;

public class VideoProcessingTests(IntegrationTestsWebFactory factory) : FileServiceTestsBase(factory)
{
    [Fact]
    public async Task ProcessVideoAsync_WhenValidVideoUploaded_ShouldSuccess()
    {
        // arrange
        using var cts = new CancellationTokenSource();
        CancellationToken cancellationToken = cts.Token;

        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        var videoProcessingService = scope.ServiceProvider.GetRequiredService<IVideoProcessingService>();

        Guid videoAssetId = await UploadTestVideoAsync(cancellationToken);

        // Act
        var result = await videoProcessingService.ProcessVideoAsync(videoAssetId, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        MediaAsset? mediaAsset = null;
        string? rawKey = null;

        await ExecuteInDb(async dbContext =>
        {
            mediaAsset = await dbContext.MediaAssets
                .AsNoTracking()
                .FirstOrDefaultAsync(ma => ma.Id == videoAssetId, cancellationToken);

            var videoProcess = await dbContext.VideoProcesses
                .AsNoTracking()
                .FirstOrDefaultAsync(vp => vp.VideoAssetId == videoAssetId, cancellationToken);

            Assert.NotNull(mediaAsset);
            Assert.Equal(MediaStatus.READY, mediaAsset.Status);

            Assert.NotNull(mediaAsset.Key);
            Assert.Equal($"hls/{videoAssetId}/{VideoAsset.MASTER_PLAYLIST_NAME}", mediaAsset.Key.Value);

            VideoAsset? videoAsset = mediaAsset as VideoAsset;
            Assert.NotNull(videoAsset);
            Assert.NotNull(videoAsset.RawKey);
            rawKey = videoAsset.RawKey.Value;

            Assert.NotNull(videoProcess);
            Assert.Equal(ProcessingStatus.COMPLETED, videoProcess.Status);
        });

        await ExecuteInS3(async s3Client =>
        {
            StorageKey key = mediaAsset?.Key ?? throw new InvalidOperationException("Media Asset Key is null");
            string prefix = key.Prefix;

            var listRequest = new ListObjectsV2Request { BucketName = VideoAsset.LOCATION, Prefix = prefix };

            var listResponse = await s3Client.ListObjectsV2Async(listRequest, cancellationToken);

            Assert.NotEmpty(listResponse.S3Objects);

            GetObjectMetadataResponse? objectData = await s3Client.GetObjectMetadataAsync(VideoAsset.LOCATION, key.Value, cancellationToken);

            Assert.NotNull(objectData);

            var exception = await Assert.ThrowsAsync<AmazonS3Exception>(
                async () => await s3Client.GetObjectMetadataAsync(VideoAsset.LOCATION, rawKey, cancellationToken));

            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        });
    }

    private async Task<Guid> UploadTestVideoAsync(CancellationToken cancellationToken)
    {
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TestFileName));
        StartMultipartUploadResponse startResponse = await StartMultipartUpload(fileInfo, cancellationToken);

        IReadOnlyCollection<PartETagDto> partETag = await UploadChunks(fileInfo, startResponse, cancellationToken);
        await CompleteMultipartUpload(startResponse, partETag, cancellationToken);
        return startResponse.MediaAssetId;
    }

    private async Task<StartMultipartUploadResponse> StartMultipartUpload(
        FileInfo fileInfo,
        CancellationToken cancellationToken)
    {
        await InitializeBucketAsync(VideoAsset.LOCATION, cancellationToken);
        await InitializeBucketAsync(PreviewAsset.LOCATION, cancellationToken);

        var request = new StartMultipartUploadRequest(
            fileInfo.Name,
            "video",
            "video/mp4",
            fileInfo.Length,
            "location",
            Guid.NewGuid());

        HttpResponseMessage startMultipartUploadResponse = await AppHttpClient.PostAsJsonAsync("/files/multipart/start", request, cancellationToken);
        var startMultipartUploadResult = await startMultipartUploadResponse.HandleResponseAsync<StartMultipartUploadResponse>(cancellationToken);

        startMultipartUploadResponse.EnsureSuccessStatusCode();

        await ExecuteInDb(async dbContext =>
        {
            await dbContext.MediaAssets
                .FirstOrDefaultAsync(m => m.Id == startMultipartUploadResult.Value.MediaAssetId, cancellationToken);
        });

        return startMultipartUploadResult.Value;
    }

    private async Task<IReadOnlyList<PartETagDto>> UploadChunks(
        FileInfo fileInfo,
        StartMultipartUploadResponse startMultipartUploadResponse,
        CancellationToken cancellationToken)
    {
        var parts = new List<PartETagDto>();

        using var httpClient = new HttpClient();
        await using Stream fileStream = fileInfo.OpenRead();
        foreach (ChunkUploadUrl chunkUploadUrl in startMultipartUploadResponse.ChunkUploadUrls.OrderBy(c => c.PartNumber))
        {
            byte[] chunk = new byte [startMultipartUploadResponse.ChunkSize];
            int bytesRead = await fileStream.ReadAsync(
                chunk.AsMemory(
                0,
                startMultipartUploadResponse.ChunkSize),
                cancellationToken);
            if (bytesRead == 0)
                break;

            var content = new ByteArrayContent(chunk, 0, bytesRead);

            var response = await httpClient.PutAsync(chunkUploadUrl.UploadUrl, content, cancellationToken);

            response.EnsureSuccessStatusCode();

            string? etag = response.Headers.ETag?.ToString().Trim('"');
            parts.Add(new PartETagDto(chunkUploadUrl.PartNumber, etag!));
        }

        return parts;
    }

    private async Task CompleteMultipartUpload(
        StartMultipartUploadResponse startMultipartUploadResponse,
        IEnumerable<PartETagDto> partETags,
        CancellationToken cancellationToken)
    {
        var completeRequest = new CompleteMultipartUploadRequest(
            startMultipartUploadResponse.MediaAssetId,
            startMultipartUploadResponse.UploadId,
            partETags.ToList());

        var completeResponse = await AppHttpClient.PostAsJsonAsync("/files/multipart/complete", completeRequest, cancellationToken);
        await completeResponse.HandleResponseAsync(cancellationToken);
    }

    private async Task InitializeBucketAsync(string bucketName, CancellationToken cancellationToken)
    {
        await using var scope = Services.CreateAsyncScope();
        var storageProvider = scope.ServiceProvider.GetRequiredService<IFileStorageProvider>();

        var initResult = await storageProvider.InitializeBucketAsync(bucketName, cancellationToken);
        Assert.True(initResult.IsSuccess);
    }
}