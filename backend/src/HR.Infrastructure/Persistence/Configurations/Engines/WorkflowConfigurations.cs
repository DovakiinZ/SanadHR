using HR.Domain.Engines.Workflows;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations.Engines;

public class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
    {
        builder.ToTable("engine_workflow_definitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TriggerEntityType).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        builder.HasMany(x => x.Versions).WithOne(x => x.WorkflowDefinition).HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Instances).WithOne(x => x.WorkflowDefinition).HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class WorkflowVersionConfiguration : IEntityTypeConfiguration<WorkflowVersion>
{
    public void Configure(EntityTypeBuilder<WorkflowVersion> builder)
    {
        builder.ToTable("engine_workflow_versions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Configuration).HasColumnType("jsonb");
        builder.HasMany(x => x.Nodes).WithOne(x => x.WorkflowVersion).HasForeignKey(x => x.WorkflowVersionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Edges).WithOne(x => x.WorkflowVersion).HasForeignKey(x => x.WorkflowVersionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkflowNodeConfiguration : IEntityTypeConfiguration<WorkflowNode>
{
    public void Configure(EntityTypeBuilder<WorkflowNode> builder)
    {
        builder.ToTable("engine_workflow_nodes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Configuration).HasColumnType("jsonb");
        builder.HasMany(x => x.Conditions).WithOne(x => x.WorkflowNode).HasForeignKey(x => x.WorkflowNodeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.ApproverRules).WithOne(x => x.WorkflowNode).HasForeignKey(x => x.WorkflowNodeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.OutgoingEdges).WithOne(x => x.SourceNode).HasForeignKey(x => x.SourceNodeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.IncomingEdges).WithOne(x => x.TargetNode).HasForeignKey(x => x.TargetNodeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.InstanceSteps).WithOne(x => x.WorkflowNode).HasForeignKey(x => x.WorkflowNodeId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class WorkflowEdgeConfiguration : IEntityTypeConfiguration<WorkflowEdge>
{
    public void Configure(EntityTypeBuilder<WorkflowEdge> builder)
    {
        builder.ToTable("engine_workflow_edges");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Condition).HasColumnType("jsonb");
    }
}

public class WorkflowConditionConfiguration : IEntityTypeConfiguration<WorkflowCondition>
{
    public void Configure(EntityTypeBuilder<WorkflowCondition> builder)
    {
        builder.ToTable("engine_workflow_conditions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Field).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Operator).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(500).IsRequired();
        builder.Property(x => x.LogicalOperator).HasMaxLength(10);
    }
}

public class WorkflowApproverRuleConfiguration : IEntityTypeConfiguration<WorkflowApproverRule>
{
    public void Configure(EntityTypeBuilder<WorkflowApproverRule> builder)
    {
        builder.ToTable("engine_workflow_approver_rules");
        builder.HasKey(x => x.Id);
    }
}

public class WorkflowInstanceConfiguration : IEntityTypeConfiguration<WorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
    {
        builder.ToTable("engine_workflow_instances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasMany(x => x.Steps).WithOne(x => x.WorkflowInstance).HasForeignKey(x => x.WorkflowInstanceId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkflowInstanceStepConfiguration : IEntityTypeConfiguration<WorkflowInstanceStep>
{
    public void Configure(EntityTypeBuilder<WorkflowInstanceStep> builder)
    {
        builder.ToTable("engine_workflow_instance_steps");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.AssignedToId);
    }
}
