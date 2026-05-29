using HR.Modules.Tenancy.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CompanyName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.CompanyNameAr).HasMaxLength(300);
        builder.Property(x => x.Domain).HasMaxLength(200);
        builder.Property(x => x.SubscriptionPlan).HasMaxLength(50);
        builder.HasIndex(x => x.Domain).IsUnique();
    }
}
