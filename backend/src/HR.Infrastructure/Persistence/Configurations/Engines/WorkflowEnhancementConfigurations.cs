using HR.Domain.Engines.Workflows;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations.Engines;

public class WorkflowDynamicApproverConfiguration : IEntityTypeConfiguration<WorkflowDynamicApprover>
{
    public void Configure(EntityTypeBuilder<WorkflowDynamicApprover> builder)
    {
        builder.ToTable("engine_workflow_dynamic_approvers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ApproverStrategy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FallbackStrategy).HasMaxLength(100);
        builder.HasOne(x => x.WorkflowNode).WithMany().HasForeignKey(x => x.WorkflowNodeId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkflowDynamicConditionConfiguration : IEntityTypeConfiguration<WorkflowDynamicCondition>
{
    public void Configure(EntityTypeBuilder<WorkflowDynamicCondition> builder)
    {
        builder.ToTable("engine_workflow_dynamic_conditions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ConditionType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FieldPath).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Operator).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.LogicalOperator).HasMaxLength(10);
        builder.HasOne(x => x.WorkflowNode).WithMany().HasForeignKey(x => x.WorkflowNodeId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkflowActionConfiguration : IEntityTypeConfiguration<WorkflowAction>
{
    public void Configure(EntityTypeBuilder<WorkflowAction> builder)
    {
        builder.ToTable("engine_workflow_actions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ActionType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Configuration).HasColumnType("jsonb");
        builder.HasOne(x => x.WorkflowNode).WithMany().HasForeignKey(x => x.WorkflowNodeId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkflowSimulationConfiguration : IEntityTypeConfiguration<WorkflowSimulation>
{
    public void Configure(EntityTypeBuilder<WorkflowSimulation> builder)
    {
        builder.ToTable("engine_workflow_simulations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.InputData).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.Result).HasColumnType("jsonb").IsRequired();
    }
}
