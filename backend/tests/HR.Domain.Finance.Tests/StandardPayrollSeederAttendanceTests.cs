using FluentAssertions;
using HR.Application.Common.Interfaces;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Engines.Finance;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class StandardPayrollSeederAttendanceTests
{
    private sealed class FakeUser : ICurrentUserService
    {
        public Guid UserId => Guid.Parse("22222222-2222-2222-2222-222222222222");
        public Guid TenantId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string? Email => "t@t.local";
        public IReadOnlyList<string> Permissions { get; } = Array.Empty<string>();
        public bool IsAuthenticated => true;
    }
    private static ApplicationDbContext Ctx(string n) => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(n).Options, new FakeUser());

    [Fact]
    public async Task New_tenant_seed_does_not_create_attendance_ded_rule()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        await new StandardPayrollSeeder(db).EnsureStandardMonthlyAsync();

        (await db.FinanceRules.AnyAsync(r => r.Code == "ATTENDANCE_DED")).Should().BeFalse();
    }

    [Fact]
    public async Task Existing_attendance_ded_rule_is_deactivated_on_reseed()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        await new StandardPayrollSeeder(db).EnsureStandardMonthlyAsync();
        // Simulate a legacy tenant that already had the rule.
        var versionId = await db.FinanceRuleSetVersions.Select(v => v.Id).FirstAsync();
        db.FinanceRules.Add(new Rule
        {
            RuleSetVersionId = versionId, Code = "ATTENDANCE_DED", Name = "ATTENDANCE_DED", NameAr = "x",
            Kind = PayComponentKind.Deduction, Sequence = 99, ExpressionText = "0",
            ExpressionAstJson = "{}", OutputComponentCode = "ATTENDANCE_DED", IsActive = true,
        });
        await db.SaveChangesAsync();

        await new StandardPayrollSeeder(db).EnsureStandardMonthlyAsync();

        (await db.FinanceRules.Where(r => r.Code == "ATTENDANCE_DED").AllAsync(r => !r.IsActive))
            .Should().BeTrue();
    }
}
