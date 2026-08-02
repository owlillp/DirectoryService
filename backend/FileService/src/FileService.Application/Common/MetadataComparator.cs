using CSharpFunctionalExtensions;
using FileService.Domain;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Common;

public static class MetadataComparator
{
    public static UnitResult<Errors> Compare(MediaData mediaData, StorageObjectMetadata storedMetadata)
    {
        var errors = new List<Error>();

        if (mediaData.Size != storedMetadata.ContentLength)
        {
            errors.Add(GeneralErrors.ValueIsInvalid(
                $"size mismatch: expected {mediaData.Size}, actual {storedMetadata.ContentLength}"));
        }

        if (!string.Equals(
                mediaData.ContentType.Value,
                storedMetadata.ContentType,
                StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(GeneralErrors.ValueIsInvalid(
                $"content type mismatch: expected {mediaData.ContentType.Value}, actual {storedMetadata.ContentType}"));
        }

        return errors.Any()
            ? new Errors(errors)
            : UnitResult.Success<Errors>();
    }
}
