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

        builder.Property(l => l.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(l => l.Name)
            .HasColumnName("name")
            .HasConversion(ln => ln.Value, s => LocationName.Create(s).Value)
            .HasMaxLength(LengthConstants.LENGTH_120)
            .IsRequired();

        builder.OwnsOne(l => l.Address, ab =>
        {
            ab.ToJson("address");

            ab.Property(a => a.Country)
                .HasJsonPropertyName("country")
                .HasMaxLength(LengthConstants.LENGTH_500)
                .IsRequired();

            ab.Property(a => a.City)
                .HasJsonPropertyName("city")
                .HasMaxLength(LengthConstants.LENGTH_500)
                .IsRequired();

            ab.Property(a => a.Street)
                .HasJsonPropertyName("street")
                .HasMaxLength(LengthConstants.LENGTH_500)
                .IsRequired();

            ab.Property(a => a.PostalCode)
                .HasJsonPropertyName("postal_code")
                .IsRequired();

            ab.Property(a => a.BuildingNumber)
                .HasJsonPropertyName("building_number")
                .IsRequired();

            ab.Property(a => a.Apartment)
                .HasJsonPropertyName("apartment")
                .HasMaxLength(LengthConstants.LENGTH_500)
                .IsRequired(false);
        });

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