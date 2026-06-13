using HR.Domain.Engines.Attendance;
using HR.Domain.Engines.Leave;
using HR.Domain.Engines.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations.Engines;

public class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.ToTable("engine_leave_balances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntitledDays).HasColumnType("numeric(7,2)");
        builder.Property(x => x.UsedDays).HasColumnType("numeric(7,2)");
        builder.Property(x => x.CarriedForwardDays).HasColumnType("numeric(7,2)");
        builder.Ignore(x => x.RemainingDays);
        builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.LeaveTypeId, x.Year }).IsUnique();
    }
}

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("engine_attendance_records");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Source).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.Date });
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("engine_notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TitleEn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.TitleAr).HasMaxLength(300).IsRequired();
        builder.Property(x => x.BodyEn).HasMaxLength(2000);
        builder.Property(x => x.BodyAr).HasMaxLength(2000);
        builder.Property(x => x.Category).HasMaxLength(100);
        builder.Property(x => x.Link).HasMaxLength(500);
        builder.HasIndex(x => new { x.TenantId, x.UserId, x.IsRead });
    }
}
