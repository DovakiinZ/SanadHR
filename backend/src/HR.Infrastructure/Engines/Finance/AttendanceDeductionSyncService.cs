using HR.Application.Common.Exceptions;
using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Engines.MasterData;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Finance;

public sealed class AttendanceDeductionSyncService : IAttendanceDeductionSyncService
{
    private const string Source = "Attendance";
    private const string RefType = "AttendancePeriodPenalty";

    private static readonly (AttendancePenaltyKind Kind, string Code)[] KindCodes =
    {
        (AttendancePenaltyKind.Absence, "ABSENCE"),
        (AttendancePenaltyKind.Late, "LATE"),
        (AttendancePenaltyKind.Shortage, "SHORTAGE"),
    };

    private readonly ApplicationDbContext _db;
    private readonly IPayrollFactProvider _facts;
    private readonly AttendanceWageCalculator _attendance;

    public AttendanceDeductionSyncService(ApplicationDbContext db, IPayrollFactProvider facts, AttendanceWageCalculator attendance)
    { _db = db; _facts = facts; _attendance = attendance; }

    public async Task<AttendanceDeductionSyncReport> SyncAsync(
        PayrollDefinitionVersion version, PayrollPeriod period,
        IReadOnlyCollection<Guid> employeeIds, CancellationToken ct = default)
    {
        if (employeeIds.Count == 0 || !PayrollCalcSettings.IncludeAttendanceDeductions(version.CalcSettingsJson))
            return new AttendanceDeductionSyncReport(0, 0, 0, 0, 0);

        // Resolve the DeductionType master-data ids by code (config errors surface clearly).
        var typeByKind = await ResolveTypesAsync(ct);

        // Wages + aggregate penalty inputs, straight from the fact provider (identical to the retired rule).
        var inputs = await _facts.BuildInputsAsync(version, period, employeeIds, ct);
        var factsByEmp = inputs.ToDictionary(i => i.EmployeeId, i => i.Facts);

        // Per-day drill-down rows for the reference snapshot.
        var rowsByEmp = (await _attendance.BreakdownRowsAsync(employeeIds, period, ct))
            .GroupBy(r => r.EmployeeId).ToDictionary(g => g.Key, g => g.ToList());

        // Existing attendance-sourced records for this period (any live status).
        var existing = await _db.PayrollTransactions
            .Where(t => t.SourceModule == Source && employeeIds.Contains(t.EmployeeId)
                        && t.TargetPeriodYear == period.Year && t.TargetPeriodMonth == period.Month)
            .ToListAsync(ct);
        var existingByKey = existing
            .GroupBy(t => (t.EmployeeId, t.TypeId))
            .ToDictionary(g => g.Key, g => g.First());

        int created = 0, updated = 0, removed = 0, skipped = 0, processed = 0;

        foreach (var empId in employeeIds)
        {
            if (!factsByEmp.TryGetValue(empId, out var f)) continue;
            var dailyWage = Dec(f, "DailyWage");
            var hourlyWage = Dec(f, "HourlyWage");

            foreach (var (kind, code) in KindCodes)
            {
                processed++;
                var amount = kind switch
                {
                    AttendancePenaltyKind.Absence => Math.Round(Dec(f, "AbsentDays") * dailyWage, 2),
                    AttendancePenaltyKind.Late => Math.Round(Dec(f, "LateHours") * hourlyWage, 2),
                    AttendancePenaltyKind.Shortage => Math.Round(Dec(f, "ShortageHours") * hourlyWage, 2),
                    _ => 0m,
                };
                var typeId = typeByKind[kind];
                existingByKey.TryGetValue((empId, typeId), out var txn);

                if (txn is { Status: PayrollTransactionStatus.Posted })
                { skipped++; continue; }

                if (amount <= 0m)
                {
                    if (txn is not null && txn.Status != PayrollTransactionStatus.Cancelled)
                    {
                        txn.Status = PayrollTransactionStatus.Cancelled;
                        txn.StatusReason = "Attendance penalty cleared on re-sync.";
                        await ClearRefsAsync(txn.Id, ct);
                        removed++;
                    }
                    continue;
                }

                if (txn is null)
                {
                    txn = new PayrollTransaction
                    {
                        Kind = PayrollTransactionKind.Deduction,
                        EmployeeId = empId,
                        TypeId = typeId,
                        Amount = amount,
                        EffectiveDate = period.Start,           // period-scoped: never cutoff-carried
                        TransactionDate = period.End,
                        TargetPeriodYear = period.Year,
                        TargetPeriodMonth = period.Month,
                        SourceModule = Source,
                        ReferenceType = RefType,
                        Status = PayrollTransactionStatus.Approved,
                    };
                    _db.PayrollTransactions.Add(txn);
                    await _db.SaveChangesAsync(ct);        // materialize Id for the reference rows
                    created++;
                }
                else
                {
                    txn.Amount = amount;
                    if (txn.Status == PayrollTransactionStatus.Cancelled)
                        txn.Status = PayrollTransactionStatus.Approved; // penalty reappeared
                    updated++;
                }

                await WriteRefsAsync(txn.Id, kind, dailyWage, hourlyWage,
                    rowsByEmp.TryGetValue(empId, out var rs) ? rs : new(), ct);
            }
        }

        await _db.SaveChangesAsync(ct);
        return new AttendanceDeductionSyncReport(created, updated, removed, skipped, processed);
    }

