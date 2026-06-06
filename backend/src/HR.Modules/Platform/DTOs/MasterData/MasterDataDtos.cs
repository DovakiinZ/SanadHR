namespace HR.Modules.Platform.DTOs.MasterData;

/// <summary>Full management view of a master data item.</summary>
public record MasterDataItemDto
{
    public Guid Id { get; init; }
    public string ObjectType { get; init; } = null!;
    public string Code { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string? Description { get; init; }
    public string? Color { get; init; }
    public string? Icon { get; init; }
    public int SortOrder { get; init; }
    public bool IsSystemDefault { get; init; }
    public bool IsActive { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public string? UpdatedBy { get; init; }
}

/// <summary>
/// Compact, consumption-oriented shape returned by the system-wide lookup API
/// (GET /api/lookups/{objectType}). Matches the response contract in the spec.
/// </summary>
public record LookupItemDto
{
    public Guid Id { get; init; }
    public string ObjectType { get; init; } = null!;
    public string Code { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    /// <summary>Display label (NameAr fallback NameEn).</summary>
    public string Label { get; init; } = null!;
    public string? Description { get; init; }
    public string? Color { get; init; }
    public string? Icon { get; init; }
    public bool IsActive { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>One object type plus its live item count for the master data type catalogue.</summary>
public record MasterDataObjectTypeDto
{
    public string ObjectType { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public int Count { get; init; }
}

/// <summary>Per-module breakdown of where a master data item is referenced.</summary>
public record MasterDataUsageEntryDto
{
    public string Module { get; init; } = null!;
    public int Count { get; init; }
}

/// <summary>Usage report used before deactivate/delete/merge decisions.</summary>
public record MasterDataUsageDto
{
    public Guid ItemId { get; init; }
    public string ObjectType { get; init; } = null!;
    public int TotalUsageCount { get; init; }
    public List<MasterDataUsageEntryDto> Usages { get; init; } = new();
}

/// <summary>Outcome of seeding per-tenant defaults + Object Registry registration.</summary>
public record SeedMasterDataResultDto
{
    public int ItemsSeeded { get; init; }
    public int TypesRegistered { get; init; }
}
