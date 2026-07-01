using HR.Domain.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;

namespace HR.Application.Engines.Finance;

/// <summary>Outcome of an attendance-deduction sync pass.</summary>
public sealed record AttendanceDeductionSyncReport(int Created, int Updated, int Removed, int SkippedPosted, int TotalProcessed);

/// <summary>Materializes attendance penalties (absence/late/shortage) into Approved PayrollTransaction
/// deduction records — one per employee/period/kind — idempotently. The single source of truth for
/// attendance deductions (the ATTENDANCE_DED rule is retired).</summary>
public interface IAttendanceDeductionSyncService
{
    Task<AttendanceDeductionSyncReport> SyncAsync(
        PayrollDefinitionVersion version, PayrollPeriod period,
        IReadOnlyCollection<Guid> employeeIds, CancellationToken ct = default);
}
