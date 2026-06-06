using HR.Domain.Common;

namespace HR.Domain.Engines.MasterData;

/// <summary>
/// Generic, tenant-scoped master data record. A single table backs every
/// configurable business object type (Job Titles, Leave Types, Allowance Types,
/// Document Types, Tags, …) so structured values are stored as reusable objects
/// instead of free text. The <see cref="ObjectType"/> discriminator selects the
/// logical type; <see cref="MetadataJson"/> carries type-specific extra fields.
/// </summary>
public class MasterDataItem : TenantEntity
{
    /// <summary>Logical type discriminator, e.g. "JobTitle". See <see cref="MasterDataObjectType"/>.</summary>
    public string ObjectType { get; set; } = null!;

    /// <summary>Stable, tenant-unique code within an <see cref="ObjectType"/>, e.g. "SWE".</summary>
    public string Code { get; set; } = null!;

    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? Description { get; set; }

    /// <summary>Optional UI hint, hex colour e.g. "#FBBF24".</summary>
    public string? Color { get; set; }

    /// <summary>Optional UI hint, icon key e.g. "briefcase".</summary>
    public string? Icon { get; set; }

    public int SortOrder { get; set; }

    /// <summary>Seeded default that ships with a new tenant. System defaults cannot be hard-deleted.</summary>
    public bool IsSystemDefault { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Arbitrary type-specific metadata as a JSON object (jsonb in PostgreSQL).</summary>
    public string? MetadataJson { get; set; }
}
