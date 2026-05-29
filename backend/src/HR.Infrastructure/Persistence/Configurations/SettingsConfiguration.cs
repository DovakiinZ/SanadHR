using HR.Modules.Settings.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations;

public class CompanySettingsConfiguration : IEntityTypeConfiguration<CompanySettings>
{
    public void Configure(EntityTypeBuilder<CompanySettings> builder)
    {
        builder.ToTable("company_settings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CompanyName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.CompanyNameAr).HasMaxLength(300);
        builder.Property(x => x.Currency).HasMaxLength(10);
        builder.Property(x => x.Timezone).HasMaxLength(100);
        builder.HasIndex(x => x.TenantId).IsUnique();
    }
}
