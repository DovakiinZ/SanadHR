namespace HR.Modules.Platform.Controllers;

public sealed class TokenGroupDto
{
    public string Group { get; set; } = null!;
    public List<TokenItemDto> Tokens { get; set; } = new();
}
public sealed class TokenItemDto
{
    public string Token { get; set; } = null!;
    public string Label { get; set; } = null!;
}
public sealed class PreviewHtmlRequest { public string? Body { get; set; } }

/// <summary>The canonical token set available to document templates + sample values for preview.</summary>
public static class DocumentTokens
{
    public static readonly List<TokenGroupDto> Catalog = new()
    {
        new TokenGroupDto { Group = "الموظف", Tokens = new()
        {
            new() { Token = "{{Employee.FullName}}", Label = "اسم الموظف" },
            new() { Token = "{{Employee.EmployeeNumber}}", Label = "الرقم الوظيفي" },
            new() { Token = "{{Employee.Department}}", Label = "الإدارة" },
            new() { Token = "{{Employee.JobTitle}}", Label = "المسمى الوظيفي" },
            new() { Token = "{{Employee.Manager}}", Label = "المدير المباشر" },
        }},
        new TokenGroupDto { Group = "الطلب", Tokens = new()
        {
            new() { Token = "{{Request.Number}}", Label = "رقم الطلب" },
            new() { Token = "{{Request.CreatedDate}}", Label = "تاريخ الطلب" },
            new() { Token = "{{Request.LeaveType}}", Label = "نوع الإجازة" },
            new() { Token = "{{Request.StartDate}}", Label = "تاريخ البداية" },
            new() { Token = "{{Request.EndDate}}", Label = "تاريخ النهاية" },
            new() { Token = "{{Request.Days}}", Label = "عدد الأيام" },
        }},
        new TokenGroupDto { Group = "الشركة", Tokens = new()
        {
            new() { Token = "{{Company.Name}}", Label = "اسم الشركة" },
            new() { Token = "{{Company.CR}}", Label = "السجل التجاري" },
            new() { Token = "{{Company.VAT}}", Label = "الرقم الضريبي" },
        }},
        new TokenGroupDto { Group = "النظام", Tokens = new()
        {
            new() { Token = "{{System.Today}}", Label = "تاريخ اليوم" },
        }},
    };

    public static readonly Dictionary<string, string> Sample = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Employee.FullName"] = "بسام محمد", ["Employee.EmployeeNumber"] = "EMP-0001",
        ["Employee.Department"] = "تقنية المعلومات", ["Employee.JobTitle"] = "مهندس برمجيات", ["Employee.Manager"] = "أحمد علي",
        ["Request.Number"] = "REQ-2026-000001", ["Request.CreatedDate"] = "2026-06-13",
        ["Request.LeaveType"] = "إجازة سنوية", ["Request.StartDate"] = "2026-07-01", ["Request.EndDate"] = "2026-07-05", ["Request.Days"] = "5",
        ["Company.Name"] = "شركة آمن للموارد البشرية", ["Company.CR"] = "1010101010", ["Company.VAT"] = "300012345600003",
        ["System.Today"] = "2026-06-13",
    };
}
