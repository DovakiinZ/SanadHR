namespace HR.Modules.Platform.Services.Catalog;

/// <summary>
/// Projects the live EF Core model into the Object / Property Registry catalog and
/// resolves object codes to the physical table/column metadata the Widget Data engine
/// needs. This is the single source of truth that makes every business object
/// automatically available to the dashboard platform — no per-object code.
/// </summary>
public interface IObjectCatalogService
{
    /// <summary>All discoverable business objects with their fields (for the builder).</summary>
    IReadOnlyList<CatalogObjectDto> GetCatalog();

    /// <summary>A single object's catalog entry, or null when the code is unknown.</summary>
    CatalogObjectDto? GetObject(string objectCode);

    /// <summary>Resolve an object code to physical metadata, or null when unknown / not discoverable.</summary>
    ResolvedObject? Resolve(string objectCode);
}

// ── Internal resolution structures used by the data engine ────────────────────

public enum FieldKind { Text, Number, Decimal, Currency, Percentage, Date, DateTime, Boolean, Reference, Enum, Guid }

public sealed class ResolvedObject
{
    public string Code { get; init; } = null!;
    public string TableName { get; init; } = null!;
    public string? Schema { get; init; }
    public bool HasTenant { get; init; }
    public bool HasSoftDelete { get; init; }
    public string KeyColumn { get; init; } = "Id";
    public IReadOnlyDictionary<string, ResolvedField> Fields { get; init; } =
        new Dictionary<string, ResolvedField>(StringComparer.OrdinalIgnoreCase);

    public ResolvedField? Field(string? code)
        => code is not null && Fields.TryGetValue(code, out var f) ? f : null;
}

public sealed class ResolvedField
{
    public string Code { get; init; } = null!;
    public string ColumnName { get; init; } = null!;
    public Type ClrType { get; init; } = typeof(string);
    public FieldKind Kind { get; init; }
    public bool IsReference => Reference is not null;
    public ResolvedReference? Reference { get; init; }
}

public sealed class ResolvedReference
{
    public string PrincipalTable { get; init; } = null!;
    public string? PrincipalSchema { get; init; }
    public string PrincipalKeyColumn { get; init; } = "Id";
    public string? DisplayColumn { get; init; }
    public string[]? DisplayConcatColumns { get; init; }
}
