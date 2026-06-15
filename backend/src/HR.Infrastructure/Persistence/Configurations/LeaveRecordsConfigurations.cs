using HR.Domain.Engines.Leave;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations;

public class LeaveRecordConfiguration : IEntityTypeConfiguration<LeaveRecord>
{
    public void Configure(EntityTypeBuilder<LeaveRecord> builder)
    {
        builder.ToTable("engine_leave_records");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RecordNumber).HasMaxLength(40).IsRequired();
        builder.Property(x => x.DaysCount).HasColumnType("decimal(8,2)");
        builder.Property(x => x.BalanceBefore).HasColumnType("decimal(8,2)");
        builder.Property(x => x.BalanceAfter).HasColumnType("decimal(8,2)");
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.AttachmentUrl).HasMaxLength(500);
        builder.Property(x => x.CancelReason).HasMaxLength(1000);
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.TenantId, x.EmployeeId });
        builder.HasIndex(x => new { x.TenantId, x.RequestInstanceId });
        builder.HasIndex(x => new { x.TenantId, x.RecordNumber }).IsUnique();
    }
}

public class LeaveBalanceTransactionConfiguration : IEntityTypeConfiguration<LeaveBalanceTransaction>
{
    public void Configure(EntityTypeBuilder<LeaveBalanceTransaction> builder)
    {
        builder.ToTable("engine_leave_balance_transactions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Delta).HasColumnType("decimal(8,2)");
        builder.Property(x => x.BalanceAfter).HasColumnType("decimal(8,2)");
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.LeaveTypeId, x.Year });
        builder.HasIndex(x => x.LeaveRecordId);
    }
}

public class LeaveAssignmentConfiguration : IEntityTypeConfiguration<LeaveAssignment>
{
    public void Configure(EntityTypeBuilder<LeaveAssignment> builder)
    {
        builder.ToTable("engine_leave_assignments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DaysCount).HasColumnType("decimal(8,2)");
        builder.Property(x => x.TargetScope).HasMaxLength(40);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.AttachmentUrl).HasMaxLength(500);
        builder.HasIndex(x => x.TenantId);
    }
}

public class LeaveCancellationConfiguration : IEntityTypeConfiguration<LeaveCancellation>
{
    public void Configure(EntityTypeBuilder<LeaveCancellation> builder)
    {
        builder.ToTable("engine_leave_cancellations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RestoredDays).HasColumnType("decimal(8,2)");
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.LeaveRecordId);
    }
}

public class LeaveAuditLogConfiguration : IEntityTypeConfiguration<LeaveAuditLog>
{
    public void Configure(EntityTypeBuilder<LeaveAuditLog> builder)
    {
        builder.ToTable("engine_leave_audit_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(80).IsRequired();
        builder.Property(x => x.DetailsAr).HasMaxLength(1000);
        builder.Property(x => x.DetailsEn).HasMaxLength(1000);
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.TenantId, x.LeaveRecordId });
        builder.HasIndex(x => new { x.TenantId, x.EmployeeId });
    }
}
