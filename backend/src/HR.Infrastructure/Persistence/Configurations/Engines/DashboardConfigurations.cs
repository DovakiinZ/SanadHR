using HR.Domain.Engines.Dashboards;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations.Engines;

public class DashboardDefinitionConfiguration : IEntityTypeConfiguration<DashboardDefinition>
{
    public void Configure(EntityTypeBuilder<DashboardDefinition> builder)
    {
        builder.ToTable("engine_dashboard_definitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        builder.HasMany(x => x.Widgets).WithOne(x => x.DashboardDefinition).HasForeignKey(x => x.DashboardDefinitionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class DashboardWidgetConfiguration : IEntityTypeConfiguration<DashboardWidget>
{
    public void Configure(EntityTypeBuilder<DashboardWidget> builder)
    {
        builder.ToTable("engine_dashboard_widgets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Configuration).HasColumnType("jsonb");
        builder.HasMany(x => x.Filters).WithOne(x => x.DashboardWidget).HasForeignKey(x => x.DashboardWidgetId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Layout).WithOne(x => x.DashboardWidget).HasForeignKey<WidgetLayout>(x => x.DashboardWidgetId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class WidgetFilterConfiguration : IEntityTypeConfiguration<WidgetFilter>
{
    public void Configure(EntityTypeBuilder<WidgetFilter> builder)
    {
        builder.ToTable("engine_widget_filters");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FieldCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Operator).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(500).IsRequired();
    }
}

public class WidgetLayoutConfiguration : IEntityTypeConfiguration<WidgetLayout>
{
    public void Configure(EntityTypeBuilder<WidgetLayout> builder)
    {
        builder.ToTable("engine_widget_layouts");
        builder.HasKey(x => x.Id);
    }
}
