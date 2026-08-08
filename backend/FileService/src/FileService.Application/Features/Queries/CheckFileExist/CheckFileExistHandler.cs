using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Queries.CheckFileExist;

public class CheckFileExistHandler(
    IValidator<CheckFileExistQuery> validator,
    IReadDbContext dbContext) : IQueryHandler<bool, CheckFileExistQuery>
{
    public async Task<Result<bool, Errors>> Handle(CheckFileExistQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        AssetType? assetType = string.IsNullOrWhiteSpace(query.AssetType)
            ? null
            : query.AssetType.ToAssetType();

        return await dbContext.MediaAssetsRead.AnyAsync(
            ma => ma.Id == query.FileId && ma.Status == MediaStatus.UPLOADED && (!assetType.HasValue || ma.AssetType == assetType.Value),
            cancellationToken);
    }
}