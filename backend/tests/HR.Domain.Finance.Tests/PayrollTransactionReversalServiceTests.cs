using FluentAssertions;
using HR.Application.Common.Exceptions;
using HR.Application.Common.Interfaces;
using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Engines.Finance;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class PayrollTransactionReversalServiceTests
{
    private sealed class FakeUser : ICurrentUserService
    {
        public Guid UserId => Guid.Parse("22222222-2222-2222-2222-222222222222");
        public Guid TenantId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string? Email => "t@t.local";
        public IReadOnlyList<string> Permissions { get; } = new[] { "Payroll.Approve" };
        public bool IsAuthenticated => true;
    }

    private sealed class FakeLedger : IFinancialLedger
    {
        public List<Guid> Reversed { get; } = new();
        public Task<FinancialLedgerEntry> ReverseAsync(Guid entryId, string reason, CancellationToken ct = default)
        {
            Reversed.Add(entryId);
            return Task.FromResult(new FinancialLedgerEntry { Id = Guid.NewGuid(), EntryNumber = "REV-1", Amount = 0m });
        }
        public Task<FinancialLedgerEntry> PostAsync(LedgerPostingRequest r, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<FinancialLedgerEntry>> PostManyAsync(IEnumerable<LedgerPostingRequest> r, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<decimal> GetEmployeeBalanceAsync(Guid e, string c = "SAR", CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<FinancialLedgerEntry>> QueryAsync(LedgerQuery q, CancellationToken ct = default) => throw new NotImplementedException();
    }

    private static ApplicationDbContext Ctx(string name) => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options, new FakeUser());

    private static PayrollTransaction PostedTxn(Guid ledgerEntryId) => new()
    {
        Kind = PayrollTransactionKind.Deduction, EmployeeId = Guid.NewGuid(), TypeId = Guid.NewGuid(),
        Amount = 100m, EffectiveDate = new DateTime(2026, 7, 10, 0, 0, 0, DateTimeKind.Utc),
        TransactionDate = new DateTime(2026, 7, 10, 0, 0, 0, DateTimeKind.Utc),
        Status = PayrollTransactionStatus.Posted, LedgerEntryId = ledgerEntryId,
    };

    [Fact]
    public async Task Reverse_posted_transaction_marks_reversed_and_reverses_ledger_entry()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var ledgerEntryId = Guid.NewGuid();
        var txn = PostedTxn(ledgerEntryId);
        db.PayrollTransactions.Add(txn);
        await db.SaveChangesAsync();

        var ledger = new FakeLedger();
        var sut = new PayrollTransactionReversalService(db, ledger, new FakeUser());

        var result = await sut.ReverseAsync(txn.Id, "duplicate entry", createCorrection: false, correctedAmount: null, default);

        var reloaded = await db.PayrollTransactions.SingleAsync(t => t.Id == txn.Id);
        reloaded.Status.Should().Be(PayrollTransactionStatus.Reversed);
        reloaded.ReversalReason.Should().Be("duplicate entry");
        ledger.Reversed.Should().ContainSingle().Which.Should().Be(ledgerEntryId);
        result.CorrectionTransactionId.Should().BeNull();
    }

    [Fact]
    public async Task Reverse_with_correction_creates_a_draft_correction_in_a_new_period()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var txn = PostedTxn(Guid.NewGuid());
        db.PayrollTransactions.Add(txn);
        await db.SaveChangesAsync();

        var sut = new PayrollTransactionReversalService(db, new FakeLedger(), new FakeUser());

        var result = await sut.ReverseAsync(txn.Id, "wrong amount", createCorrection: true, correctedAmount: 80m, default);

        result.CorrectionTransactionId.Should().NotBeNull();
        var correction = await db.PayrollTransactions.SingleAsync(t => t.Id == result.CorrectionTransactionId);
        correction.Status.Should().Be(PayrollTransactionStatus.Draft);
        correction.Amount.Should().Be(80m);
        correction.SourceModule.Should().Be("Correction");
        correction.ReversesTransactionId.Should().Be(txn.Id);
        correction.Kind.Should().Be(txn.Kind);
        correction.EffectiveDate.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task Reversing_a_non_posted_transaction_throws_DomainException()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var txn = PostedTxn(Guid.NewGuid());
        txn.Status = PayrollTransactionStatus.Draft;
        db.PayrollTransactions.Add(txn);
        await db.SaveChangesAsync();

        var sut = new PayrollTransactionReversalService(db, new FakeLedger(), new FakeUser());

        var act = async () => await sut.ReverseAsync(txn.Id, "x", false, null, default);
        await act.Should().ThrowAsync<DomainException>();
    }
}
