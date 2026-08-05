using CSharpFunctionalExtensions;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Abstractions;

public interface IChunkSizeCalculator
{
    Result<(int ChunkSize, int TotalChunks), Error> Calculate(long fileSize);
}