using HR.Domain.Engines.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations.Engines;

public class PermissionTemplateConfiguration : IEntityTypeConfiguration<PermissionTemplate>
{
    public void Configure(EntityTypeBuilder<PermissionTemplate> builder)
    {
        builder.ToTable("engine_permission_templates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.TenantId);
        builder.HasMany(x => x.Items).WithOne(x => x.PermissionTemplate).HasForeignKey(x => x.PermissionTemplateId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PermissionTemplateItemConfiguration : IEntityTypeConfiguration<PermissionTemplateItem>
{
    public void Configure(EntityTypeBuilder<PermissionTemplateItem> builder)
    {
        builder.ToTable("engine_permission_template_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PermissionCode).HasMaxLength(200).IsRequired();
    }
}

public class UserPermissionOverrideConfiguration : IEntityTypeConfiguration<UserPermissionOverride>
{
    public void Configure(EntityTypeBuilder<UserPermissionOverride> builder)
    {
        builder.ToTable("engine_user_permission_overrides");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PermissionCode).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.UserId });
    }
}

public class UserPermissionScopeConfiguration : IEntityTypeConfiguration<UserPermissionScope>
{
    public void Configure(EntityTypeBuilder<UserPermissionScope> builder)
    {
        builder.ToTable("engine_user_permission_scopes");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.UserId);
    }
}

public class UserPermissionTemplateConfiguration : IEntityTypeConfiguration<UserPermissionTemplate>
{
    public void Configure(EntityTypeBuilder<UserPermissionTemplate> builder)
    {
        builder.ToTable("UserPermissionTemplates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.AssignedAt).IsRequired();
        builder.HasOne(x => x.PermissionTemplate)
            .WithMany()
            .HasForeignKey(x => x.PermissionTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.UserId, x.PermissionTemplateId }).IsUnique();
    }
}
