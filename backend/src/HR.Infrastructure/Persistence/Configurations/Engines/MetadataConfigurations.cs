using HR.Domain.Engines.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations.Engines;

public class MetadataDefinitionConfiguration : IEntityTypeConfiguration<MetadataDefinition>
{
    public void Configure(EntityTypeBuilder<MetadataDefinition> builder)
    {
        builder.ToTable("engine_metadata_definitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Module).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        builder.HasMany(x => x.Fields).WithOne(x => x.MetadataDefinition).HasForeignKey(x => x.MetadataDefinitionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Values).WithOne(x => x.MetadataDefinition).HasForeignKey(x => x.MetadataDefinitionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class MetadataFieldConfiguration : IEntityTypeConfiguration<MetadataField>
{
    public void Configure(EntityTypeBuilder<MetadataField> builder)
    {
        builder.ToTable("engine_metadata_fields");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.HasMany(x => x.Options).WithOne(x => x.MetadataField).HasForeignKey(x => x.MetadataFieldId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class MetadataOptionConfiguration : IEntityTypeConfiguration<MetadataOption>
{
    public void Configure(EntityTypeBuilder<MetadataOption> builder)
    {
        builder.ToTable("engine_metadata_options");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Value).HasMaxLength(500).IsRequired();
        builder.Property(x => x.LabelEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.LabelAr).HasMaxLength(200).IsRequired();
    }
}

public class MetadataValueConfiguration : IEntityTypeConfiguration<MetadataValue>
{
    public void Configure(EntityTypeBuilder<MetadataValue> builder)
    {
        builder.ToTable("engine_metadata_values");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Values).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasIndex(x => x.TenantId);
    }
}
