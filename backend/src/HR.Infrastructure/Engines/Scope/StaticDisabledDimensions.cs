using HR.Application.Engines.Scope;

namespace HR.Infrastructure.Engines.Scope;

/// <summary>Dimensions the product intends to support but whose owning module has not yet shipped a
/// provider. Surfaced (disabled) in the scope builder so the model is forward-compatible.</summary>
public static class StaticDisabledDimensions
{
    public static readonly IReadOnlyList<ScopeDimensionInfo> All = new[]
    {
        Disabled("Tag", "Tags", "الوسوم", "tags"),
        Disabled("CostCenter", "Cost Center", "مركز التكلفة", "cost-centers"),
        Disabled("Grade", "Grade", "الدرجة الوظيفية", "grades"),
        Disabled("Shift", "Shift", "الوردية", "shift-types"),
        Disabled("Project", "Project", "المشروع", null),
        Disabled("BusinessUnit", "Business Unit", "وحدة الأعمال", null),
        Disabled("Company", "Company", "الشركة", null),
    };

    private static ScopeDimensionInfo Disabled(string key, string en, string ar, string? slug) =>
        new(key, en, ar,
            new ScopeValueSource(slug is null ? ScopeValueSourceKind.Custom : ScopeValueSourceKind.MasterData, slug),
            IsAvailable: false,
            UnavailableNote: "Backing provider not yet available");
}
