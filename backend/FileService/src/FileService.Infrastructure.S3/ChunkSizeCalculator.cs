using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using Microsoft.Extensions.Options;
using Shared.SharedKernel.Failures;

namespace FileService.Infrastructure.S3;

public class ChunkSizeCalculator(IOptions<S3Options> options) : IChunkSizeCalculator
{
    private readonly S3Options _s3Options = options.Value;

    public Result<(int ChunkSize, int TotalChunks), Error> Calculate(long fileSize)
    {
        if(_s3Options.RecommendedChunkSizeBytes <= 0 || _s3Options.MaxChunks <= 0 )
            return GeneralErrors.ValueIsInvalid("invalid chunk size or max chunks");

        if (fileSize <= _s3Options.RecommendedChunkSizeBytes)
        {
            return ((int)fileSize, 1);
        }

        int calculatedChunks = (int)Math.Ceiling((double)fileSize / _s3Options.RecommendedChunkSizeBytes);

        int actualChunks = Math.Min(calculatedChunks, _s3Options.MaxChunks);

        long chunkSize = (fileSize + actualChunks - 1) / actualChunks;

        return ((int)chunkSize, actualChunks);
    }
}