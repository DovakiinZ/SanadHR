using HR.Application.Common.Interfaces;
using HR.Application.Engines.Scope;
using HR.Infrastructure.Persistence;
using HR.Modules.Employees.Entities;
using HR.Modules.Employees.Scope;
using HR.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.Modules.Employees.Tests;

public class EmployeeScopeProvidersTests
{
    private sealed class FakeUser : ICurrentUserService
    {
        public Guid UserId => Guid.Parse("22222222-2222-2222-2222-222222222222");
        public Guid TenantId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string? Email => "t@t.local";
        public IReadOnlyList<string> Permissions { get; } = new[] { "Payroll.View" };
        public bool IsAuthenticated => true;
    }

    private static ApplicationDbContext Ctx() => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase($"emp-{Guid.NewGuid()}").Options,
        new FakeUser());

    private static Employee Emp(string num, Guid? dept = null, EmployeeStatus status = EmployeeStatus.Active) => new()
    {
        EmployeeNumber = num, FirstName = num, LastName = "X", Email = $"{num}@t.local",
        DepartmentId = dept, Status = status, BasicSalary = 1000m,
    };

    [Fact]
    public async Task Department_provider_resolves_by_value_ids()
    {
        var sales = Guid.NewGuid();
        await using var db = Ctx();
        var a = Emp("E1", sales); var b = Emp("E2", Guid.NewGuid());
        db.Employees.AddRange(a, b);
        await db.SaveChangesAsync();

        var provider = new DepartmentScopeProvider(db);
        var ids = await provider.ResolveEmployeesAsync(new[] { sales }, default);
        Assert.Equal(new[] { a.Id }, ids);
        Assert.Equal("Department", provider.DimensionKey);
        Assert.True(provider.Info.IsAvailable);
    }

    [Fact]
    public async Task Base_population_excludes_terminated_and_resigned()
    {
        await using var db = Ctx();
        var active = Emp("A1");
        db.Employees.AddRange(active, Emp("T1", status: EmployeeStatus.Terminated), Emp("R1", status: EmployeeStatus.Resigned));
        await db.SaveChangesAsync();

        var ids = await new ActiveEmployeePopulationProvider(db).ResolveAllAsync(default);
        Assert.Equal(new[] { active.Id }, ids);
    }

    [Fact]
    public async Task Status_provider_uses_static_enum_value_source()
    {
        await using var db = Ctx();
        var p = new StatusScopeProvider(db);
        Assert.Equal(ScopeValueSourceKind.StaticEnum, p.Info.ValueSource.Kind);
    }
}
