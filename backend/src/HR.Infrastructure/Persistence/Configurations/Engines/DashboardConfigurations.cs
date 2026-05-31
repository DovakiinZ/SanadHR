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
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.LayoutConfiguration).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        builder.HasOne(x => x.Category).WithMany(x => x.Dashboards).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);
        builder.HasMany(x => x.Widgets).WithOne(x => x.DashboardDefinition).HasForeignKey(x => x.DashboardDefinitionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Shares).WithOne(x => x.DashboardDefinition).HasForeignKey(x => x.DashboardDefinitionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class DashboardCategoryConfiguration : IEntityTypeConfiguration<DashboardCategory>
{
    public void Configure(EntityTypeBuilder<DashboardCategory> builder)
    {
        builder.ToTable("engine_dashboard_categories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Icon).HasMaxLength(100);
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}

public class DashboardTemplateConfiguration : IEntityTypeConfiguration<DashboardTemplate>
{
    public void Configure(EntityTypeBuilder<DashboardTemplate> builder)
    {
        builder.ToTable("engine_dashboard_templates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.LayoutConfiguration).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.WidgetConfiguration).HasColumnType("jsonb").IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}

public class DashboardShareConfiguration : IEntityTypeConfiguration<DashboardShare>
{
    public void Configure(EntityTypeBuilder<DashboardShare> builder)
    {
        builder.ToTable("engine_dashboard_shares");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SharedBy).HasMaxLength(200);
    }
}

public class DashboardWidgetConfiguration : IEntityTypeConfiguration<DashboardWidget>
{
    public void Configure(EntityTypeBuilder<DashboardWidget> builder)
    {
        builder.ToTable("engine_dashboard_widgets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TitleEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TitleAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Configuration).HasColumnType("jsonb");
        builder.Property(x => x.DataSourceConfig).HasColumnType("jsonb");
        builder.HasMany(x => x.Filters).WithOne(x => x.DashboardWidget).HasForeignKey(x => x.DashboardWidgetId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Drilldowns).WithOne(x => x.DashboardWidget).HasForeignKey(x => x.DashboardWidgetId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Layout).WithOne(x => x.DashboardWidget).HasForeignKey<WidgetLayout>(x => x.DashboardWidgetId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class WidgetDefinitionConfiguration : IEntityTypeConfiguration<WidgetDefinition>
{
    public void Configure(EntityTypeBuilder<WidgetDefinition> builder)
    {
        builder.ToTable("engine_widget_definitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Icon).HasMaxLength(100);
        builder.Property(x => x.DefaultConfiguration).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        builder.HasMany(x => x.DataSources).WithOne(x => x.WidgetDefinition).HasForeignKey(x => x.WidgetDefinitionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Permissions).WithOne(x => x.WidgetDefinition).HasForeignKey(x => x.WidgetDefinitionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class WidgetDataSourceConfiguration : IEntityTypeConfiguration<WidgetDataSource>
{
    public void Configure(EntityTypeBuilder<WidgetDataSource> builder)
    {
        builder.ToTable("engine_widget_data_sources");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.QueryTemplate).HasColumnType("jsonb");
        builder.Property(x => x.ApiEndpoint).HasMaxLength(500);
        builder.Property(x => x.AggregationField).HasMaxLength(200);
        builder.Property(x => x.GroupByField).HasMaxLength(200);
        builder.Property(x => x.DateRangeField).HasMaxLength(200);
    }
}

public class WidgetDrilldownConfiguration : IEntityTypeConfiguration<WidgetDrilldown>
{
    public void Configure(EntityTypeBuilder<WidgetDrilldown> builder)
    {
        builder.ToTable("engine_widget_drilldowns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TargetRoute).HasMaxLength(500);
        builder.Property(x => x.FilterMapping).HasColumnType("jsonb");
        builder.Property(x => x.LabelEn).HasMaxLength(200);
        builder.Property(x => x.LabelAr).HasMaxLength(200);
    }
}

public class WidgetPermissionConfiguration : IEntityTypeConfiguration<WidgetPermission>
{
    public void Configure(EntityTypeBuilder<WidgetPermission> builder)
    {
        builder.ToTable("engine_widget_permissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PermissionCode).HasMaxLength(200).IsRequired();
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
