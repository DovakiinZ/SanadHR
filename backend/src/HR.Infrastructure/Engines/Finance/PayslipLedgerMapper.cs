using System.Text.Json;
using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Enums;

namespace HR.Infrastructure.Engines.Finance;

/// <summary>Pure mapping from a frozen payslip's components to immutable ledger postings: earnings credit
/// the employee, deductions debit them, employer contributions and information components don't touch the
/// employee ledger. Entry numbers are deterministic per payslip/component so concurrent workers — and
/// resumed runs — never collide or double-number.</summary>
public static class PayslipLedgerMapper
{
    public const string PayslipReference = "PayrollPayslip";

    public static List<LedgerPostingRequest> Map(Guid runId, PayrollPayslip payslip)
    {
        var postings = new List<LedgerPostingRequest>();
        if (string.IsNullOrWhiteSpace(payslip.ComponentsJson)) return postings;

        using var doc = JsonDocument.Parse(payslip.ComponentsJson);
        if (!doc.RootElement.TryGetProperty("components", out var comps) || comps.ValueKind != JsonValueKind.Array)
            return postings;

        var index = 0;
        foreach (var c in comps.EnumerateArray())
        {
            var applied = c.TryGetProperty("Applied", out var ap) && ap.ValueKind == JsonValueKind.True;
            if (!applied) continue;
            var amount = c.TryGetProperty("Amount", out var am) && am.TryGetDecimal(out var d) ? d : 0m;
            if (amount == 0m) continue;
            var kind = c.TryGetProperty("Kind", out var k) && k.TryGetInt32(out var ki)
                ? (PayComponentKind)ki : PayComponentKind.Information;

            LedgerDirection direction;
            if (kind == PayComponentKind.Earning) direction = LedgerDirection.Credit;
            else if (kind == PayComponentKind.Deduction) direction = LedgerDirection.Debit;
            else continue;

            var componentCode = c.TryGetProperty("ComponentCode", out var cc) ? cc.GetString() ?? "COMP" : "COMP";

            postings.Add(new LedgerPostingRequest
            {
                EmployeeId = payslip.EmployeeId,
                SourceModule = FinanceSourceModule.Payroll,
                ComponentCode = componentCode,
                Amount = Math.Abs(amount),
                Currency = payslip.Currency,
                Direction = direction,
                Description = $"Payroll {componentCode} for {payslip.EmployeeNumber}",
                ReferenceType = PayslipReference,
                ReferenceId = payslip.Id,
                PayrollRunId = runId,
                EntryNumber = $"PRL-{payslip.Id:N}-{index:D2}",
            });
            index++;
        }
        return postings;
    }
}
