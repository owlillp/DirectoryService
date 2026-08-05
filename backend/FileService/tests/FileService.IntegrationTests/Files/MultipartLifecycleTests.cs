using System.Net.Http.Headers;
using System.Net.Http.Json;
using FileService.Application.Abstractions;
using FileService.Contracts;
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

/// <summary>
/// Integration tests covering the multipart upload lifecycle:
/// start, uploading all parts, completing, aborting and state transitions.
/// </summary>
public class MultipartLifecycleTests(IntegrationTestsWebFactory factory) : FileServiceTestsBase(factory)
{
    // The chunk size is configured in the test web factory. S3/MinIO require
    // every part (except the last) to be at least 5MB, so a 15MB file splits
    // into three 5MB parts.
    private const int CHUNK_SIZE = 5 * 1024 * 1024;
    private const int PART_COUNT = 3;
    private const long FILE_SIZE = CHUNK_SIZE * PART_COUNT;
    private const string VIDEOS_BUCKET = "videos";
    private const string CONTENT_TYPE = "video/mp4";

    [Fact]
    public async Task Successful_multipart_upload_of_all_parts_marks_asset_uploaded()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        await InitializeBucketAsync(VIDEOS_BUCKET, cancellationToken);

        var (fileId, uploadId, chunkUrls, chunkSize) = await StartMultipartAsync(cancellationToken);

        // the file is split into three equal parts
        Assert.Equal(PART_COUNT, chunkUrls.Count);
        Assert.Equal(CHUNK_SIZE, chunkSize);

        var partETags = await UploadAllPartsAsync(chunkUrls, cancellationToken);

        // act
        var completeResponse = await AppHttpClient.PostAsJsonAsync(
            "/files/multipart/complete",
            new CompleteMultipartUploadRequest(fileId, uploadId, partETags),
            cancellationToken);
        var completeResult = await completeResponse.HandleResponseAsync<Guid>(cancellationToken);

        // assert
        Assert.True(completeResult.IsSuccess);
        Assert.Equal(fileId, completeResult.Value);

        await AssertStatusAsync(fileId, MediaStatus.UPLOADED, cancellationToken);
        await AssertAssetSizeAsync(fileId, FILE_SIZE, cancellationToken);

