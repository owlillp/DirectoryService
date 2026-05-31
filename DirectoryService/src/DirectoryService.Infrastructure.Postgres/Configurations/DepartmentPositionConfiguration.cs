using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_positions");

        builder.HasKey(dp => dp.Id).HasName("pk_department_positions");

        builder.HasIndex(dp => new
            {
                dp.DepartmentId,
                dp.PositionId,
            }).HasDatabaseName("ix_department_positions_department_id_position_id")
            .IsUnique();

        builder
            .HasIndex(dp => new
            {
                dp.PositionId,
                dp.DepartmentId,
            })
            .HasDatabaseName("ix_department_positions_position_id_department_id")
            .IsUnique();

        builder.Property(dp => dp.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, guid => new DepartmentPositionId(guid))
            .IsRequired();

        builder.Property(dp => dp.DepartmentId)
            .HasColumnName("department_id")
            .HasConversion(di => di.Value, guid => new DepartmentId(guid))
            .IsRequired();

        builder.Property(dp => dp.PositionId)
            .HasColumnName("position_id")
            .HasConversion(pi => pi.Value, guid => new PositionId(guid))
            .IsRequired();

        builder
            .HasOne<Department>()
            .WithMany(d => d.Positions)
            .HasForeignKey(dp => dp.DepartmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<Position>()
            .WithMany(p => p.Departments)
            .HasForeignKey(dp => dp.PositionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}