using HR.Modules.Platform.Services.Catalog;

namespace HR.Modules.Platform.Services.WidgetData;

/// <summary>
/// "Describe what you want" → a widget spec. A self-contained, catalog-driven parser
/// (Arabic + English) — no external LLM required. It resolves the object, aggregation,
/// measure, group-by dimension, time-trend and visualization from the live catalog, so
/// it automatically understands any object the registry exposes.
/// </summary>
public interface IWidgetSuggestionService
{
    WidgetSuggestion Suggest(string prompt);
}

public sealed class WidgetSuggestion
{
    public WidgetQuerySpec Spec { get; set; } = null!;
    public string Visualization { get; set; } = "KpiCard";
    public string TitleAr { get; set; } = "";
    public string Explanation { get; set; } = "";
}

public sealed class WidgetSuggestionService : IWidgetSuggestionService
{
    private readonly IObjectCatalogService _catalog;
    public WidgetSuggestionService(IObjectCatalogService catalog) => _catalog = catalog;

    // Arabic/English synonyms that hint at a specific object.
    private static readonly (string token, string objectCode, string? measure)[] ObjectHints =
    {
        ("payroll", "Employee", "BasicSalary"), ("رواتب", "Employee", "BasicSalary"),
        ("راتب", "Employee", "BasicSalary"), ("salary", "Employee", "BasicSalary"),
        ("موظف", "Employee", null), ("employee", "Employee", null), ("staff", "Employee", null),
        ("قسم", "Department", null), ("ادارة", "Department", null), ("إدارة", "Department", null), ("department", "Department", null),
        ("فرع", "Branch", null), ("فروع", "Branch", null), ("branch", "Branch", null),
        ("مهمة", "HrTask", null), ("مهام", "HrTask", null), ("task", "HrTask", null),
        ("طلب", "FormSubmission", null), ("طلبات", "FormSubmission", null), ("request", "FormSubmission", null),
    };

    public WidgetSuggestion Suggest(string prompt)
    {
        var p = (prompt ?? "").Trim();
        var lower = p.ToLowerInvariant();
        var catalog = _catalog.GetCatalog();

        // 1) Object — synonyms first, then any catalog name/code token.
        CatalogObjectDto? obj = null;
        string? hintMeasure = null;
        foreach (var (token, code, measure) in ObjectHints)
            if (lower.Contains(token)) { obj ??= catalog.FirstOrDefault(o => o.Code == code); if (obj is not null) { hintMeasure = measure; break; } }
        obj ??= catalog.FirstOrDefault(o => p.Contains(o.NameAr) || ContainsWord(lower, o.NameEn) || lower.Contains(o.Code.ToLowerInvariant()));
        obj ??= catalog.FirstOrDefault(o => o.Code == "Employee") ?? catalog.FirstOrDefault();

        if (obj is null)
            return Fallback(p);

        // 2) Aggregation.
        string aggregation =
            ContainsAny(lower, "average", "avg", "mean") || p.Contains("متوسط") ? "Average" :
            ContainsAny(lower, "sum", "total", "cost", "payroll", "spend", "expense", "budget") || p.Contains("مجموع") || p.Contains("اجمالي") || p.Contains("إجمالي") || p.Contains("تكلفة") ? "Sum" :
            ContainsAny(lower, "percentage", "percent", "rate") || p.Contains("نسبة") ? "Percentage" :
            "Count";

        // 3) Measure field (for Sum/Average).
        string? measureField = null;
        if (aggregation is "Sum" or "Average")
        {
            measureField = FindField(obj, p, lower, onlyMeasures: true)?.Code
                ?? (hintMeasure is not null && obj.Fields.Any(f => f.Code == hintMeasure) ? hintMeasure : null)
                ?? obj.Fields.FirstOrDefault(f => f.IsMeasure)?.Code;
            if (measureField is null) aggregation = "Count";
        }

        // 4) Group-by dimension + time-trend.
        string? groupBy = null, granularity = null;
        var isTrend = ContainsAny(lower, "trend", "over time", "by month", "monthly", "by day", "by year") ||
                      p.Contains("اتجاه") || p.Contains("شهري") || p.Contains("بمرور") || p.Contains("خلال") || p.Contains("تطور");
        if (isTrend)
        {
            var dateField = FindField(obj, p, lower, onlyDates: true)
                ?? obj.Fields.FirstOrDefault(f => f.Code == "HireDate")
                ?? obj.Fields.FirstOrDefault(f => f.IsDate);
            if (dateField is not null)
            {
                groupBy = dateField.Code;
                granularity = ContainsAny(lower, "by day", "daily") || p.Contains("يومي") ? "day"
                    : ContainsAny(lower, "by year", "yearly") || p.Contains("سنوي") ? "year" : "month";
            }
        }
        if (groupBy is null && aggregation != "Percentage")
        {
            // Prefer the phrase after "by/per/حسب/لكل" so we match the dimension, not the
            // object's own name (e.g. "employees by gender" → Gender, not EmployeeNumber).
            var tail = ExtractDimension(p, lower);
            var dim = tail is not null
                ? FindField(obj, tail, tail.ToLowerInvariant(), onlyGroupable: true)
                : FindField(obj, p, lower, onlyGroupable: true);
            if (dim is not null) groupBy = dim.Code;
        }

        // 5) "last N days" filter on a date column.
        var filters = new List<WidgetFilterSpec>();
        var days = ExtractLastNDays(lower, p);
        if (days is int n)
        {
            var dateCol = obj.Fields.FirstOrDefault(f => f.Code == "CreatedAt")?.Code
                ?? obj.Fields.FirstOrDefault(f => f.IsDate)?.Code;
            if (dateCol is not null) filters.Add(new WidgetFilterSpec { Field = dateCol, Operator = "last_n_days", Value = n.ToString() });
        }

        // 6) Visualization.
        string visualization;
        if (groupBy is null) visualization = aggregation == "Percentage" ? "Gauge" : "KpiCard";
        else if (granularity is not null) visualization = "LineChart";
        else
        {
            var gf = obj.Fields.FirstOrDefault(f => f.Code == groupBy);
            visualization = gf is not null && IsCategorical(gf) ? "DonutChart" : "BarChart";
        }

        var spec = new WidgetQuerySpec
        {
            ObjectCode = obj.Code,
            Aggregation = aggregation,
            AggregationField = measureField,
            GroupByField = groupBy,
            DateGranularity = granularity,
            Visualization = visualization,
            Limit = granularity is not null ? 24 : 12,
            Filters = filters,
        };

        return new WidgetSuggestion
        {
            Spec = spec,
            Visualization = visualization,
            TitleAr = string.IsNullOrWhiteSpace(p) ? obj.NameAr : p,
            Explanation = $"{obj.NameAr} · {aggregation}{(measureField is not null ? $" ({measureField})" : "")}{(groupBy is not null ? $" · {groupBy}" : "")}",
        };
    }

