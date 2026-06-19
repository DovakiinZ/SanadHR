using HR.Domain.Engines.FlowBuilder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations.Engines;

public class FlowWorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
    {
        builder.ToTable("flow_workflow_definitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();

        builder.HasMany(x => x.Steps)
            .WithOne(x => x.Definition)
            .HasForeignKey(x => x.DefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Requests)
            .WithOne(x => x.Definition)
            .HasForeignKey(x => x.DefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class FlowWorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
{
    public void Configure(EntityTypeBuilder<WorkflowStep> builder)
    {
        builder.ToTable("flow_workflow_steps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Config).HasColumnType("jsonb");
        builder.HasIndex(x => x.DefinitionId);
        // NextStepIdSuccess / NextStepIdFailure are intentionally NOT modelled as FKs:
        // they are soft pointers within the same definition so the builder can rewire the
        // graph freely without EF cascade-cycle restrictions.
    }
}

public class FlowWorkflowRequestConfiguration : IEntityTypeConfiguration<WorkflowRequest>
{
    public void Configure(EntityTypeBuilder<WorkflowRequest> builder)
    {
        builder.ToTable("flow_workflow_requests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RequestNumber).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Payload).HasColumnType("jsonb");
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.DefinitionId, x.Status });
        builder.HasIndex(x => x.RequesterId);

        builder.HasMany(x => x.AuditTrail)
            .WithOne(x => x.Request)
            .HasForeignKey(x => x.RequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FlowWorkflowAuditTrailConfiguration : IEntityTypeConfiguration<WorkflowAuditTrail>
{
    public void Configure(EntityTypeBuilder<WorkflowAuditTrail> builder)
    {
        builder.ToTable("flow_workflow_audit_trail");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(60).IsRequired();
        builder.Property(x => x.Result).HasMaxLength(500);
        builder.Property(x => x.StepName).HasMaxLength(200);
        builder.Property(x => x.Comment).HasMaxLength(2000);
        builder.HasIndex(x => x.RequestId);
    }
}
