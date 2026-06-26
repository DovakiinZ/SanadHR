using HR.Application.Common.Interfaces;
using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Finance;

/// <summary>Append-only implementation of the financial ledger. Entries are inserted and never updated;
/// <see cref="ReverseAsync"/> writes an opposite counter-entry instead of mutating the original, so the
/// full history (and the audit trail) is preserved.</summary>
public sealed class FinancialLedger : IFinancialLedger
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogService _audit;

    public FinancialLedger(ApplicationDbContext db, ICurrentUserService currentUser, IAuditLogService audit)
    {
        _db = db;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<FinancialLedgerEntry> PostAsync(LedgerPostingRequest request, CancellationToken ct = default)
    {
        var entry = BuildEntry(request, await NextEntryNumberAsync(ct));
        _db.Set<FinancialLedgerEntry>().Add(entry);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("LedgerPosted", nameof(FinancialLedgerEntry), entry.Id,
            null, new { entry.EntryNumber, entry.ComponentCode, entry.Amount, entry.Currency, entry.Direction }, ct);
        return entry;
    }

    public async Task<IReadOnlyList<FinancialLedgerEntry>> PostManyAsync(
        IEnumerable<LedgerPostingRequest> requests, CancellationToken ct = default)
    {
        var list = requests.ToList();
        if (list.Count == 0) return Array.Empty<FinancialLedgerEntry>();

        var seq = await NextEntrySequenceAsync(ct);
        var year = DateTime.UtcNow.Year;
        var entries = new List<FinancialLedgerEntry>(list.Count);
        foreach (var request in list)
        {
            entries.Add(BuildEntry(request, FormatEntryNumber(year, ++seq)));
        }

        _db.Set<FinancialLedgerEntry>().AddRange(entries);
        await _db.SaveChangesAsync(ct);
        return entries;
    }

    public async Task<FinancialLedgerEntry> ReverseAsync(Guid entryId, string reason, CancellationToken ct = default)
    {
        var original = await _db.Set<FinancialLedgerEntry>().FirstOrDefaultAsync(e => e.Id == entryId, ct)
            ?? throw new InvalidOperationException($"Ledger entry {entryId} not found.");

        if (original.Status == LedgerEntryStatus.Reversal)
            throw new InvalidOperationException("Cannot reverse a reversal entry.");

        var alreadyReversed = await _db.Set<FinancialLedgerEntry>()
            .AnyAsync(e => e.ReversesEntryId == entryId, ct);
        if (alreadyReversed)
            throw new InvalidOperationException("Ledger entry has already been reversed.");

        var reversal = new FinancialLedgerEntry
        {
            EntryNumber = await NextEntryNumberAsync(ct),
            EmployeeId = original.EmployeeId,
            SourceModule = original.SourceModule,
            ComponentCode = original.ComponentCode,
            Description = $"Reversal of {original.EntryNumber}: {reason}",
            Amount = original.Amount,
            Currency = original.Currency,
            Direction = original.Direction == LedgerDirection.Credit ? LedgerDirection.Debit : LedgerDirection.Credit,
            ReferenceType = original.ReferenceType,
            ReferenceId = original.ReferenceId,
            PayrollRunId = original.PayrollRunId,
            Status = LedgerEntryStatus.Reversal,
            Version = original.Version,
            ReversesEntryId = original.Id,
            PostedAt = DateTime.UtcNow,
            ActorUserId = _currentUser.IsAuthenticated ? _currentUser.UserId : null,
        };

        _db.Set<FinancialLedgerEntry>().Add(reversal);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("LedgerReversed", nameof(FinancialLedgerEntry), original.Id,
            new { original.EntryNumber, original.Amount, original.Direction },
            new { reversal.EntryNumber, reason }, ct);
        return reversal;
    }

    public async Task<decimal> GetEmployeeBalanceAsync(Guid employeeId, string currency = "SAR", CancellationToken ct = default)
    {
        var cur = currency.Trim().ToUpperInvariant();
        var credits = await _db.Set<FinancialLedgerEntry>()
            .Where(e => e.EmployeeId == employeeId && e.Currency == cur && e.Direction == LedgerDirection.Credit)
            .SumAsync(e => (decimal?)e.Amount, ct) ?? 0m;
        var debits = await _db.Set<FinancialLedgerEntry>()
            .Where(e => e.EmployeeId == employeeId && e.Currency == cur && e.Direction == LedgerDirection.Debit)
            .SumAsync(e => (decimal?)e.Amount, ct) ?? 0m;
        return credits - debits;
    }

    public async Task<IReadOnlyList<FinancialLedgerEntry>> QueryAsync(LedgerQuery query, CancellationToken ct = default)
    {
        var q = _db.Set<FinancialLedgerEntry>().AsNoTracking().AsQueryable();
        if (query.EmployeeId is { } emp) q = q.Where(e => e.EmployeeId == emp);
        if (query.PayrollRunId is { } run) q = q.Where(e => e.PayrollRunId == run);
        if (query.SourceModule is { } src) q = q.Where(e => e.SourceModule == src);
        if (query.From is { } from) q = q.Where(e => e.PostedAt >= from);
        if (query.To is { } to) q = q.Where(e => e.PostedAt <= to);
        return await q.OrderBy(e => e.PostedAt).ThenBy(e => e.EntryNumber).ToListAsync(ct);
    }

    private FinancialLedgerEntry BuildEntry(LedgerPostingRequest request, string entryNumber)
    {
        if (request.Amount < 0m)
            throw new InvalidOperationException("Ledger amount must be non-negative; use Direction to express sign.");
        return new FinancialLedgerEntry
        {
            EntryNumber = entryNumber,
            EmployeeId = request.EmployeeId,
            SourceModule = request.SourceModule,
            ComponentCode = request.ComponentCode,
            Description = request.Description,
            Amount = request.Amount,
            Currency = request.Currency.Trim().ToUpperInvariant(),
            Direction = request.Direction,
            ReferenceType = request.ReferenceType,
            ReferenceId = request.ReferenceId,
            PayrollRunId = request.PayrollRunId,
            Status = LedgerEntryStatus.Posted,
            Version = 1,
            PostedAt = request.PostedAt ?? DateTime.UtcNow,
            ActorUserId = _currentUser.IsAuthenticated ? _currentUser.UserId : null,
        };
    }

    private async Task<string> NextEntryNumberAsync(CancellationToken ct) =>
        FormatEntryNumber(DateTime.UtcNow.Year, await NextEntrySequenceAsync(ct) + 1);

    private async Task<int> NextEntrySequenceAsync(CancellationToken ct) =>
        await _db.Set<FinancialLedgerEntry>().CountAsync(ct);

    private static string FormatEntryNumber(int year, int sequence) => $"LED-{year}-{sequence:D8}";
}
