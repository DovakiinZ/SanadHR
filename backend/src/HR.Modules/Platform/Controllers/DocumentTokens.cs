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
            new() { Token = "{{Employee.Nationality}}", Label = "الجنسية" },
            new() { Token = "{{Employee.NationalId}}", Label = "رقم الهوية" },
            new() { Token = "{{Employee.HireDate}}", Label = "تاريخ التعيين" },
            new() { Token = "{{Employee.Email}}", Label = "البريد الإلكتروني" },
            new() { Token = "{{Employee.Phone}}", Label = "الجوال" },
        }},
        new TokenGroupDto { Group = "الطلب", Tokens = new()
        {
            new() { Token = "{{Request.Number}}", Label = "رقم الطلب" },
            new() { Token = "{{Request.Type}}", Label = "نوع الطلب" },
            new() { Token = "{{Request.CreatedDate}}", Label = "تاريخ الطلب" },
            new() { Token = "{{Request.ApprovalDate}}", Label = "تاريخ الاعتماد" },
            new() { Token = "{{Request.Status}}", Label = "حالة الطلب" },
        }},
        new TokenGroupDto { Group = "الإجازة", Tokens = new()
        {
            new() { Token = "{{Leave.Type}}", Label = "نوع الإجازة" },
            new() { Token = "{{Leave.StartDate}}", Label = "تاريخ البداية" },
            new() { Token = "{{Leave.EndDate}}", Label = "تاريخ النهاية" },
            new() { Token = "{{Leave.Days}}", Label = "عدد الأيام" },
        }},
        new TokenGroupDto { Group = "الراتب", Tokens = new()
        {
            new() { Token = "{{Payroll.BasicSalary}}", Label = "الراتب الأساسي" },
            new() { Token = "{{Payroll.HousingAllowance}}", Label = "بدل السكن" },
            new() { Token = "{{Payroll.TransportationAllowance}}", Label = "بدل النقل" },
            new() { Token = "{{Payroll.TotalSalary}}", Label = "إجمالي الراتب" },
            new() { Token = "{{Payroll.Currency}}", Label = "العملة" },
        }},
        new TokenGroupDto { Group = "الشركة", Tokens = new()
        {
            new() { Token = "{{Company.Name}}", Label = "اسم الشركة (عربي)" },
            new() { Token = "{{Company.NameEn}}", Label = "اسم الشركة (إنجليزي)" },
            new() { Token = "{{Company.CR}}", Label = "السجل التجاري" },
            new() { Token = "{{Company.VAT}}", Label = "الرقم الضريبي" },
            new() { Token = "{{Company.Address}}", Label = "العنوان" },
            new() { Token = "{{Company.Phone}}", Label = "الهاتف" },
            new() { Token = "{{Company.Email}}", Label = "البريد الإلكتروني" },
            new() { Token = "{{Company.Website}}", Label = "الموقع الإلكتروني" },
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
        ["Employee.Nationality"] = "سعودي", ["Employee.NationalId"] = "1012345678",
        ["Employee.HireDate"] = "2022-03-01", ["Employee.Email"] = "bassam@company.com", ["Employee.Phone"] = "0555000111",
        ["Request.Number"] = "REQ-2026-000001", ["Request.Type"] = "طلب إجازة", ["Request.CreatedDate"] = "2026-06-13",
        ["Request.ApprovalDate"] = "2026-06-14", ["Request.Status"] = "معتمد",
        ["Leave.Type"] = "إجازة سنوية", ["Leave.StartDate"] = "2026-07-01", ["Leave.EndDate"] = "2026-07-05", ["Leave.Days"] = "5",
        ["Payroll.BasicSalary"] = "10,000", ["Payroll.HousingAllowance"] = "2,500", ["Payroll.TransportationAllowance"] = "1,000",
        ["Payroll.TotalSalary"] = "13,500", ["Payroll.Currency"] = "ر.س",
        ["Company.Name"] = "شركة آمن للموارد البشرية", ["Company.NameEn"] = "AMN HR Solutions",
        ["Company.CR"] = "1010101010", ["Company.VAT"] = "300012345600003",
        ["Company.Address"] = "الرياض، المملكة العربية السعودية", ["Company.Phone"] = "0112000000",
        ["Company.Email"] = "info@company.com", ["Company.Website"] = "www.company.com",
        // Legacy aliases (original seeded token names)
        ["Request.LeaveType"] = "إجازة سنوية", ["Request.StartDate"] = "2026-07-01", ["Request.EndDate"] = "2026-07-05", ["Request.Days"] = "5",
        ["System.Today"] = "2026-06-14",
    };
}
