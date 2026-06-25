using HR.Application.Engines.Completion;
using HR.Domain.Engines.Leave;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Services.Completion.Executors;

/// <summary>
/// Effect: create the canonical approved <see cref="LeaveRecord"/>, deduct the paid balance, write
/// the balance-ledger entry, and log the leave action. Owned by the Leave module.
/// </summary>
public sealed class LeaveCreateApprovedLeaveExecutor : IEffectExecutor
{
    private readonly ApplicationDbContext _db;

    public LeaveCreateApprovedLeaveExecutor(ApplicationDbContext db) => _db = db;

    public string EffectType => EffectTypes.LeaveCreateApprovedLeave;

    public async Task<EffectExecutionResult> ExecuteAsync(EffectContext ctx, CancellationToken ct)
    {
        var leaveTypeId = ctx.Guid("leaveTypeId") ?? throw new InvalidOperationException("leaveTypeId missing.");
        var start = ctx.Date("startDate") ?? throw new InvalidOperationException("startDate missing.");
        var end = ctx.Date("endDate") ?? throw new InvalidOperationException("endDate missing.");
        var days = ctx.Dec("daysCount");
        var affectsBalance = ctx.Bool("affectsBalance");
        var entitledFallback = ctx.Dec("entitledFallback");
        var year = start.Year;

        var bal = await _db.LeaveBalances.FirstOrDefaultAsync(
            b => b.EmployeeId == ctx.EmployeeId && b.LeaveTypeId == leaveTypeId && b.Year == year, ct);
        var entitled = bal?.EntitledDays ?? (entitledFallback > 0 ? entitledFallback : 30m);
        var balanceBefore = (bal?.EntitledDays ?? entitled) + (bal?.CarriedForwardDays ?? 0m) - (bal?.UsedDays ?? 0m);
        var balanceAfter = balanceBefore;

        if (affectsBalance)
        {
            if (bal is null)
            {
                bal = new LeaveBalance { EmployeeId = ctx.EmployeeId, LeaveTypeId = leaveTypeId, Year = year, EntitledDays = entitled, UsedDays = 0m };
                _db.LeaveBalances.Add(bal);
            }
            bal.UsedDays += days;
            balanceAfter = bal.EntitledDays + bal.CarriedForwardDays - bal.UsedDays;
        }

        var record = new LeaveRecord
        {
            RecordNumber = await NextLeaveNumberAsync(ct),
            EmployeeId = ctx.EmployeeId,
            LeaveTypeId = leaveTypeId,
            StartDate = start,
            EndDate = end,
            DaysCount = days,
            AffectsBalance = affectsBalance,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            Status = LeaveRecordStatus.Approved,
            Source = LeaveRecordSource.Request,
            RequestInstanceId = ctx.RequestInstanceId,
            ApprovedByUserId = ctx.ActorUserId,
            ApprovedAt = DateTime.UtcNow,
            Notes = ctx.Str("notes"),
        };
        _db.LeaveRecords.Add(record);

        if (affectsBalance)
            _db.LeaveBalanceTransactions.Add(new LeaveBalanceTransaction
            {
                EmployeeId = ctx.EmployeeId, LeaveTypeId = leaveTypeId, Year = year, LeaveRecordId = record.Id,
                Type = LeaveTransactionType.Usage,
                Delta = -days, BalanceAfter = balanceAfter, Reason = "Leave request approved",
                ActorUserId = ctx.ActorUserId, At = DateTime.UtcNow,
            });

        _db.LeaveAuditLogs.Add(new LeaveAuditLog
        {
            LeaveRecordId = record.Id, EmployeeId = ctx.EmployeeId, Action = "Created",
            DetailsAr = $"إنشاء سجل إجازة من الطلب {ctx.RequestNumber}",
            DetailsEn = $"Leave record created from request {ctx.RequestNumber}",
            ActorUserId = ctx.ActorUserId, At = DateTime.UtcNow,
        });

        return EffectExecutionResult.Ok(
            targetEntityType: "LeaveRecord",
            targetRecordId: record.Id,
            before: new { balance = balanceBefore },
            after: new { balance = balanceAfter, record.RecordNumber, record.DaysCount },
            summary: $"Leave {record.RecordNumber}: {days} day(s), balance {balanceBefore} → {balanceAfter}");
    }

    private async Task<string> NextLeaveNumberAsync(CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _db.LeaveRecords.CountAsync(ct);
        return $"LV-{year}-{(count + 1):D6}";
    }
}
