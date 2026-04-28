using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public class DepartmentLocationConfiguration : IEntityTypeConfiguration<DepartmentLocation>
{
    public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
    {
        builder.ToTable("department_locations");

        builder.HasKey(dl => dl.Id).HasName("pk_department_locations");

        builder.HasIndex(dl => new
            {
                dl.DepartmentId,
                dl.LocationId,
            }).HasDatabaseName("ix_department_locations_department_id_location_id")
            .IsUnique();

        builder.Property(dl => dl.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, guid => new DepartmentLocationId(guid))
            .IsRequired();

        builder.Property(dl => dl.DepartmentId)
            .HasColumnName("department_id")
            .HasConversion(di => di.Value, guid => new DepartmentId(guid))
            .IsRequired();

        builder.Property(dl => dl.LocationId)
            .HasColumnName("location_id")
            .HasConversion(li => li.Value, guid => new LocationId(guid))
            .IsRequired();

        builder
            .HasOne<Department>()
            .WithMany(d => d.Locations)
            .HasForeignKey(dl => dl.DepartmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<Location>()
            .WithMany(l => l.Departments)
            .HasForeignKey(dl => dl.LocationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}