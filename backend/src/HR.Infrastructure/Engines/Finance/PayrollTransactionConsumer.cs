using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Finance;

public sealed class PayrollTransactionConsumer : IPayrollTransactionConsumer
{
    private readonly ApplicationDbContext _db;

    public PayrollTransactionConsumer(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<ConsumableTransaction>> GetConsumableAsync(
        int periodYear, int periodMonth,
        IReadOnlyCollection<Guid> employeeIds,
        int cutoffDay, bool carryToNextPeriod,
        CancellationToken ct = default)
    {
        if (employeeIds.Count == 0) return Array.Empty<ConsumableTransaction>();

        // All approved transactions for the population; period membership is resolved in memory via the
        // shared cutoff rule so the consumer and the impact preview can never drift.
        var approved = await _db.PayrollTransactions.AsNoTracking()
            .Where(t => t.Status == PayrollTransactionStatus.Approved && employeeIds.Contains(t.EmployeeId))
            .Select(t => new { t.Id, t.EmployeeId, t.Kind, t.TypeId, t.Amount, t.EffectiveDate })
            .ToListAsync(ct);

        var inPeriod = approved
            .Where(t => PayrollPeriodResolver.Resolve(t.EffectiveDate, cutoffDay, carryToNextPeriod) == (periodYear, periodMonth))
            .ToList();
        if (inPeriod.Count == 0) return Array.Empty<ConsumableTransaction>();

        var typeIds = inPeriod.Select(t => t.TypeId).Distinct().ToList();
        var typeCodes = await _db.MasterDataItems.AsNoTracking()
            .Where(m => typeIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, m => m.Code, ct);

        return inPeriod
            .Select(t => new ConsumableTransaction(
                t.Id, t.EmployeeId, t.Kind,
                typeCodes.TryGetValue(t.TypeId, out var code) ? code : "TXN",
                t.Amount, t.EffectiveDate))
            .ToList();
    }
}
