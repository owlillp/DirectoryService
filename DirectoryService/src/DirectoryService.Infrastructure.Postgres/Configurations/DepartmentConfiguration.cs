using DirectoryService.Domain.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Constants;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

        builder.HasKey(d => d.Id).HasName("pk_departments");

        builder.Property(d => d.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(d => d.Name)
            .HasColumnName("name")
            .HasConversion(dn => dn.Value, s => DepartmentName.Create(s).Value)
            .HasMaxLength(LengthConstants.LENGTH_150)
            .IsRequired();

        builder.Property(d => d.Identifier)
            .HasColumnName("identifier")
            .HasConversion(di => di.Value, s => DepartmentIdentifier.Create(s).Value)
            .HasMaxLength(LengthConstants.LENGTH_150)
            .IsRequired();

        builder.Property(d => d.ParentId)
            .HasColumnName("parent_id")
            .HasMaxLength(LengthConstants.LENGTH_500)
            .IsRequired(false);

        builder.Property(d => d.Path)
            .HasColumnName("path")
            .HasConversion(dp => dp.Value, s => DepartmentPath.Create(s).Value)
            .HasMaxLength(LengthConstants.LENGTH_500)
            .IsRequired();

        builder.Property(d => d.Depth)
            .HasColumnName("depth")
            .IsRequired();

        builder.Property(d => d.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}