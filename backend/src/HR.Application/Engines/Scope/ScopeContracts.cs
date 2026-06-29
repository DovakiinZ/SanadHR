namespace HR.Application.Engines.Scope;

/// <summary>Where the UI fetches selectable values for a dimension.</summary>
public enum ScopeValueSourceKind { MasterData, StaticEnum, Custom }

public sealed record ScopeValueSource(ScopeValueSourceKind Kind, string? Reference);
// Reference: master-data object-type slug (MasterData), or an opaque key (StaticEnum/Custom).

/// <summary>Describes a selection dimension to the scope builder UI and the engine.</summary>
public sealed record ScopeDimensionInfo(
    string Key,
    string NameEn,
    string NameAr,
    ScopeValueSource ValueSource,
    bool IsAvailable,
    string? UnavailableNote);

public sealed record ScopeCriterion(string Dimension, IReadOnlyList<Guid> ValueIds);

/// <summary>Deserialized from PayrollDefinitionVersion.SelectionScopeJson.</summary>
public sealed record SelectionScope(
    string Mode,                                   // "All" | "Criteria"
    IReadOnlyList<ScopeCriterion> Include,
    IReadOnlyList<ScopeCriterion> Exclude,
    IReadOnlyList<Guid> IncludeEmployeeIds,
    IReadOnlyList<Guid> ExcludeEmployeeIds)
{
    public static SelectionScope All() =>
        new("All", Array.Empty<ScopeCriterion>(), Array.Empty<ScopeCriterion>(),
            Array.Empty<Guid>(), Array.Empty<Guid>());
}

public sealed record ScopeExclusion(Guid EmployeeId, string DimensionKey);

public sealed record ScopeResolution(
    IReadOnlyCollection<Guid> IncludedEmployeeIds,
    IReadOnlyCollection<ScopeExclusion> ExcludedByScope,
    IReadOnlyCollection<string> Warnings);
