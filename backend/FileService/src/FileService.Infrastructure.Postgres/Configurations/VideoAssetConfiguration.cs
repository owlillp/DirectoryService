using System.Text.Json;
using FileService.Domain;
using FileService.Domain.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileService.Infrastructure.Postgres.Configurations;

public class VideoAssetConfiguration : IEntityTypeConfiguration<VideoAsset>
{
    public void Configure(EntityTypeBuilder<VideoAsset> builder)
    {
        builder.Property(x => x.Metadata)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<VideoMetadata>(v, (JsonSerializerOptions?)null))
            .HasColumnName("video_metadata")
            .HasColumnType("jsonb");

        builder.Property(x => x.SpritePreviewKey)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<StorageKey>(v, (JsonSerializerOptions?)null))
            .HasColumnName("sprite_preview_key")
            .HasColumnType("jsonb");

        builder.Property(x => x.PreviewKeys)
            .HasField("_previewKeys")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<StorageKey>>(v, (JsonSerializerOptions?)null) ?? new List<StorageKey>())
            .HasColumnName("preview_keys")
            .HasColumnType("jsonb");
    }
}