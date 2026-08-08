using Core.Abstractions;
using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Domain;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Queries.CheckFileExist;

public class CheckFileExistHandler(IReadDbContext dbContext) : IQueryHandler<bool, CheckFileExistQuery>
{
    public async Task<Result<bool, Errors>> Handle(CheckFileExistQuery query, CancellationToken cancellationToken)
        => await dbContext.MediaAssetsRead.AnyAsync(ma => ma.Id == query.FileId && ma.Status == MediaStatus.UPLOADED, cancellationToken);
}