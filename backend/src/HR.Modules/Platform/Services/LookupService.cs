using AutoMapper;
using HR.Domain.Engines.MasterData;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.MasterData;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Services;

/// <summary>
/// Centralised, tenant-scoped read service for master data lookups. This is the
/// single home for lookup logic so no module writes its own dropdown queries.
/// Tenant isolation and soft-delete are enforced by the global query filters on
/// <see cref="MasterDataItem"/>.
/// </summary>
public interface ILookupService
{
    /// <param name="objectType">Canonical name or kebab slug (e.g. "JobTitle" or "job-titles").</param>
    /// <returns>Ordered lookup items, or an empty list when the type is unknown.</returns>
    Task<List<LookupItemDto>> GetLookupAsync(string objectType, bool activeOnly = true, CancellationToken ct = default);
}

public class LookupService : ILookupService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public LookupService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<LookupItemDto>> GetLookupAsync(
        string objectType, bool activeOnly = true, CancellationToken ct = default)
    {
        // Accept either the canonical PascalCase name or the kebab slug used in URLs.
        var canonical = MasterDataObjectType.Normalize(objectType)
                        ?? MasterDataObjectType.FromSlug(objectType);
        if (canonical is null)
            return new List<LookupItemDto>();

        var query = _context.MasterDataItems
            .AsNoTracking()
            .Where(x => x.ObjectType == canonical);

        if (activeOnly)
            query = query.Where(x => x.IsActive);

        var entities = await query
            .OrderBy(x => x.SortOrder).ThenBy(x => x.NameEn)
            .ToListAsync(ct);

        return _mapper.Map<List<LookupItemDto>>(entities);
    }
}
