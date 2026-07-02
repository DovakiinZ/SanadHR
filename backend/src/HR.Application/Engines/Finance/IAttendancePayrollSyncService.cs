using HR.Domain.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;

namespace HR.Application.Engines.Finance;

/// <summary>Outcome of an attendance payroll sync pass.</summary>
public sealed record AttendancePayrollSyncReport(int Created, int Updated, int Removed, int SkippedPosted, int TotalProcessed);

/// <summary>Materializes attendance impacts (absence/late/shortage deductions + overtime additions) into
/// Approved PayrollTransaction records — one per employee/period/kind — idempotently. Single source of
/// truth for attendance payroll effects.</summary>
public interface IAttendancePayrollSyncService
{
    Task<AttendancePayrollSyncReport> SyncAsync(
        PayrollDefinitionVersion version, PayrollPeriod period,
        IReadOnlyCollection<Guid> employeeIds, bool? includeOvertime = null, CancellationToken ct = default);
}
