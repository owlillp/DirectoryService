using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Application.Models;
using Microsoft.Extensions.Options;
using Shared.SharedKernel.Failures;

namespace FileService.Infrastructure.S3;

public class ChunkSizeCalculator(IOptions<FileStorageOptions> options) : IChunkSizeCalculator
{
    private const long MIN_CHUNK_SIZE = 5 * 1024 * 1024;

    private readonly FileStorageOptions _fileStorageOptions = options.Value;

    public Result<(int ChunkSize, int TotalChunks), Error> Calculate(long fileSize)
    {
        if(_fileStorageOptions.RecommendedChunkSizeBytes <= 0 || _fileStorageOptions.MaxChunks <= 0 )
            return GeneralErrors.ValueIsInvalid("invalid chunk size or max chunks");

        if (fileSize <= _fileStorageOptions.RecommendedChunkSizeBytes)
        {
            return ((int)fileSize, 1);
        }

        int calculatedChunks = (int)Math.Ceiling((double)fileSize / _fileStorageOptions.RecommendedChunkSizeBytes);
        int actualChunks = Math.Min(calculatedChunks, _fileStorageOptions.MaxChunks);
        long chunkSize = (fileSize + actualChunks - 1) / actualChunks;

        if (chunkSize < MIN_CHUNK_SIZE && actualChunks > 1)
        {
            actualChunks = (int)Math.Min(_fileStorageOptions.MaxChunks, fileSize / MIN_CHUNK_SIZE);
            chunkSize = (int)((fileSize + actualChunks - 1) / actualChunks);
        }

        return ((int)chunkSize, actualChunks);
    }
}