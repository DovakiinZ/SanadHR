using System.Text.Json;
using HR.Application.Engines.Completion;
using HR.Domain.Engines.Requests;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.Services.Requests;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Services.Completion;

/// <summary>
/// Translates a request's declarative <see cref="RequestImpactMapping"/> (boolean flags) + form
/// values + leave-type rules into the ordered list of completion-effect intents. This is the only
/// place that understands the flag→effect mapping; the engine and executors stay generic.
/// </summary>
public sealed class CompletionEffectFactory : ICompletionEffectFactory
{
    private readonly ApplicationDbContext _db;
    private readonly ILeaveService _leave;
    private static readonly JsonSerializerOptions Json = new(); // member names as-written (camelCase)

    public CompletionEffectFactory(ApplicationDbContext db, ILeaveService leave)
    {
        _db = db;
        _leave = leave;
    }

    public async Task<IReadOnlyList<EffectIntent>> BuildAsync(Guid requestInstanceId, CancellationToken ct)
    {
        var instance = await _db.RequestInstances
            .Include(r => r.RequestType).ThenInclude(t => t.ImpactMapping)
            .FirstOrDefaultAsync(r => r.Id == requestInstanceId, ct)
            ?? throw new InvalidOperationException($"RequestInstance {requestInstanceId} not found.");

        var type = instance.RequestType;
        var impact = type.ImpactMapping;
        var intents = new List<EffectIntent>();
        if (impact is null) return intents;

        // Leave-type rules drive the leave/attendance decisions (object-driven, never hardcoded).
        LeaveRules? rules = null;
        if (instance.LeaveTypeId is { } ltId)
        {
            var leaveItem = await _db.MasterDataItems.FirstOrDefaultAsync(m => m.Id == ltId, ct);
            rules = _leave.GetRules(leaveItem?.MetadataJson);
        }

        var seq = 0;

        // 1) Leave: create the approved leave record (+ balance), then mark attendance days.
        if (instance.LeaveTypeId is { } leaveTypeId && instance.StartDate is { } start && instance.EndDate is { } end)
        {
            var days = instance.DaysCount ?? 0m;
            var affectsBalance = impact.AffectsLeaveBalance && days > 0 && (rules?.Paid ?? true);

            intents.Add(new EffectIntent(EffectTypes.LeaveCreateApprovedLeave, ++seq, Serialize(new
            {
                leaveTypeId,
                startDate = start,
                endDate = end,
                daysCount = days,
                affectsBalance,
                entitledFallback = (decimal)(rules?.AnnualBalance ?? 30),
                notes = instance.DecisionNote,
            })));

            var marksAttendance = impact.AffectsAttendance && (rules?.AffectsAttendance ?? true);
            if (marksAttendance && end.Date >= start.Date)
                intents.Add(new EffectIntent(EffectTypes.AttendanceApplyLeaveDays, ++seq, Serialize(new
                {
                    startDate = start,
                    endDate = end,
                })));
        }

        // 2) Non-leave targets read submitted form values.
        var needsForm = impact.CreatesExpenseRecord || impact.CreatesLoanRecord || impact.CreatesAttendancePunch
            || (impact.AffectsAttendance && instance.LeaveTypeId is null);
        if (needsForm)
        {
            var fv = await LoadFormValuesAsync(instance.FormSubmissionId, ct);
            string? V(string code) => fv.TryGetValue(code, out var x) ? x.Value : null;
            string? Fl(string code) => fv.TryGetValue(code, out var x) ? x.FileUrl : null;

            if (impact.CreatesExpenseRecord)
            {
                var currency = await _db.Employees.Where(e => e.Id == instance.EmployeeId)
                    .Select(e => e.Currency).FirstOrDefaultAsync(ct) ?? "SAR";
                intents.Add(new EffectIntent(EffectTypes.ExpenseCreateClaim, ++seq, Serialize(new
                {
                    expenseCategory = V("expenseCategory"),
                    amount = V("amount"),
                    description = V("description") ?? V("reason"),
                    receipt = Fl("receipt"),
                    currency,
                })));
            }

            if (impact.CreatesLoanRecord)
            {
                var isAdvance = type.Code == "SALARY_ADVANCE";
                intents.Add(new EffectIntent(EffectTypes.LoanCreate, ++seq, Serialize(new
                {
                    loanType = V("loanType"),
                    amount = V("amount"),
                    installmentMonths = isAdvance ? "1" : V("installmentMonths"),
                    kind = isAdvance ? "Advance" : "Loan",
                })));
            }

            if (impact.CreatesAttendancePunch)
            {
                intents.Add(new EffectIntent(EffectTypes.AttendanceCreatePunch, ++seq, Serialize(new
                {
                    date = V("startDate") ?? V("date"),
                    checkIn = V("checkIn"),
                    checkOut = V("checkOut"),
                    reason = V("reason"),
                })));
            }
            else if (impact.AffectsAttendance && instance.LeaveTypeId is null)
            {
                intents.Add(new EffectIntent(EffectTypes.AttendanceCorrect, ++seq, Serialize(new
                {
                    date = V("startDate"),
                    reason = V("reason"),
                })));
            }
        }

        return intents;
    }

    private static string Serialize(object payload) => JsonSerializer.Serialize(payload, Json);

    /// <summary>Load a submission's values into a fieldCode → (value, fileUrl) map.</summary>
    private async Task<Dictionary<string, (string? Value, string? FileUrl)>> LoadFormValuesAsync(Guid submissionId, CancellationToken ct)
    {
        var vals = await _db.FormSubmissionValues.Where(v => v.FormSubmissionId == submissionId)
            .Select(v => new { v.FieldCode, v.Value, v.FileUrl }).ToListAsync(ct);
        return vals.GroupBy(v => v.FieldCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => (g.Last().Value, g.Last().FileUrl), StringComparer.OrdinalIgnoreCase);
    }
}
