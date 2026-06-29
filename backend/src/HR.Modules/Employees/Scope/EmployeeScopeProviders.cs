using HR.Application.Common.Interfaces;
using HR.Application.Engines.Scope;
using HR.Domain.Engines.MasterData;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using HR.Modules.Employees.Entities;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Employees.Scope;

/// <summary>Base "all active employees" population (mode = All): excludes Terminated/Resigned.</summary>
public sealed class ActiveEmployeePopulationProvider : IBasePopulationProvider
{
    private readonly ApplicationDbContext _db;
    public ActiveEmployeePopulationProvider(ApplicationDbContext db) => _db = db;
    public async Task<ISet<Guid>> ResolveAllAsync(CancellationToken ct) =>
        (await _db.Employees.AsNoTracking()
            .Where(e => e.Status != EmployeeStatus.Terminated && e.Status != EmployeeStatus.Resigned)
            .Select(e => e.Id).ToListAsync(ct)).ToHashSet();
}

public abstract class EmployeeColumnDimension : IScopeDimensionProvider
{
    protected readonly ApplicationDbContext Db;
    protected EmployeeColumnDimension(ApplicationDbContext db) => Db = db;
    public abstract string DimensionKey { get; }
    public abstract ScopeDimensionInfo Info { get; }
    public abstract Task<ISet<Guid>> ResolveEmployeesAsync(IReadOnlyCollection<Guid> valueIds, CancellationToken ct);

    protected static ScopeDimensionInfo MasterDataDim(string key, string en, string ar, string slug) =>
        new(key, en, ar, new ScopeValueSource(ScopeValueSourceKind.MasterData, slug), true, null);

    protected async Task<ISet<Guid>> ByColumn(
        IReadOnlyCollection<Guid> valueIds,
        System.Linq.Expressions.Expression<Func<Employee, bool>> predicate, CancellationToken ct)
    {
        if (valueIds.Count == 0) return new HashSet<Guid>();
        return (await Db.Employees.AsNoTracking().Where(predicate).Select(e => e.Id).ToListAsync(ct)).ToHashSet();
    }
}

public sealed class DepartmentScopeProvider : EmployeeColumnDimension
{
    public DepartmentScopeProvider(ApplicationDbContext db) : base(db) { }
    public override string DimensionKey => "Department";
    public override ScopeDimensionInfo Info => MasterDataDim(DimensionKey, "Department", "القسم", "departments");
    public override Task<ISet<Guid>> ResolveEmployeesAsync(IReadOnlyCollection<Guid> v, CancellationToken ct) =>
        ByColumn(v, e => e.DepartmentId != null && v.Contains(e.DepartmentId.Value), ct);
}

public sealed class BranchScopeProvider : EmployeeColumnDimension
{
    public BranchScopeProvider(ApplicationDbContext db) : base(db) { }
    public override string DimensionKey => "Branch";
    public override ScopeDimensionInfo Info => MasterDataDim(DimensionKey, "Branch", "الفرع", "branches");
    public override Task<ISet<Guid>> ResolveEmployeesAsync(IReadOnlyCollection<Guid> v, CancellationToken ct) =>
        ByColumn(v, e => e.BranchId != null && v.Contains(e.BranchId.Value), ct);
}

public sealed class JobTitleScopeProvider : EmployeeColumnDimension
{
    public JobTitleScopeProvider(ApplicationDbContext db) : base(db) { }
    public override string DimensionKey => "JobTitle";
    public override ScopeDimensionInfo Info => MasterDataDim(DimensionKey, "Job Title", "المسمى الوظيفي", "job-titles");
    public override Task<ISet<Guid>> ResolveEmployeesAsync(IReadOnlyCollection<Guid> v, CancellationToken ct) =>
        ByColumn(v, e => e.JobTitleId != null && v.Contains(e.JobTitleId.Value), ct);
}

