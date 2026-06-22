using HR.Application.Engines.Completion;
using HR.Domain.Engines.Expenses;
using HR.Infrastructure.Persistence;

namespace HR.Modules.Expenses.Completion;

/// <summary>Effect: create the tracked <see cref="Expense"/> record from an approved expense claim.</summary>
public sealed class ExpenseCreateClaimExecutor : IEffectExecutor
{
    private readonly ApplicationDbContext _db;

    public ExpenseCreateClaimExecutor(ApplicationDbContext db) => _db = db;

    public string EffectType => EffectTypes.ExpenseCreateClaim;

    public Task<EffectExecutionResult> ExecuteAsync(EffectContext ctx, CancellationToken ct)
    {
        var expense = new Expense
        {
            EmployeeId = ctx.EmployeeId,
            ExpenseCategoryId = ctx.Guid("expenseCategory"),
            Amount = ctx.Dec("amount"),
            Currency = ctx.Str("currency") ?? "SAR",
            Description = ctx.Str("description"),
            ReceiptUrl = ctx.Str("receipt"),
            Status = "Approved",
            RequestInstanceId = ctx.RequestInstanceId,
            DecidedAt = DateTime.UtcNow,
        };
        _db.Expenses.Add(expense);

        return Task.FromResult(EffectExecutionResult.Ok(
            targetEntityType: "Expense",
            targetRecordId: expense.Id,
            after: new { expense.Amount, expense.Currency, expense.Status },
            summary: $"Expense {expense.Amount} {expense.Currency} approved"));
    }
}