    private static WidgetSuggestion Fallback(string p) => new()
    {
        Spec = new WidgetQuerySpec { ObjectCode = "Employee", Aggregation = "Count", Filters = new() },
        Visualization = "KpiCard",
        TitleAr = string.IsNullOrWhiteSpace(p) ? "مؤشر" : p,
    };

    private static CatalogFieldDto? FindField(CatalogObjectDto obj, string prompt, string lower, bool onlyMeasures = false, bool onlyDates = false, bool onlyGroupable = false)
    {
        IEnumerable<CatalogFieldDto> fields = obj.Fields;
        if (onlyMeasures) fields = fields.Where(f => f.IsMeasure);
        if (onlyDates) fields = fields.Where(f => f.IsDate);
        if (onlyGroupable) fields = fields.Where(f => f.IsGroupable);
        // Longest Arabic/English name match wins (most specific).
        return fields
            .Select(f => (f, score: FieldScore(f, prompt, lower)))
            .Where(x => x.score > 0)
            .OrderByDescending(x => x.score)
            .Select(x => x.f)
            .FirstOrDefault();
    }

    private static int FieldScore(CatalogFieldDto f, string prompt, string lower)
    {
        // Whole-word match so e.g. "الجنس" (Gender) does not falsely match inside "الجنسية".
        if (!string.IsNullOrWhiteSpace(f.NameAr) && ContainsWhole(prompt, f.NameAr)) return f.NameAr.Length + 5;
        if (ContainsWord(lower, f.NameEn)) return f.NameEn.Length;
        var bare = f.Code.EndsWith("Id", StringComparison.Ordinal) ? f.Code[..^2] : f.Code;
        if (bare.Length >= 3 && lower.Contains(bare.ToLowerInvariant())) return bare.Length;
        return 0;
    }

    private static bool IsCategorical(CatalogFieldDto f)
        => f.FieldType is "Boolean" or "Enum" || f.Code is "Gender" or "Status" or "Priority" or "ObjectType";

    private static int? ExtractLastNDays(string lower, string p)
    {
        foreach (var m in System.Text.RegularExpressions.Regex.Matches(lower, @"last\s+(\d+)\s*day").Cast<System.Text.RegularExpressions.Match>())
            if (int.TryParse(m.Groups[1].Value, out var n)) return n;
        foreach (var m in System.Text.RegularExpressions.Regex.Matches(p, @"آخر\s+(\d+)\s*يوم").Cast<System.Text.RegularExpressions.Match>())
            if (int.TryParse(m.Groups[1].Value, out var n)) return n;
        if (lower.Contains("this year") || p.Contains("هذا العام")) return 365;
        return null;
    }

    // Returns the dimension phrase after a "by/per/حسب/لكل" marker, or null.
    private static string? ExtractDimension(string p, string lower)
    {
        foreach (var marker in new[] { " by ", " per ", " grouped by ", " حسب ", " لكل ", " بحسب " })
        {
            var idx = lower.LastIndexOf(marker, StringComparison.Ordinal);
            if (idx >= 0)
            {
                var tail = p.Substring(idx + marker.Length).Trim();
                if (tail.Length > 0) return tail;
            }
        }
        return null;
    }

    // Substring match that requires word boundaries on both sides (Arabic-aware).
    private static bool ContainsWhole(string haystack, string needle)
    {
        var idx = 0;
        while ((idx = haystack.IndexOf(needle, idx, StringComparison.Ordinal)) >= 0)
        {
            var before = idx == 0 || !char.IsLetterOrDigit(haystack[idx - 1]);
            var endPos = idx + needle.Length;
            var after = endPos >= haystack.Length || !char.IsLetterOrDigit(haystack[endPos]);
            if (before && after) return true;
            idx++;
        }
        return false;
    }

    private static bool ContainsAny(string s, params string[] tokens) => tokens.Any(s.Contains);
    private static bool ContainsWord(string lowerHaystack, string phrase)
    {
        foreach (var w in phrase.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var word = w.TrimEnd('s'); // crude singularization
            if (word.Length >= 3 && lowerHaystack.Contains(word)) return true;
        }
        return false;
    }
}
