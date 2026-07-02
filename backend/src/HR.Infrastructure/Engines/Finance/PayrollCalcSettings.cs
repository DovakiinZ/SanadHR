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

    public readonly record struct AttendanceRates(decimal Absence, decimal Late, decimal Shortage, decimal Overtime);

    private static readonly AttendanceRates Defaults = new(1.0m, 1.0m, 1.0m, 1.5m);

    /// <summary>Reads attendanceRates multipliers; any missing/invalid value falls back to its statutory default.</summary>
    public static AttendanceRates Rates(string? calcSettingsJson)
    {
        if (string.IsNullOrWhiteSpace(calcSettingsJson)) return Defaults;
        try
        {
            using var doc = JsonDocument.Parse(calcSettingsJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object
                || !doc.RootElement.TryGetProperty("attendanceRates", out var r)
                || r.ValueKind != JsonValueKind.Object)
                return Defaults;
            return new AttendanceRates(
                Mult(r, "absenceMultiplier", Defaults.Absence),
                Mult(r, "lateMultiplier", Defaults.Late),
                Mult(r, "shortageMultiplier", Defaults.Shortage),
                Mult(r, "overtimeMultiplier", Defaults.Overtime));
        }
        catch (JsonException) { return Defaults; }
    }

    /// <summary>Whether overtime additions are materialized. Opt-in: default false.</summary>
    public static bool IncludeOvertime(string? calcSettingsJson)
    {
        if (string.IsNullOrWhiteSpace(calcSettingsJson)) return false;
        try
        {
            using var doc = JsonDocument.Parse(calcSettingsJson);
            return doc.RootElement.ValueKind == JsonValueKind.Object
                && doc.RootElement.TryGetProperty("includeOvertime", out var v)
                && v.ValueKind == JsonValueKind.True;
        }
        catch (JsonException) { return false; }
    }

    private static decimal Mult(JsonElement obj, string key, decimal fallback)
        => obj.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.Number && v.TryGetDecimal(out var d)
            ? d : fallback;
}
