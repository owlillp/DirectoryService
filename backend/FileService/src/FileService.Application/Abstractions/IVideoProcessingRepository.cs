using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using FileService.Domain.MediaProcessing;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Abstractions;

public interface IVideoProcessingRepository
{
    Task<Result<VideoProcess, Error>> GetByAsync(Expression<Func<VideoProcess, bool>> expression, CancellationToken cancellationToken = default);

    Task<Result<Guid, Error>> AddAsync(VideoProcess videoProcess, CancellationToken cancellationToken = default);
}