    private async Task<IReadOnlyDictionary<AttendancePenaltyKind, Guid>> ResolveTypesAsync(CancellationToken ct)
    {
        var codes = KindCodes.Select(k => k.Code).ToArray();
        var found = await _db.MasterDataItems.AsNoTracking()
            .Where(m => m.ObjectType == MasterDataObjectType.DeductionType && codes.Contains(m.Code))
            .Select(m => new { m.Code, m.Id }).ToListAsync(ct);
        var byCode = found.ToDictionary(x => x.Code, x => x.Id, StringComparer.OrdinalIgnoreCase);
        var map = new Dictionary<AttendancePenaltyKind, Guid>();
        foreach (var (kind, code) in KindCodes)
        {
            if (!byCode.TryGetValue(code, out var id))
                throw new DomainException($"No DeductionType configured for attendance penalty '{kind}' (code {code}). Re-run payroll bootstrap.");
            map[kind] = id;
        }
        return map;
    }

    private async Task WriteRefsAsync(Guid txnId, AttendancePenaltyKind kind, decimal dailyWage, decimal hourlyWage,
        List<AttendanceBreakdownRow> rows, CancellationToken ct)
    {
        await ClearRefsAsync(txnId, ct);
        foreach (var r in rows.Where(r => r.PenaltyKind == kind))
        {
            var contribution = kind == AttendancePenaltyKind.Absence
                ? dailyWage * r.Days
                : Math.Round(r.Minutes / 60m, 2) * hourlyWage;
            _db.PayrollTransactionAttendanceReferences.Add(new PayrollTransactionAttendanceReference
            {
                PayrollTransactionId = txnId, AttendanceRecordId = r.AttendanceRecordId,
                Date = r.Date, PenaltyKind = kind, Minutes = r.Minutes, Days = r.Days,
                AmountContribution = contribution,
            });
        }
    }

    private async Task ClearRefsAsync(Guid txnId, CancellationToken ct)
    {
        var old = await _db.PayrollTransactionAttendanceReferences
            .Where(r => r.PayrollTransactionId == txnId).ToListAsync(ct);
        if (old.Count > 0) _db.PayrollTransactionAttendanceReferences.RemoveRange(old);
    }

    private static decimal Dec(IReadOnlyDictionary<string, object?> facts, string key) =>
        facts.TryGetValue(key, out var v) && v is not null ? Convert.ToDecimal(v) : 0m;
}
