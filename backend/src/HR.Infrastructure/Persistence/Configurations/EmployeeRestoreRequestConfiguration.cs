using HR.Modules.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations;

public class EmployeeRestoreRequestConfiguration : IEntityTypeConfiguration<EmployeeRestoreRequest>
{
    public void Configure(EntityTypeBuilder<EmployeeRestoreRequest> builder)
    {
        builder.ToTable("employee_restore_requests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(2000);
        builder.Property(x => x.RejectionReason).HasMaxLength(1000);
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.TenantId, x.EmployeeId });
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasMany(x => x.ApprovalSteps)
            .WithOne(s => s.Request)
            .HasForeignKey(s => s.EmployeeRestoreRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RestoreApprovalStepConfiguration : IEntityTypeConfiguration<RestoreApprovalStep>
{
    public void Configure(EntityTypeBuilder<RestoreApprovalStep> builder)
    {
        builder.ToTable("employee_restore_approval_steps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Comment).HasMaxLength(1000);
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.EmployeeRestoreRequestId);
    }
}
