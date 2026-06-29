using System.Text.Json;

namespace HR.Application.Engines.Scope;

/// <summary>Tolerant parse/serialize for PayrollDefinitionVersion.SelectionScopeJson. Any malformed or
/// missing config degrades to mode = All (never throws), so a bad config can never empty a payroll silently.</summary>
public static class SelectionScopeJson
{
    private static readonly JsonSerializerOptions Opts = new() { PropertyNameCaseInsensitive = true };

    public static SelectionScope Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return SelectionScope.All();
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return SelectionScope.All();

            var mode = root.TryGetProperty("mode", out var m) && m.ValueKind == JsonValueKind.String
                ? m.GetString()! : "All";
            return new SelectionScope(
                mode,
                ReadCriteria(root, "include"),
                ReadCriteria(root, "exclude"),
                ReadGuidArray(root, "includeEmployeeIds"),
                ReadGuidArray(root, "excludeEmployeeIds"));
        }
        catch (JsonException) { return SelectionScope.All(); }
    }

    public static string Serialize(SelectionScope scope) => JsonSerializer.Serialize(new
    {
        mode = scope.Mode,
        include = scope.Include.Select(c => new { dimension = c.Dimension, valueIds = c.ValueIds }),
        exclude = scope.Exclude.Select(c => new { dimension = c.Dimension, valueIds = c.ValueIds }),
        includeEmployeeIds = scope.IncludeEmployeeIds,
        excludeEmployeeIds = scope.ExcludeEmployeeIds,
    }, Opts);

    private static List<ScopeCriterion> ReadCriteria(JsonElement root, string prop)
    {
        var list = new List<ScopeCriterion>();
        if (root.TryGetProperty(prop, out var arr) && arr.ValueKind == JsonValueKind.Array)
            foreach (var el in arr.EnumerateArray())
            {
                if (el.ValueKind != JsonValueKind.Object) continue;
                var dim = el.TryGetProperty("dimension", out var d) && d.ValueKind == JsonValueKind.String
                    ? d.GetString() : null;
                if (string.IsNullOrWhiteSpace(dim)) continue;
                list.Add(new ScopeCriterion(dim!, ReadGuidArray(el, "valueIds")));
            }
        return list;
    }

    private static List<Guid> ReadGuidArray(JsonElement obj, string prop)
    {
        var ids = new List<Guid>();
        if (obj.TryGetProperty(prop, out var arr) && arr.ValueKind == JsonValueKind.Array)
            foreach (var el in arr.EnumerateArray())
                if (el.ValueKind == JsonValueKind.String && Guid.TryParse(el.GetString(), out var g))
                    ids.Add(g);
        return ids;
    }
}
