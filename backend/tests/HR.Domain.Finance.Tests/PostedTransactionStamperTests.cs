using FluentAssertions;
using HR.Application.Common.Interfaces;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Engines.Finance;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class PostedTransactionStamperTests
{
    private sealed class FakeUser : ICurrentUserService
    {
        public Guid UserId => Guid.Parse("22222222-2222-2222-2222-222222222222");
        public Guid TenantId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string? Email => "t@t.local";
        public IReadOnlyList<string> Permissions { get; } = new[] { "Payroll.Lock" };
        public bool IsAuthenticated => true;
    }

    private static ApplicationDbContext Ctx(string name) => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options, new FakeUser());

    [Fact]
    public async Task Stamps_referenced_transaction_as_posted_and_leaves_others_untouched()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var runId = Guid.NewGuid();
        var empId = Guid.NewGuid();
        var postedAt = new DateTime(2026, 7, 31, 12, 0, 0, DateTimeKind.Utc);

        var consumed = new PayrollTransaction { Kind = PayrollTransactionKind.Addition, EmployeeId = empId, TypeId = Guid.NewGuid(), Amount = 200m, EffectiveDate = postedAt, TransactionDate = postedAt, Status = PayrollTransactionStatus.Approved };
        var untouched = new PayrollTransaction { Kind = PayrollTransactionKind.Addition, EmployeeId = empId, TypeId = Guid.NewGuid(), Amount = 9m, EffectiveDate = postedAt, TransactionDate = postedAt, Status = PayrollTransactionStatus.Approved };
        db.PayrollTransactions.AddRange(consumed, untouched);
        await db.SaveChangesAsync();

        var entry = new FinancialLedgerEntry { EntryNumber = "PRL-x-00", EmployeeId = empId, ComponentCode = "BONUS", Amount = 200m, Direction = LedgerDirection.Credit, PayrollRunId = runId, ReferenceType = "PayrollTransaction", ReferenceId = consumed.Id, PostedAt = postedAt };
        db.FinancialLedgerEntries.Add(entry);
        await db.SaveChangesAsync();

        await PostedTransactionStamper.StampAsync(db, runId, empId, default);

        var reloadedConsumed = await db.PayrollTransactions.SingleAsync(t => t.Id == consumed.Id);
        reloadedConsumed.Status.Should().Be(PayrollTransactionStatus.Posted);
        reloadedConsumed.PayrollRunId.Should().Be(runId);
        reloadedConsumed.LedgerEntryId.Should().Be(entry.Id);
        reloadedConsumed.PostedAt.Should().Be(postedAt);

        var reloadedUntouched = await db.PayrollTransactions.SingleAsync(t => t.Id == untouched.Id);
        reloadedUntouched.Status.Should().Be(PayrollTransactionStatus.Approved);
    }
}