        // the assembled file should be downloadable and match the uploaded content
        var downloadUrl = await GetDownloadUrlAsync(fileId, cancellationToken);
        var downloaded = await DownloadContentAsync(downloadUrl, cancellationToken);
        Assert.Equal(BuildExpectedContent(), downloaded);
    }

    [Fact]
    public async Task Complete_with_wrong_part_etags_fails_and_marks_asset_failed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        await InitializeBucketAsync(VIDEOS_BUCKET, cancellationToken);

        var (fileId, uploadId, chunkUrls, _) = await StartMultipartAsync(cancellationToken);
        await UploadAllPartsAsync(chunkUrls, cancellationToken);

        // provide ETags that do not match any uploaded part
        var bogusEtags = Enumerable.Range(1, PART_COUNT)
            .Select(part => new PartETagDto(part, $"\"bogus-etag-{part}\""))
            .ToList();

        // act
        var completeResponse = await AppHttpClient.PostAsJsonAsync(
            "/files/multipart/complete",
            new CompleteMultipartUploadRequest(fileId, uploadId, bogusEtags),
            cancellationToken);
        var completeResult = await completeResponse.HandleResponseAsync<Guid>(cancellationToken);

        // assert
        Assert.True(completeResult.IsFailure);

        // the asset is now in a failed state, not uploaded
        await AssertStatusAsync(fileId, MediaStatus.FAILED, cancellationToken);
    }

    [Fact]
    public async Task Complete_with_wrong_number_of_parts_fails_validation_and_keeps_asset_uploading()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        await InitializeBucketAsync(VIDEOS_BUCKET, cancellationToken);

        var (fileId, uploadId, chunkUrls, _) = await StartMultipartAsync(cancellationToken);
        var partETags = await UploadAllPartsAsync(chunkUrls, cancellationToken);

        // send only the first part - an incomplete set of parts
        var incompleteEtags = partETags.Take(partETags.Count - 1).ToList();

        // act
        var completeResponse = await AppHttpClient.PostAsJsonAsync(
            "/files/multipart/complete",
            new CompleteMultipartUploadRequest(fileId, uploadId, incompleteEtags),
            cancellationToken);
        var completeResult = await completeResponse.HandleResponseAsync<Guid>(cancellationToken);

        // assert
        Assert.True(completeResult.IsFailure);
        Assert.NotNull(completeResult.Error);
        Assert.Contains(completeResult.Error, e => e.Type == ErrorType.VALIDATION);

        // validation failed before touching the storage, so the asset remains uploading
        await AssertStatusAsync(fileId, MediaStatus.UPLOADING, cancellationToken);
    }

    [Fact]
    public async Task Repeated_complete_after_successful_upload_fails()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        await InitializeBucketAsync(VIDEOS_BUCKET, cancellationToken);

        var (fileId, uploadId, chunkUrls, _) = await StartMultipartAsync(cancellationToken);
        var partETags = await UploadAllPartsAsync(chunkUrls, cancellationToken);

        var firstCompleteResponse = await AppHttpClient.PostAsJsonAsync(
            "/files/multipart/complete",
            new CompleteMultipartUploadRequest(fileId, uploadId, partETags),
            cancellationToken);
        var firstCompleteResult = await firstCompleteResponse.HandleResponseAsync<Guid>(cancellationToken);
        Assert.True(firstCompleteResult.IsSuccess);
        await AssertStatusAsync(fileId, MediaStatus.UPLOADED, cancellationToken);

        // act - try to complete the same upload again
        var secondCompleteResponse = await AppHttpClient.PostAsJsonAsync(
            "/files/multipart/complete",
            new CompleteMultipartUploadRequest(fileId, uploadId, partETags),
            cancellationToken);
        var secondCompleteResult = await secondCompleteResponse.HandleResponseAsync<Guid>(cancellationToken);

        // assert - the multipart upload no longer exists upstream, so it fails
        Assert.True(secondCompleteResult.IsFailure);
        await AssertStatusAsync(fileId, MediaStatus.FAILED, cancellationToken);
    }

    [Fact]
    public async Task Abort_after_partial_upload_marks_asset_deleted()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        await InitializeBucketAsync(VIDEOS_BUCKET, cancellationToken);

        var (fileId, uploadId, chunkUrls, _) = await StartMultipartAsync(cancellationToken);

        // upload only a subset of the parts (partial upload)
        await UploadSinglePartAsync(chunkUrls[0], cancellationToken);

        // act
        var abortResponse = await AppHttpClient.PostAsJsonAsync(
            "/files/multipart/abort",
            new AbortMultipartUploadRequest(fileId, uploadId),
            cancellationToken);
        var abortResult = await abortResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(abortResult.IsSuccess);
        await AssertStatusAsync(fileId, MediaStatus.DELETED, cancellationToken);
    }

    private async Task<(Guid FileId, string UploadId, IReadOnlyList<ChunkUploadUrl> ChunkUrls, int ChunkSize)>
        StartMultipartAsync(CancellationToken cancellationToken)
    {
        var request = new StartMultipartUploadRequest(
            FileName: "movie.mp4",
            AssetType: "video",
            ContentType: CONTENT_TYPE,
            Size: FILE_SIZE,
            Context: "location",
            ContextId: Guid.NewGuid());

        var response = await AppHttpClient.PostAsJsonAsync("/files/multipart/start", request, cancellationToken);
        var result = await response.HandleResponseAsync<StartMultipartUploadResponse>(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        return (result.Value.MediaAssetId, result.Value.UploadId, result.Value.ChunkUploadUrls, result.Value.ChunkSize);
    }

    /// <summary>
    /// Uploads every part via its presigned url and returns the resulting ETags.
    /// </summary>
    private static async Task<IReadOnlyList<PartETagDto>> UploadAllPartsAsync(
        IReadOnlyList<ChunkUploadUrl> chunkUrls,
        CancellationToken cancellationToken)
    {
        var etags = new List<PartETagDto>();

        foreach (var chunk in chunkUrls)
        {
            string etag = await UploadSinglePartAsync(chunk, cancellationToken);
            etags.Add(new PartETagDto(chunk.PartNumber, etag));
        }

        return etags;
    }

    /// <summary>
    /// Uploads a single part via its presigned PUT url and returns the S3 ETag
    /// taken from the response headers.
    /// </summary>
    private static async Task<string> UploadSinglePartAsync(ChunkUploadUrl chunk, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();
        using var content = new ByteArrayContent(BuildPartContent(chunk.PartNumber, CHUNK_SIZE));
        content.Headers.ContentType = new MediaTypeHeaderValue(CONTENT_TYPE);

        var response = await httpClient.PutAsync(chunk.UploadUrl, content, cancellationToken);
        Assert.True(response.IsSuccessStatusCode, $"{response.StatusCode} {response.ReasonPhrase}");

        Assert.True(
            response.Headers.TryGetValues("ETag", out var etagValues),
            "S3 should return an ETag header after uploading a part.");
        return etagValues.First();
    }

    /// <summary>
    /// Builds a deterministic chunk body so the assembled file content is verifyable.
    /// </summary>
    private static byte[] BuildPartContent(int partNumber, int size)
    {
        var buffer = new byte[size];
        Array.Fill(buffer, (byte)partNumber);
        return buffer;
    }

    private static byte[] BuildExpectedContent()
    {
        // three 5MB parts, each filled with its part number (1, 2, 3)
        var expected = new byte[FILE_SIZE];
        for (int part = 1; part <= PART_COUNT; part++)
        {
            byte[] partContent = BuildPartContent(part, CHUNK_SIZE);
            Buffer.BlockCopy(partContent, 0, expected, (part - 1) * CHUNK_SIZE, CHUNK_SIZE);
        }

        return expected;
    }

    private async Task<string> GetDownloadUrlAsync(Guid fileId, CancellationToken cancellationToken)
    {
        var getFileResponse = await AppHttpClient.GetAsync($"/files/{fileId}", cancellationToken);
        var getFileResult = await getFileResponse.HandleResponseAsync<GetMediaAssetDto>(cancellationToken);

        Assert.True(getFileResult.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(getFileResult.Value!.Url));
        return getFileResult.Value.Url;
    }

    private static async Task<byte[]> DownloadContentAsync(string downloadUrl, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();

        var downloadResponse = await httpClient.GetAsync(downloadUrl, cancellationToken);
        Assert.True(downloadResponse.IsSuccessStatusCode);

        return await downloadResponse.Content.ReadAsByteArrayAsync(cancellationToken);
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
            Assert.Equal(expected, asset!.Status);
        });
    }

    private async Task AssertAssetSizeAsync(Guid fileId, long expectedSize, CancellationToken cancellationToken)
    {
        await ExecuteInDb(async dbContext =>
        {
            var asset = await dbContext.MediaAssets.FirstOrDefaultAsync(ma => ma.Id == fileId, cancellationToken);
            Assert.Equal(expectedSize, asset!.MediaData.Size);
        });
    }
}
