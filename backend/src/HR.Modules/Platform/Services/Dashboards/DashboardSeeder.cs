using System.Text.Json;
using System.Text.Json.Serialization;
using HR.Application.Common.Interfaces;
using HR.Domain.Engines.Dashboards;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.Services.Catalog;
using HR.Modules.Platform.Services.WidgetData;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Services.Dashboards;

/// <summary>
/// Provisions ready-made dashboards. Every candidate widget is validated against the live
/// catalog first, so the seeder is fully object-driven: it ships only what the model supports.
/// </summary>
public sealed class DashboardSeeder : IDashboardSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    private readonly IObjectCatalogService _catalog;

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public DashboardSeeder(ApplicationDbContext db, ICurrentUserService user, IObjectCatalogService catalog)
    {
        _db = db; _user = user; _catalog = catalog;
    }

    private sealed record Template(DashboardTemplateInfo Info, Action<WidgetBuilder> Build);

    private static readonly Template[] Templates =
    {
        new(new("executive", "اللوحة التنفيذية", "Executive Dashboard", "نظرة تنفيذية شاملة على القوى العاملة والعمليات", "Sparkles"), BuildExecutive),
        new(new("hr", "لوحة الموارد البشرية", "HR Dashboard", "تحليلات القوى العاملة والتوزيع والتعيينات", "Users"), BuildHr),
        new(new("payroll", "لوحة الرواتب", "Payroll Dashboard", "تكلفة الرواتب والبدلات حسب الإدارة والفرع", "Banknote"), BuildPayroll),
        new(new("operations", "لوحة العمليات", "Operations Dashboard", "المهام والطلبات وحالتها", "Activity"), BuildOperations),
    };

    public IReadOnlyList<DashboardTemplateInfo> AvailableTemplates() => Templates.Select(t => t.Info).ToList();

    public Task<Guid> SeedExecutiveAsync(CancellationToken ct) => SeedTemplateAsync("executive", ct);

    public async Task<Guid> SeedTemplateAsync(string key, CancellationToken ct)
    {
        var template = Templates.FirstOrDefault(t => t.Info.Key == key) ?? Templates[0];
        var code = $"{template.Info.Key}-default";

        var existing = await _db.DashboardDefinitions.FirstOrDefaultAsync(d => d.Code == code, ct);
        if (existing is not null) return existing.Id;

        var dashboard = new DashboardDefinition
        {
            Code = code,
            NameEn = template.Info.NameEn,
            NameAr = template.Info.NameAr,
            Description = template.Info.Description,
            Scope = DashboardScope.Company,
            OwnerId = _user.UserId,
            IsDefault = template.Info.Key == "executive",
            IsSystem = true,
            IsActive = true,
            SortOrder = Array.IndexOf(Templates, template),
        };
        _db.DashboardDefinitions.Add(dashboard);

        var b = new WidgetBuilder(_catalog, dashboard.Id, Json);
        template.Build(b);

        foreach (var w in b.Widgets) _db.DashboardWidgets.Add(w);
        await _db.SaveChangesAsync(ct);
        return dashboard.Id;
    }

    // ── Template definitions (object-driven; missing objects/fields are skipped) ──

    private static void BuildExecutive(WidgetBuilder b)
    {
        b.Kpi("إجمالي الموظفين", "Total Employees", "Employee", "Count");
        b.Kpi("الإدارات", "Departments", "Department", "Count");
        b.Kpi("الفروع", "Branches", "Branch", "Count");
        b.Kpi("إجمالي المهام", "Total Tasks", "HrTask", "Count");
        b.Chart(WidgetType.PieChart, "الموظفون حسب الجنس", "Employees by Gender", "Employee", groupBy: "Gender");
        b.Chart(WidgetType.BarChart, "الموظفون حسب الإدارة", "Employees by Department", "Employee", groupBy: "DepartmentId", width: 6);
        b.Chart(WidgetType.BarChart, "الموظفون حسب الفرع", "Employees by Branch", "Employee", groupBy: "BranchId", width: 6);
        b.Kpi("تكلفة الرواتب الأساسية", "Total Basic Payroll", "Employee", "Sum", field: "BasicSalary");
        b.Chart(WidgetType.DonutChart, "المهام حسب الحالة", "Tasks by Status", "HrTask", groupBy: "Status");
        b.Chart(WidgetType.DonutChart, "المهام حسب الأولوية", "Tasks by Priority", "HrTask", groupBy: "Priority");
        b.Chart(WidgetType.LineChart, "التعيينات الجديدة (شهرياً)", "New Hires (Monthly)", "Employee", groupBy: "HireDate", granularity: "month", width: 12);
        b.Chart(WidgetType.BarChart, "الطلبات حسب الحالة", "Requests by Status", "FormSubmission", groupBy: "Status", width: 6);
    }

    private static void BuildHr(WidgetBuilder b)
    {
        b.Kpi("إجمالي الموظفين", "Total Employees", "Employee", "Count");
        b.Kpi("الإدارات", "Departments", "Department", "Count");
        b.Kpi("متوسط الراتب", "Average Salary", "Employee", "Average", field: "BasicSalary");
        b.Chart(WidgetType.DonutChart, "حسب الجنس", "By Gender", "Employee", groupBy: "Gender");
        b.Chart(WidgetType.BarChart, "حسب الإدارة", "By Department", "Employee", groupBy: "DepartmentId", width: 8);
        b.Chart(WidgetType.BarChart, "حسب الجنسية", "By Nationality", "Employee", groupBy: "Nationality", width: 6);
        b.Chart(WidgetType.BarChart, "حسب المسمى الوظيفي", "By Position", "Employee", groupBy: "PositionId", width: 6);
        b.Chart(WidgetType.LineChart, "التعيينات الجديدة (شهرياً)", "New Hires (Monthly)", "Employee", groupBy: "HireDate", granularity: "month", width: 12);
    }

    private static void BuildPayroll(WidgetBuilder b)
    {
        b.Kpi("تكلفة الرواتب", "Total Payroll", "Employee", "Sum", field: "BasicSalary");
        b.Kpi("متوسط الراتب", "Average Salary", "Employee", "Average", field: "BasicSalary");
        b.Kpi("عدد الموظفين", "Headcount", "Employee", "Count");
        b.Chart(WidgetType.BarChart, "تكلفة الرواتب حسب الإدارة", "Payroll by Department", "Employee", groupBy: "DepartmentId", aggregation: "Sum", field: "BasicSalary", width: 8);
        b.Chart(WidgetType.BarChart, "تكلفة الرواتب حسب الفرع", "Payroll by Branch", "Employee", groupBy: "BranchId", aggregation: "Sum", field: "BasicSalary", width: 4);
        b.Chart(WidgetType.BarChart, "متوسط الراتب حسب الدرجة", "Avg Salary by Grade", "Employee", groupBy: "GradeId", aggregation: "Average", field: "BasicSalary", width: 6);
        b.Chart(WidgetType.PieChart, "الموظفون حسب طريقة الدفع", "By Payment Method", "Employee", groupBy: "PaymentMethodId", width: 6);
    }

    private static void BuildOperations(WidgetBuilder b)
    {
        b.Kpi("إجمالي المهام", "Total Tasks", "HrTask", "Count");
        b.Kpi("إجمالي الطلبات", "Total Requests", "FormSubmission", "Count");
        b.Chart(WidgetType.DonutChart, "المهام حسب الحالة", "Tasks by Status", "HrTask", groupBy: "Status", width: 6);
        b.Chart(WidgetType.DonutChart, "المهام حسب الأولوية", "Tasks by Priority", "HrTask", groupBy: "Priority", width: 6);
        b.Chart(WidgetType.BarChart, "الطلبات حسب الحالة", "Requests by Status", "FormSubmission", groupBy: "Status", width: 6);
        b.Chart(WidgetType.BarChart, "الطلبات حسب النموذج", "Requests by Form", "FormSubmission", groupBy: "FormDefinitionId", width: 6);
        b.Chart(WidgetType.LineChart, "المهام المنشأة (شهرياً)", "Tasks Created (Monthly)", "HrTask", groupBy: "CreatedAt", granularity: "month", width: 12);
    }

    /// <summary>Lays out validated widgets on a 12-column grid, skipping unsupported ones.</summary>
    public sealed class WidgetBuilder
    {
        private readonly IObjectCatalogService _catalog;
        private readonly Guid _dashboardId;
        private readonly JsonSerializerOptions _json;
        private int _col, _row, _rowHeight;
        public List<DashboardWidget> Widgets { get; } = new();

        public WidgetBuilder(IObjectCatalogService catalog, Guid dashboardId, JsonSerializerOptions json)
        {
            _catalog = catalog; _dashboardId = dashboardId; _json = json;
        }

        public void Kpi(string ar, string en, string objectCode, string aggregation, string? field = null)
            => Add(WidgetType.KpiCard, ar, en, objectCode, aggregation, field, null, null, 3, 2);

        public void Chart(WidgetType type, string ar, string en, string objectCode, string? groupBy = null,
            string? aggregation = null, string? field = null, string? granularity = null, int width = 6)
            => Add(type, ar, en, objectCode, aggregation ?? "Count", field, groupBy, granularity, width, 4);

        private void Add(WidgetType type, string ar, string en, string objectCode, string aggregation,
            string? field, string? groupBy, string? granularity, int width, int height)
        {
            var obj = _catalog.GetObject(objectCode);
            if (obj is null) return;
            if (field is not null && obj.Fields.All(f => f.Code != field)) return;
            if (groupBy is not null && obj.Fields.All(f => f.Code != groupBy)) return;

            var spec = new WidgetQuerySpec
            {
                ObjectCode = objectCode,
                Aggregation = aggregation,
                AggregationField = field,
                GroupByField = groupBy,
                DateGranularity = granularity,
                Visualization = type.ToString(),
                Limit = type == WidgetType.LineChart ? 24 : 12,
            };

            if (_col + width > 12) { _col = 0; _row += _rowHeight; _rowHeight = 0; }

            Widgets.Add(new DashboardWidget
            {
                DashboardDefinitionId = _dashboardId,
                WidgetType = type,
                TitleEn = en,
                TitleAr = ar,
                Configuration = JsonSerializer.Serialize(spec, _json),
                SortOrder = Widgets.Count,
                IsVisible = true,
                Layout = new WidgetLayout { Column = _col, Row = _row, Width = width, Height = height },
            });

            _col += width;
            _rowHeight = Math.Max(_rowHeight, height);
        }
    }
}
