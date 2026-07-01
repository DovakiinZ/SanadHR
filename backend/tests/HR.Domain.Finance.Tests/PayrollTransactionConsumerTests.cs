using FluentAssertions;
using HR.Application.Common.Interfaces;
using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Engines.MasterData;
using HR.Domain.Enums;
using HR.Infrastructure.Engines.Finance;
using HR.Infrastructure.Persistence;
using HR.Modules.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class PayrollTransactionConsumerTests
{
    private static readonly Guid Tenant = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private sealed class FakeUser : ICurrentUserService
    {
        public Guid UserId => Guid.Parse("22222222-2222-2222-2222-222222222222");
        public Guid TenantId => Tenant;
        public string? Email => "t@t.local";
        public IReadOnlyList<string> Permissions { get; } = new[] { "Payroll.View" };
        public bool IsAuthenticated => true;
    }

    private static ApplicationDbContext Ctx(string name) => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options, new FakeUser());

    private static DateTime Utc(int y, int m, int d) => new(y, m, d, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_only_approved_transactions_resolved_into_the_period()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var emp = new Employee { EmployeeNumber = "E1", FirstName = "Ali", LastName = "S", Email = "a@t.local" };
        db.Employees.Add(emp);
        var addType = new MasterDataItem { ObjectType = MasterDataObjectType.AdditionType, Code = "BONUS", NameAr = "م", NameEn = "Bonus" };
        db.MasterDataItems.Add(addType);
        await db.SaveChangesAsync();

        // in-period, approved -> consumed
        db.PayrollTransactions.Add(new PayrollTransaction { Kind = PayrollTransactionKind.Addition, EmployeeId = emp.Id, TypeId = addType.Id, Amount = 500m, EffectiveDate = Utc(2026, 7, 10), TransactionDate = Utc(2026, 7, 10), Status = PayrollTransactionStatus.Approved });
        // in-period but Draft -> excluded
        db.PayrollTransactions.Add(new PayrollTransaction { Kind = PayrollTransactionKind.Addition, EmployeeId = emp.Id, TypeId = addType.Id, Amount = 999m, EffectiveDate = Utc(2026, 7, 11), TransactionDate = Utc(2026, 7, 11), Status = PayrollTransactionStatus.Draft });
        // after cutoff -> carried to August -> excluded from July
        db.PayrollTransactions.Add(new PayrollTransaction { Kind = PayrollTransactionKind.Addition, EmployeeId = emp.Id, TypeId = addType.Id, Amount = 700m, EffectiveDate = Utc(2026, 7, 29), TransactionDate = Utc(2026, 7, 29), Status = PayrollTransactionStatus.Approved });
        await db.SaveChangesAsync();

        var sut = new PayrollTransactionConsumer(db);
        var result = await sut.GetConsumableAsync(2026, 7, new[] { emp.Id }, cutoffDay: 27, carryToNextPeriod: true);

        result.Should().HaveCount(1);
        result[0].Amount.Should().Be(500m);
        result[0].TypeCode.Should().Be("BONUS");
        result[0].Kind.Should().Be(PayrollTransactionKind.Addition);
    }
}
