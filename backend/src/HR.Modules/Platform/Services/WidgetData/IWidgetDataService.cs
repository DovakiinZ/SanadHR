namespace HR.Modules.Platform.Services.WidgetData;

/// <summary>
/// Executes a widget query spec against the live data as dynamic, parameterized SQL.
/// Every identifier is validated against the Object Catalog (whitelist) before use, and
/// every literal is a bound parameter — so the engine is injection-safe while remaining
/// fully object-driven (no per-object code, no hardcoded tables).
/// </summary>
public interface IWidgetDataService
{
    /// <summary>Run an aggregation and return a scalar (KPI) or series (chart) result.</summary>
    Task<WidgetDataResult> ExecuteAsync(WidgetQuerySpec spec, IReadOnlyList<WidgetFilterSpec>? dashboardFilters, CancellationToken ct);

    /// <summary>Run a saved widget by id (reads its Configuration spec), merging dashboard filters.</summary>
    Task<WidgetDataResult> ExecuteWidgetAsync(Guid widgetId, IReadOnlyList<WidgetFilterSpec>? dashboardFilters, CancellationToken ct);

    /// <summary>Drill-down: the detail rows behind a widget, optionally for one clicked segment.</summary>
    Task<WidgetDataResult> GetRowsAsync(WidgetQuerySpec spec, string? segmentKey, IReadOnlyList<WidgetFilterSpec>? dashboardFilters, int page, int pageSize, CancellationToken ct);
}
