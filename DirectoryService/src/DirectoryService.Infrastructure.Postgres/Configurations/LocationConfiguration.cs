using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Constants;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");

        builder.HasKey(l => l.Id).HasName("pk_locations");

        builder.HasIndex(l => l.Name)
            .IsUnique()
            .HasDatabaseName("ix_locations_name");

        builder.Property(l => l.Id)
            .HasColumnName("id")
            .HasConversion(li => li.Value, guid => new LocationId(guid))
            .IsRequired();

        builder.Property(l => l.Name)
            .HasColumnName("name")
            .HasConversion(ln => ln.Value, s => LocationName.Create(s).Value)
            .HasMaxLength(LengthConstants.LENGTH_120)
            .IsRequired();

        builder.OwnsOne(l => l.Address, ab =>
        {
            ab.Property(a => a.Country)
                .HasColumnName("country")
                .HasMaxLength(LengthConstants.LENGTH_500)
                .IsRequired();

            ab.Property(a => a.City)
                .HasColumnName("city")
                .HasMaxLength(LengthConstants.LENGTH_500)
                .IsRequired();

            ab.Property(a => a.Street)
                .HasColumnName("street")
                .HasMaxLength(LengthConstants.LENGTH_500)
                .IsRequired();

            ab.Property(a => a.PostalCode)
                .HasColumnName("postal_code")
                .IsRequired();

            ab.Property(a => a.BuildingNumber)
                .HasColumnName("building_number")
                .IsRequired();

            ab.Property(a => a.Apartment)
                .HasColumnName("apartment")
                .HasMaxLength(LengthConstants.LENGTH_500)
                .IsRequired(false);

            ab.HasIndex(a => new
                {
                    a.Country,
                    a.City,
                    a.Street,
                    a.BuildingNumber,
                    a.PostalCode,
                })
                .IsUnique()
                .HasDatabaseName("ix_locations_address");
        });

        builder.Navigation(l => l.Address);

        builder.Property(l => l.Timezone)
            .HasColumnName("timezone")
            .HasConversion(tz => tz.Value, s => LocationTimezone.Create(s).Value)
            .HasMaxLength(LengthConstants.LENGTH_500)
            .IsRequired();

        builder.Property(l => l.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}