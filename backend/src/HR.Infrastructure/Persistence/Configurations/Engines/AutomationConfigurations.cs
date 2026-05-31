using HR.Domain.Engines.Automation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations.Engines;

public class AutomationRuleConfiguration : IEntityTypeConfiguration<AutomationRule>
{
    public void Configure(EntityTypeBuilder<AutomationRule> builder)
    {
        builder.ToTable("engine_automation_rules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.TenantId);
        builder.HasMany(x => x.Triggers).WithOne(x => x.AutomationRule).HasForeignKey(x => x.AutomationRuleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Conditions).WithOne(x => x.AutomationRule).HasForeignKey(x => x.AutomationRuleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Actions).WithOne(x => x.AutomationRule).HasForeignKey(x => x.AutomationRuleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.ExecutionLogs).WithOne(x => x.AutomationRule).HasForeignKey(x => x.AutomationRuleId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AutomationTriggerConfiguration : IEntityTypeConfiguration<AutomationTrigger>
{
    public void Configure(EntityTypeBuilder<AutomationTrigger> builder)
    {
        builder.ToTable("engine_automation_triggers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Configuration).HasColumnType("jsonb");
    }
}

public class AutomationConditionConfiguration : IEntityTypeConfiguration<AutomationCondition>
{
    public void Configure(EntityTypeBuilder<AutomationCondition> builder)
    {
        builder.ToTable("engine_automation_conditions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Field).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Operator).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(500).IsRequired();
        builder.Property(x => x.LogicalOperator).HasMaxLength(10);
    }
}

public class AutomationActionConfiguration : IEntityTypeConfiguration<AutomationAction>
{
    public void Configure(EntityTypeBuilder<AutomationAction> builder)
    {
        builder.ToTable("engine_automation_actions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Configuration).HasColumnType("jsonb");
    }
}

public class AutomationExecutionLogConfiguration : IEntityTypeConfiguration<AutomationExecutionLog>
{
    public void Configure(EntityTypeBuilder<AutomationExecutionLog> builder)
    {
        builder.ToTable("engine_automation_execution_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TriggerEventType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
    }
}
