using HR.Domain.Engines.ObjectRegistry;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations.Engines;

public class ObjectDefinitionConfiguration : IEntityTypeConfiguration<ObjectDefinition>
{
    public void Configure(EntityTypeBuilder<ObjectDefinition> builder)
    {
        builder.ToTable("engine_object_definitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Module).HasMaxLength(100).IsRequired();
        builder.Property(x => x.TableName).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        builder.HasMany(x => x.Fields).WithOne(x => x.ObjectDefinition).HasForeignKey(x => x.ObjectDefinitionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.SourceRelationships).WithOne(x => x.SourceObject).HasForeignKey(x => x.SourceObjectId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.TargetRelationships).WithOne(x => x.TargetObject).HasForeignKey(x => x.TargetObjectId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Permissions).WithOne(x => x.ObjectDefinition).HasForeignKey(x => x.ObjectDefinitionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ObjectFieldConfiguration : IEntityTypeConfiguration<ObjectField>
{
    public void Configure(EntityTypeBuilder<ObjectField> builder)
    {
        builder.ToTable("engine_object_fields");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
    }
}

public class ObjectRelationshipConfiguration : IEntityTypeConfiguration<ObjectRelationship>
{
    public void Configure(EntityTypeBuilder<ObjectRelationship> builder)
    {
        builder.ToTable("engine_object_relationships");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ForeignKeyField).HasMaxLength(200).IsRequired();
    }
}

public class ObjectPermissionConfiguration : IEntityTypeConfiguration<ObjectPermission>
{
    public void Configure(EntityTypeBuilder<ObjectPermission> builder)
    {
        builder.ToTable("engine_object_permissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PermissionCode).HasMaxLength(200).IsRequired();
    }
}