public sealed class EmploymentTypeScopeProvider : EmployeeColumnDimension
{
    public EmploymentTypeScopeProvider(ApplicationDbContext db) : base(db) { }
    public override string DimensionKey => "EmploymentType";
    public override ScopeDimensionInfo Info => MasterDataDim(DimensionKey, "Employment Type", "نوع التوظيف", "employment-types");
    public override Task<ISet<Guid>> ResolveEmployeesAsync(IReadOnlyCollection<Guid> v, CancellationToken ct) =>
        ByColumn(v, e => e.EmploymentTypeId != null && v.Contains(e.EmploymentTypeId.Value), ct);
}

public sealed class ContractTypeScopeProvider : EmployeeColumnDimension
{
    public ContractTypeScopeProvider(ApplicationDbContext db) : base(db) { }
    public override string DimensionKey => "ContractType";
    public override ScopeDimensionInfo Info => MasterDataDim(DimensionKey, "Contract Type", "نوع العقد", "contract-types");
    public override Task<ISet<Guid>> ResolveEmployeesAsync(IReadOnlyCollection<Guid> v, CancellationToken ct) =>
        ByColumn(v, e => e.ContractTypeId != null && v.Contains(e.ContractTypeId.Value), ct);
}

public sealed class PaymentMethodScopeProvider : EmployeeColumnDimension
{
    public PaymentMethodScopeProvider(ApplicationDbContext db) : base(db) { }
    public override string DimensionKey => "PaymentMethod";
    public override ScopeDimensionInfo Info => MasterDataDim(DimensionKey, "Payment Method", "طريقة الدفع", "payment-methods");
    public override Task<ISet<Guid>> ResolveEmployeesAsync(IReadOnlyCollection<Guid> v, CancellationToken ct) =>
        ByColumn(v, e => e.PaymentMethodId != null && v.Contains(e.PaymentMethodId.Value), ct);
}

public sealed class NationalityScopeProvider : EmployeeColumnDimension
{
    public NationalityScopeProvider(ApplicationDbContext db) : base(db) { }
    public override string DimensionKey => "Nationality";
    public override ScopeDimensionInfo Info => MasterDataDim(DimensionKey, "Nationality", "الجنسية", "nationalities");
    public override Task<ISet<Guid>> ResolveEmployeesAsync(IReadOnlyCollection<Guid> v, CancellationToken ct) =>
        ByColumn(v, e => e.NationalityId != null && v.Contains(e.NationalityId.Value), ct);
}

/// <summary>Status is an enum, not master-data: each EmployeeStatus maps to a deterministic GUID so the
/// engine stays GUID-based. The UI fetches the value list via the StaticEnum value source key "EmployeeStatus".</summary>
public sealed class StatusScopeProvider : EmployeeColumnDimension
{
    public StatusScopeProvider(ApplicationDbContext db) : base(db) { }
    public override string DimensionKey => "Status";
    public override ScopeDimensionInfo Info =>
        new(DimensionKey, "Employment Status", "حالة الموظف",
            new ScopeValueSource(ScopeValueSourceKind.StaticEnum, "EmployeeStatus"), true, null);

    /// <summary>Stable GUID for an EmployeeStatus value (namespace-prefixed by the enum int).</summary>
    public static Guid StatusId(EmployeeStatus s) =>
        new($"00000000-0000-0000-0000-0000000000{(int)s:D2}");

    public override async Task<ISet<Guid>> ResolveEmployeesAsync(IReadOnlyCollection<Guid> v, CancellationToken ct)
    {
        if (v.Count == 0) return new HashSet<Guid>();
        var statuses = Enum.GetValues<EmployeeStatus>().Where(s => v.Contains(StatusId(s))).ToList();
        if (statuses.Count == 0) return new HashSet<Guid>();
        return (await Db.Employees.AsNoTracking().Where(e => statuses.Contains(e.Status))
            .Select(e => e.Id).ToListAsync(ct)).ToHashSet();
    }
}
