using Core.Constants;
using DirectoryService.Domain.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

        builder.HasKey(d => d.Id).HasName("pk_departments");

        builder.HasIndex(d => d.Identifier)
            .IsUnique()
            .HasDatabaseName("idx_departments_identifier");

        builder.HasIndex(d => d.Path)
            .HasMethod("gist")
            .HasDatabaseName("ix_departments_path");

        builder.HasIndex(d => d.CreatedAt)
            .HasDatabaseName("ix_departments_created_at");

        builder.HasIndex(d => d.ParentId)
            .HasDatabaseName("ix_departments_parent_id");

        builder.HasIndex(d => d.DeletedAt)
            .HasFilter("is_active = FALSE")
            .HasDatabaseName("ix_departments_deleted_at");

        builder.HasIndex(d => d.Name)
            .HasDatabaseName("ix_departments_name")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.Property(d => d.Id)
            .HasColumnName("id")
            .HasConversion(di => di.Value, guid => new DepartmentId(guid))
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
            .IsRequired(false)
            .HasConversion(di => di!.Value, guid => new DepartmentId(guid));

        builder.Property(d => d.Path)
            .HasColumnName("path")
            .HasColumnType("ltree")
            .HasMaxLength(LengthConstants.LENGTH_500)
            .IsRequired()
            .HasConversion(p => p.Value, s => DepartmentPath.Create(s).Value);

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

        builder.Property(d => d.DeletedAt)
            .HasColumnName("deleted_at")
            .IsRequired(false);
    }
}