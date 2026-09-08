using CSharpFunctionalExtensions;
using FileService.Domain.Assets;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Messaging.Publishers;

public interface IAssetCreatedEventPublisher
{
    Task<UnitResult<Error>> PublishAsync(MediaAsset asset);
}