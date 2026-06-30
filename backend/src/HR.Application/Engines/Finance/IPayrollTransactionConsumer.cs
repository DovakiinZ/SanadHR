using HR.Domain.Enums;

namespace HR.Application.Engines.Finance;

/// <summary>One approved payroll transaction resolved as eligible for a specific run period.</summary>
public sealed record ConsumableTransaction(
    Guid TransactionId,
    Guid EmployeeId,
    PayrollTransactionKind Kind,
    string TypeCode,
    decimal Amount,
    DateTime EffectiveDate);

/// <summary>Loads the approved addition/deduction records a run should consume for its period, applying
/// cutoff carry-over. Read-only — never writes.</summary>
public interface IPayrollTransactionConsumer
{
    Task<IReadOnlyList<ConsumableTransaction>> GetConsumableAsync(
        int periodYear, int periodMonth,
        IReadOnlyCollection<Guid> employeeIds,
        int cutoffDay, bool carryToNextPeriod,
        CancellationToken ct = default);
}
