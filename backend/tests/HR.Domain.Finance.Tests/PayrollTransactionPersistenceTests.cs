using HR.Application.Common.Interfaces;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class PayrollTransactionPersistenceTests
{
    private sealed class FakeUser : ICurrentUserService
    {
        public Guid UserId => Guid.Parse("22222222-2222-2222-2222-222222222222");
        public Guid TenantId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string? Email => "t@t.local";
        public IReadOnlyList<string> Permissions { get; } = new[] { "Payroll.Create" };
        public bool IsAuthenticated => true;
    }

    private static ApplicationDbContext Ctx(string name) => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options, new FakeUser());

    [Fact]
    public async Task Saving_a_transaction_assigns_tenant_audit_and_defaults()
    {
        var name = $"txn-{Guid.NewGuid()}";
        await using (var db = Ctx(name))
        {
            db.PayrollTransactions.Add(new PayrollTransaction
            {
                Kind = PayrollTransactionKind.Deduction,
                EmployeeId = Guid.NewGuid(),
                TypeId = Guid.NewGuid(),
                Amount = 150m,
                TransactionDate = new DateTime(2026, 7, 3, 0, 0, 0, DateTimeKind.Utc),
                EffectiveDate = new DateTime(2026, 7, 3, 0, 0, 0, DateTimeKind.Utc),
                TargetPeriodYear = 2026,
                TargetPeriodMonth = 7,
            });
            await db.SaveChangesAsync();
        }

        await using var verify = Ctx(name);
        var saved = await verify.PayrollTransactions.SingleAsync();
        Assert.Equal(PayrollTransactionStatus.Draft, saved.Status);     // default
        Assert.Equal("Manual", saved.SourceModule);                      // default
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), saved.TenantId); // auto tenant
        Assert.NotEqual(default, saved.CreatedAt);                       // auto audit
        Assert.Equal(150m, saved.Amount);
    }
}
