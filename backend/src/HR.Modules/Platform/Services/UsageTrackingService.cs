using HR.Domain.Engines.MasterData;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.MasterData;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Services;

/// <summary>
/// Resolves where a master data item is referenced across the system, and
/// reassigns references when two items are merged.
///
/// Design: consumers register a <see cref="MasterDataReference"/> (module label +
/// a delegate that counts rows referencing a given item id, and an optional
/// delegate that repoints them to another id). This is the seam later phases plug
/// into as modules migrate from free text to FK ids (e.g. Employee.JobTitleId).
/// Until then the registry is empty, so usage is reported as 0 — honest, not faked.
/// </summary>
public interface IUsageTrackingService
{
    Task<MasterDataUsageDto> GetUsageAsync(string objectType, Guid itemId, CancellationToken ct = default);
    Task<int> GetTotalUsageAsync(string objectType, Guid itemId, CancellationToken ct = default);
    /// <summary>Repoints every reference from <paramref name="sourceId"/> to <paramref name="targetId"/>. Returns rows changed.</summary>
    Task<int> ReassignReferencesAsync(string objectType, Guid sourceId, Guid targetId, CancellationToken ct = default);
}

/// <summary>A single place where a master data <see cref="ObjectType"/> is consumed.</summary>
public record MasterDataReference(
    string ObjectType,
    string Module,
    Func<ApplicationDbContext, Guid, CancellationToken, Task<int>> Count,
    Func<ApplicationDbContext, Guid, Guid, CancellationToken, Task<int>>? Reassign = null);

public class UsageTrackingService : IUsageTrackingService
{
    private readonly ApplicationDbContext _context;

    public UsageTrackingService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Reference registry — add entries here as modules adopt FK ids. Example
    /// (once Employee.JobTitleId exists):
    /// <code>
    /// new MasterDataReference(MasterDataObjectType.JobTitle, "Employees",
    ///     (db, id, ct) => db.Employees.CountAsync(e => e.JobTitleId == id, ct),
    ///     (db, src, dst, ct) => db.Employees.Where(e => e.JobTitleId == src)
    ///         .ExecuteUpdateAsync(s => s.SetProperty(e => e.JobTitleId, dst), ct))
    /// </code>
    /// </summary>
    private static readonly IReadOnlyList<MasterDataReference> References = new[]
    {
        new MasterDataReference(MasterDataObjectType.JobTitle, "الموظفون",
            (db, id, ct) => db.Employees.CountAsync(e => e.JobTitleId == id, ct),
            (db, src, dst, ct) => db.Employees.Where(e => e.JobTitleId == src)
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.JobTitleId, dst), ct)),

        new MasterDataReference(MasterDataObjectType.Nationality, "الموظفون",
            (db, id, ct) => db.Employees.CountAsync(e => e.NationalityId == id, ct),
            (db, src, dst, ct) => db.Employees.Where(e => e.NationalityId == src)
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.NationalityId, dst), ct)),

        new MasterDataReference(MasterDataObjectType.ContractType, "الموظفون",
            (db, id, ct) => db.Employees.CountAsync(e => e.ContractTypeId == id, ct),
            (db, src, dst, ct) => db.Employees.Where(e => e.ContractTypeId == src)
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.ContractTypeId, dst), ct)),
    };

    private static IEnumerable<MasterDataReference> For(string objectType) =>
        References.Where(r => r.ObjectType == objectType);

    public async Task<MasterDataUsageDto> GetUsageAsync(string objectType, Guid itemId, CancellationToken ct = default)
    {
        var canonical = MasterDataObjectType.Normalize(objectType) ?? objectType;
        var usages = new List<MasterDataUsageEntryDto>();
        var total = 0;

        foreach (var reference in For(canonical))
        {
            var count = await reference.Count(_context, itemId, ct);
            if (count > 0)
            {
                usages.Add(new MasterDataUsageEntryDto { Module = reference.Module, Count = count });
                total += count;
            }
        }

        return new MasterDataUsageDto
        {
            ItemId = itemId,
            ObjectType = canonical,
            TotalUsageCount = total,
            Usages = usages
        };
    }

    public async Task<int> GetTotalUsageAsync(string objectType, Guid itemId, CancellationToken ct = default)
    {
        var canonical = MasterDataObjectType.Normalize(objectType) ?? objectType;
        var total = 0;
        foreach (var reference in For(canonical))
            total += await reference.Count(_context, itemId, ct);
        return total;
    }

    public async Task<int> ReassignReferencesAsync(
        string objectType, Guid sourceId, Guid targetId, CancellationToken ct = default)
    {
        var canonical = MasterDataObjectType.Normalize(objectType) ?? objectType;
        var changed = 0;
        foreach (var reference in For(canonical))
        {
            if (reference.Reassign is not null)
                changed += await reference.Reassign(_context, sourceId, targetId, ct);
        }
        return changed;
    }
}
