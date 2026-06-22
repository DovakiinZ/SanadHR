using HR.Application.Engines.Completion;
using HR.Domain.Engines.Loans;
using HR.Infrastructure.Persistence;

namespace HR.Modules.Loans.Completion;

/// <summary>
/// Effect: create a <see cref="Loan"/> (or salary advance) plus its monthly installment schedule
/// that payroll consumes as deductions.
/// </summary>
public sealed class LoanCreateExecutor : IEffectExecutor
{
    private readonly ApplicationDbContext _db;

    public LoanCreateExecutor(ApplicationDbContext db) => _db = db;

    public string EffectType => EffectTypes.LoanCreate;

    public Task<EffectExecutionResult> ExecuteAsync(EffectContext ctx, CancellationToken ct)
    {
        var principal = ctx.Dec("amount");
        var months = Math.Max(1, ctx.Int("installmentMonths"));
        var monthly = Math.Round(principal / months, 2);

        var loan = new Loan
        {
            EmployeeId = ctx.EmployeeId,
            LoanTypeId = ctx.Guid("loanType"),
            Kind = ctx.Str("kind") ?? "Loan",
            Principal = principal,
            InstallmentMonths = months,
            MonthlyInstallment = monthly,
            Status = "Active",
            RequestInstanceId = ctx.RequestInstanceId,
            StartDate = DateTime.UtcNow,
        };

        var firstMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);
        for (var i = 0; i < months; i++)
            loan.Installments.Add(new LoanInstallment { DueMonth = firstMonth.AddMonths(i), Amount = monthly, Paid = false });

        _db.Loans.Add(loan);

        return Task.FromResult(EffectExecutionResult.Ok(
            targetEntityType: "Loan",
            targetRecordId: loan.Id,
            after: new { loan.Kind, loan.Principal, loan.InstallmentMonths, loan.MonthlyInstallment },
            summary: $"{loan.Kind} {principal} over {months} month(s) ({monthly}/mo)"));
    }
}
