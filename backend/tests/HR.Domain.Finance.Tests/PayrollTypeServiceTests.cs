using HR.Application.Common.Interfaces;
using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Engines.Finance;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class PayrollTypeServiceTests
{
    private sealed class FakeUser : ICurrentUserService
    {
        public Guid UserId => Guid.Parse("22222222-2222-2222-2222-222222222222");
        public Guid TenantId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string? Email => "t@t.local";
        public IReadOnlyList<string> Permissions { get; } = new[] { "Payroll.Configure" };
        public bool IsAuthenticated => true;
    }

    private static ApplicationDbContext Ctx(string name) => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options, new FakeUser());

    private static PayrollTypeService Svc(ApplicationDbContext db) => new(db, new FakeUser());

    [Fact]
    public async Task Create_type_makes_a_draft_version_v1()
    {
        var name = $"db-{Guid.NewGuid()}";
        await using var db = Ctx(name);
        var svc = Svc(db);
        var id = await svc.CreateTypeAsync(new CreatePayrollTypeArgs("MONTHLY2", "Monthly 2", "شهري", null), default);
        var def = await db.PayrollDefinitions.Include(d => d.Versions).FirstAsync(d => d.Id == id);
        Assert.Single(def.Versions);
        Assert.Equal(VersionStatus.Draft, def.Versions.First().Status);
        Assert.Equal(1, def.Versions.First().VersionNumber);
    }

    [Fact]
    public async Task Publish_supersedes_prior_and_closes_its_EffectiveTo()
    {
        var name = $"db-{Guid.NewGuid()}";
        Guid typeId, v1Id, v2Id;
        await using (var db = Ctx(name))
        {
            var svc = Svc(db);
            typeId = await svc.CreateTypeAsync(new CreatePayrollTypeArgs("M", "M", "م", null), default);
            v1Id = (await db.PayrollDefinitionVersions.FirstAsync(v => v.PayrollDefinitionId == typeId)).Id;
            await svc.PublishVersionAsync(typeId, v1Id, default);
            v2Id = await svc.CloneVersionAsync(typeId, v1Id, default);
            await svc.PublishVersionAsync(typeId, v2Id, default);
        }
        await using (var db = Ctx(name))
        {
            var v1 = await db.PayrollDefinitionVersions.FirstAsync(v => v.Id == v1Id);
            var v2 = await db.PayrollDefinitionVersions.FirstAsync(v => v.Id == v2Id);
            var def = await db.PayrollDefinitions.FirstAsync(d => d.Id == typeId);
            Assert.Equal(VersionStatus.Superseded, v1.Status);
            Assert.NotNull(v1.EffectiveTo);
            Assert.Equal(VersionStatus.Published, v2.Status);
            Assert.Equal(v2Id, def.CurrentVersionId);
        }
    }

    [Fact]
    public async Task Clone_creates_next_version_number_as_draft()
    {
        var name = $"db-{Guid.NewGuid()}";
        await using var db = Ctx(name);
        var svc = Svc(db);
        var typeId = await svc.CreateTypeAsync(new CreatePayrollTypeArgs("M", "M", "م", null), default);
        var v1 = await db.PayrollDefinitionVersions.FirstAsync(v => v.PayrollDefinitionId == typeId);
        await svc.PublishVersionAsync(typeId, v1.Id, default);
        var v2Id = await svc.CloneVersionAsync(typeId, v1.Id, default);
        var v2 = await db.PayrollDefinitionVersions.FirstAsync(v => v.Id == v2Id);
        Assert.Equal(2, v2.VersionNumber);
        Assert.Equal(VersionStatus.Draft, v2.Status);
    }

    [Fact]
    public async Task Cannot_edit_a_published_version()
    {
        var name = $"db-{Guid.NewGuid()}";
        await using var db = Ctx(name);
        var svc = Svc(db);
        var typeId = await svc.CreateTypeAsync(new CreatePayrollTypeArgs("M", "M", "م", null), default);
        var v1 = await db.PayrollDefinitionVersions.FirstAsync(v => v.PayrollDefinitionId == typeId);
        await svc.PublishVersionAsync(typeId, v1.Id, default);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.UpdateDraftVersionAsync(typeId, v1.Id, new UpdatePayrollVersionArgs { CutoffDay = 25 }, default));
    }
}
