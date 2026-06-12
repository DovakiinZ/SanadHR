namespace HR.Modules.Platform.Services.Catalog;

// ──────────────────────────────────────────────────────────────────────────────
//  Object / Property Registry catalog models.
//  These are projected from the LIVE EF Core model so that any business object
//  (any entity carrying a TenantId) is automatically discoverable by the Widget
//  Builder with zero per-object code. Add an entity to the DbContext → it appears.
// ──────────────────────────────────────────────────────────────────────────────

/// <summary>A discoverable business object (maps to one table).</summary>
public class CatalogObjectDto
{
    public string Code { get; set; } = null!;        // canonical object code (CLR entity name, e.g. "Employee")
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string Module { get; set; } = null!;       // grouping for the builder
    public string? Icon { get; set; }
    public bool HasTenantScope { get; set; }
    public bool HasSoftDelete { get; set; }
    public bool HasDateCreated { get; set; }
    public int FieldCount { get; set; }
    public List<CatalogFieldDto> Fields { get; set; } = new();
}

/// <summary>A discoverable property of an object (maps to one column).</summary>
public class CatalogFieldDto
{
    public string Code { get; set; } = null!;         // canonical column name (e.g. "DepartmentId")
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string FieldType { get; set; } = null!;    // Text/Number/Decimal/Currency/Date/DateTime/Boolean/Reference/Enum...
    public bool IsMeasure { get; set; }               // numeric → can Sum/Avg/Min/Max
    public bool IsGroupable { get; set; }             // can be used on a GROUP BY axis
    public bool IsFilterable { get; set; }
    public bool IsDate { get; set; }
    public bool IsReference { get; set; }             // FK to another object
    public string? ReferenceObjectCode { get; set; }  // target object code when IsReference
    public List<EnumOptionDto>? Options { get; set; } // enum value labels when applicable
}

public class EnumOptionDto
{
    public int Value { get; set; }
    public string Label { get; set; } = null!;
}
