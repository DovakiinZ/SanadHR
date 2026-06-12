using System.Text;

namespace HR.Modules.Platform.Services.Catalog;

/// <summary>
/// Bilingual display labels. This is a presentation dictionary only — discovery and
/// execution never depend on it, so unknown/future objects still appear (labelled by a
/// humanized English fallback) until a curated label is added here.
/// </summary>
internal static class Labels
{
    private static readonly Dictionary<string, (string en, string ar)> Objects = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Employee"] = ("Employees", "الموظفون"),
        ["Department"] = ("Departments", "الإدارات"),
        ["Branch"] = ("Branches", "الفروع"),
        ["HrTask"] = ("Tasks", "المهام"),
        ["MasterDataItem"] = ("Master Data", "البيانات الأساسية"),
        ["FormSubmission"] = ("Requests", "الطلبات"),
        ["FormDefinition"] = ("Forms", "النماذج"),
        ["WorkflowInstance"] = ("Workflow Cases", "حالات سير العمل"),
        ["WorkflowDefinition"] = ("Workflows", "مسارات العمل"),
        ["GeneratedDocument"] = ("Documents", "المستندات"),
        ["Position"] = ("Positions", "المسميات الوظيفية"),
        ["Grade"] = ("Grades", "الدرجات"),
        ["CostCenter"] = ("Cost Centers", "مراكز التكلفة"),
        ["EmployeeAllowance"] = ("Allowances", "البدلات"),
        ["AuditEntry"] = ("Audit Entries", "سجلات التدقيق"),
        ["DashboardDefinition"] = ("Dashboards", "اللوحات"),
    };

    private static readonly Dictionary<string, (string en, string ar)> Fields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["DepartmentId"] = ("Department", "الإدارة"),
        ["BranchId"] = ("Branch", "الفرع"),
        ["ManagerId"] = ("Manager", "المدير"),
        ["PositionId"] = ("Position", "المسمى الوظيفي"),
        ["GradeId"] = ("Grade", "الدرجة"),
        ["CostCenterId"] = ("Cost Center", "مركز التكلفة"),
        ["NationalityId"] = ("Nationality", "الجنسية"),
        ["Nationality"] = ("Nationality", "الجنسية"),
        ["ContractTypeId"] = ("Contract Type", "نوع العقد"),
        ["EmploymentTypeId"] = ("Employment Type", "نوع التوظيف"),
        ["PaymentMethodId"] = ("Payment Method", "طريقة الدفع"),
        ["Gender"] = ("Gender", "الجنس"),
        ["Status"] = ("Status", "الحالة"),
        ["Priority"] = ("Priority", "الأولوية"),
        ["BasicSalary"] = ("Basic Salary", "الراتب الأساسي"),
        ["FirstName"] = ("First Name", "الاسم الأول"),
        ["LastName"] = ("Last Name", "اسم العائلة"),
        ["FirstNameAr"] = ("First Name (Ar)", "الاسم الأول"),
        ["LastNameAr"] = ("Last Name (Ar)", "اسم العائلة"),
        ["Email"] = ("Email", "البريد الإلكتروني"),
        ["Phone"] = ("Phone", "الهاتف"),
        ["HireDate"] = ("Hire Date", "تاريخ التعيين"),
        ["DateOfBirth"] = ("Date of Birth", "تاريخ الميلاد"),
        ["CreatedAt"] = ("Created", "تاريخ الإنشاء"),
        ["UpdatedAt"] = ("Updated", "تاريخ التحديث"),
        ["DueDate"] = ("Due Date", "تاريخ الاستحقاق"),
        ["IsActive"] = ("Active", "نشط"),
        ["NameEn"] = ("Name (En)", "الاسم (إنجليزي)"),
        ["NameAr"] = ("Name (Ar)", "الاسم (عربي)"),
        ["Title"] = ("Title", "العنوان"),
        ["TitleEn"] = ("Title (En)", "العنوان (إنجليزي)"),
        ["TitleAr"] = ("Title (Ar)", "العنوان (عربي)"),
        ["EmployeeNumber"] = ("Employee No.", "الرقم الوظيفي"),
        ["ObjectType"] = ("Type", "النوع"),
    };

    private static readonly Dictionary<string, string> Icons = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Employee"] = "Users",
        ["Department"] = "Building2",
        ["Branch"] = "MapPin",
        ["HrTask"] = "CheckSquare",
        ["FormSubmission"] = "FileText",
        ["GeneratedDocument"] = "FolderOpen",
        ["MasterDataItem"] = "Database",
    };

    public static string ObjectEn(string code) => Objects.TryGetValue(code, out var v) ? v.en : Humanize(code);
    public static string ObjectAr(string code) => Objects.TryGetValue(code, out var v) ? v.ar : Humanize(code);
    public static string? ObjectIcon(string code) => Icons.TryGetValue(code, out var v) ? v : null;

    public static string FieldEn(string col, bool isReference)
        => Fields.TryGetValue(col, out var v) ? v.en : Humanize(StripId(col, isReference));
    public static string FieldAr(string col, bool isReference)
        => Fields.TryGetValue(col, out var v) ? v.ar : Humanize(StripId(col, isReference));

    public static string EnumLabel(Type enumType, object value)
        => Humanize(Enum.GetName(enumType, value) ?? value.ToString() ?? "");

    private static string StripId(string col, bool isReference)
        => isReference && col.EndsWith("Id", StringComparison.Ordinal) && col.Length > 2
            ? col[..^2] : col;

    /// <summary>Split a PascalCase/identifier into spaced words.</summary>
    private static string Humanize(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        var sb = new StringBuilder(s.Length + 6);
        for (int i = 0; i < s.Length; i++)
        {
            var c = s[i];
            if (i > 0 && char.IsUpper(c) && (!char.IsUpper(s[i - 1]) || (i + 1 < s.Length && char.IsLower(s[i + 1]))))
                sb.Append(' ');
            sb.Append(c);
        }
        return sb.ToString();
    }
}
