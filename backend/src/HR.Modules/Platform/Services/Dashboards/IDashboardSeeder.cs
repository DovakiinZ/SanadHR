namespace HR.Modules.Platform.Services.Dashboards;

public sealed record DashboardTemplateInfo(string Key, string NameAr, string NameEn, string Description, string Icon);

public interface IDashboardSeeder
{
    /// <summary>The ready-made templates this engine can provision.</summary>
    IReadOnlyList<DashboardTemplateInfo> AvailableTemplates();

    /// <summary>
    /// Idempotently provisions a ready-made dashboard for the current tenant. Only widgets
    /// whose object and fields exist in the catalog are created, so it adapts to the model.
    /// Returns the dashboard id. Unknown keys fall back to the executive template.
    /// </summary>
    Task<Guid> SeedTemplateAsync(string key, CancellationToken ct);

    /// <summary>Convenience: provision the default Executive dashboard.</summary>
    Task<Guid> SeedExecutiveAsync(CancellationToken ct);
}
