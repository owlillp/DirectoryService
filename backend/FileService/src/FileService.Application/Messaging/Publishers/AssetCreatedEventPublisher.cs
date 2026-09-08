using CSharpFunctionalExtensions;
using FileService.Domain;
using FileService.Domain.Assets;
using Messaging.IntegrationEvents.Files.Events;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Messaging.Publishers;

public class AssetCreatedEventPublisher : IAssetCreatedEventPublisher
{
    private readonly Dictionary<AssetType, Func<MediaAsset, Task>> _publishers;

    public AssetCreatedEventPublisher(IOutboxService outboxService)
    {
        _publishers = new Dictionary<AssetType, Func<MediaAsset, Task>>
        {
            [AssetType.VIDEO] = asset => outboxService.PublishAsync(
                new VideoCreatedEvent(asset.Id, asset.Owner.EntityId, asset.Owner.Context)),
            [AssetType.PREVIEW] = asset => outboxService.PublishAsync(
                new PreviewCreatedEvent(asset.Id, asset.Owner.EntityId, asset.Owner.Context)),
        };
    }

    public async Task<UnitResult<Error>> PublishAsync(MediaAsset asset)
    {
        if (!_publishers.TryGetValue(asset.AssetType,  out var publisher))
        {
            return Error.Validation(
                "asset.unsupported.type",
                $"No integration event mapping for asset type '{asset.AssetType}'");
        }

        await publisher(asset);

        return UnitResult.Success<Error>();
    }
}