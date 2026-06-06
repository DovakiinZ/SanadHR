using HR.Domain.Engines.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations.Engines;

public class MasterDataItemConfiguration : IEntityTypeConfiguration<MasterDataItem>
{
    public void Configure(EntityTypeBuilder<MasterDataItem> builder)
    {
        builder.ToTable("tenant_master_data_items");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ObjectType).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Color).HasMaxLength(20);
        builder.Property(x => x.Icon).HasMaxLength(80);
        builder.Property(x => x.MetadataJson).HasColumnType("jsonb");

        // Code is unique per tenant + object type.
        builder.HasIndex(x => new { x.TenantId, x.ObjectType, x.Code }).IsUnique();
        // Primary read path: lookups by tenant + type, active first, ordered.
        builder.HasIndex(x => new { x.TenantId, x.ObjectType, x.IsActive, x.SortOrder });
    }
}
