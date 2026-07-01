using System.Text.Json;

namespace HR.Infrastructure.Engines.Finance;

/// <summary>Minimal reader for the version's CalcSettingsJson toggles. Absent/invalid → feature enabled.</summary>
public static class PayrollCalcSettings
{
    public static bool IncludeAttendanceDeductions(string? calcSettingsJson)
    {
        if (string.IsNullOrWhiteSpace(calcSettingsJson)) return true;
        try
        {
            using var doc = JsonDocument.Parse(calcSettingsJson);
            if (doc.RootElement.ValueKind == JsonValueKind.Object
                && doc.RootElement.TryGetProperty("includeAttendanceDeductions", out var v)
                && v.ValueKind == JsonValueKind.False)
                return false;
        }
        catch (JsonException) { /* malformed → default enabled */ }
        return true;
    }
}
