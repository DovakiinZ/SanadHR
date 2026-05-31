using HR.Domain.Engines.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations.Engines;

public class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("engine_audit_entries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserEmail).HasMaxLength(200);
        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Module).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.OldValues).HasColumnType("jsonb");
        builder.Property(x => x.NewValues).HasColumnType("jsonb");
        builder.Property(x => x.IpAddress).HasMaxLength(50);
        builder.Property(x => x.UserAgent).HasMaxLength(500);
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.Timestamp);
    }
}

public class AuditConfigurationEntityConfiguration : IEntityTypeConfiguration<AuditConfiguration>
{
    public void Configure(EntityTypeBuilder<AuditConfiguration> builder)
    {
        builder.ToTable("engine_audit_configurations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TrackedFields).HasColumnType("jsonb");
        builder.HasIndex(x => x.EntityType).IsUnique();
    }
}
