using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions");

        builder.HasKey(p => p.Id).HasName("pk_positions");

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();

        builder
            .Property(p => p.Name)
            .HasColumnName("name")
            .HasConversion(pn => pn.Value, s => PositionName.Create(s).Value)
            .HasMaxLength(LengthConstants.LENGTH_100)
            .IsRequired();

        builder.Navigation(p => p.Description).IsRequired(false);
        builder.OwnsOne(p => p.Description, db =>
        {
            db.Property(d => d.Value)
                .HasColumnName("description")
                .HasMaxLength(LengthConstants.LENGTH_100)
                .IsRequired();
        });

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}