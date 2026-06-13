using System.Text.Json;
using HR.Domain.Engines.Documents;
using HR.Domain.Engines.Forms;
using HR.Domain.Engines.MasterData;
using HR.Domain.Engines.Requests;
using HR.Domain.Engines.Workflows;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Services.Requests;

/// <summary>
/// Provisions built-in System Requests fully (form + workflow + impact + print template).
/// Everything is object-referenced (category/leave-type via master data), never free text.
/// </summary>
public sealed class RequestSeeder : IRequestSeeder
{
    private readonly ApplicationDbContext _db;
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public RequestSeeder(ApplicationDbContext db) => _db = db;

    public async Task<int> SeedSystemRequestsAsync(CancellationToken ct)
    {
        // 0) Object references (created if missing — governance: requests reference these).
        var catTimeOff = await EnsureCategory("TIME_OFF", "الإجازات والحضور", "Time Off & Attendance", ct);
        var catFinance = await EnsureCategory("FINANCE", "المالية والرواتب", "Finance & Payroll", ct);
        var catLetters = await EnsureCategory("LETTERS", "الخطابات والشهادات", "Letters & Certificates", ct);
        var catHr = await EnsureCategory("HR", "الموارد البشرية والشخصية", "HR & Personal", ct);

        var ltAnnual = await EnsureLeaveType("ANNUAL", "إجازة سنوية", "Annual Leave", ct);
        var ltSick = await EnsureLeaveType("SICK", "إجازة مرضية", "Sick Leave", ct);
        var ltEmergency = await EnsureLeaveType("EMERGENCY", "إجازة طارئة", "Emergency Leave", ct);

        // 1) Print templates (official documents — full PDF rendering arrives with QuestPDF).
        var tplLeave = await EnsureDocTemplate("DOC_LEAVE_APPROVAL", "Leave Approval", "موافقة إجازة", LeaveApprovalHtml, ct);
        var tplSalaryCert = await EnsureDocTemplate("DOC_SALARY_CERTIFICATE", "Salary Certificate", "شهادة تعريف بالراتب", SalaryCertificateHtml, ct);

        // 2) Workflows (separate entities — requests reference them by id).
        var wfManager = await EnsureWorkflow("WF_REQ_MANAGER", "Manager Approval", "موافقة المدير",
            new[] { Step(ApproverType.DirectManager, "المدير المباشر", "Direct Manager") }, ct);
        var wfMgrFinPay = await EnsureWorkflow("WF_REQ_MGR_FIN_PAY", "Manager → Finance → Payroll", "المدير ← المالية ← الرواتب",
            new[] { Step(ApproverType.DirectManager, "المدير المباشر", "Direct Manager"), Step(ApproverType.SystemAdmin, "المالية", "Finance"), Step(ApproverType.SystemAdmin, "الرواتب", "Payroll") }, ct);
        var wfMgrFin = await EnsureWorkflow("WF_REQ_MGR_FIN", "Manager → Finance", "المدير ← المالية",
            new[] { Step(ApproverType.DirectManager, "المدير المباشر", "Direct Manager"), Step(ApproverType.SystemAdmin, "المالية", "Finance") }, ct);
        var wfMgrDeptHr = await EnsureWorkflow("WF_REQ_MGR_DEPT_HR", "Manager → Dept → HR", "المدير ← مدير الإدارة ← الموارد البشرية",
            new[] { Step(ApproverType.DirectManager, "المدير المباشر", "Direct Manager"), Step(ApproverType.DepartmentHead, "مدير الإدارة", "Department Manager"), Step(ApproverType.HrManager, "الموارد البشرية", "HR") }, ct);
        var wfHr = await EnsureWorkflow("WF_REQ_HR", "HR Approval", "موافقة الموارد البشرية",
            new[] { Step(ApproverType.HrManager, "الموارد البشرية", "HR") }, ct);

        // 3) System requests, each fully provisioned.
        int created = 0;
        created += await EnsureRequest("ANNUAL_LEAVE", "إجازة سنوية", "Annual Leave", catTimeOff, ltAnnual, wfManager, tplLeave,
            LeaveForm("FORM_ANNUAL_LEAVE", "نموذج إجازة سنوية", "Annual Leave Form"),
            Impact(leave: true, attendance: true, document: true), "Plane", "#34D399", ct);
        created += await EnsureRequest("SICK_LEAVE", "إجازة مرضية", "Sick Leave", catTimeOff, ltSick, wfManager, tplLeave,
            LeaveForm("FORM_SICK_LEAVE", "نموذج إجازة مرضية", "Sick Leave Form", attachment: true),
            Impact(leave: true, attendance: true, document: true), "Thermometer", "#F87171", ct);
        created += await EnsureRequest("EMERGENCY_LEAVE", "إجازة طارئة", "Emergency Leave", catTimeOff, ltEmergency, wfManager, tplLeave,
            LeaveForm("FORM_EMERGENCY_LEAVE", "نموذج إجازة طارئة", "Emergency Leave Form"),
            Impact(leave: true, attendance: true, document: true), "Siren", "#FB923C", ct);
        created += await EnsureRequest("SALARY_CERTIFICATE", "شهادة راتب", "Salary Certificate", catLetters, null, wfHr, tplSalaryCert,
            SalaryCertificateForm(), Impact(document: true), "FileText", "#60A5FA", ct);
        created += await EnsureRequest("SALARY_ADVANCE", "سلفة راتب", "Salary Advance", catFinance, null, wfMgrFinPay, null,
            AmountForm("FORM_SALARY_ADVANCE", "نموذج سلفة راتب", "Salary Advance Form"),
            Impact(payroll: true, createsLoan: true, finance: true), "Wallet", "#FBBF24", ct);
        created += await EnsureRequest("LOAN_REQUEST", "طلب قرض", "Loan Request", catFinance, null, wfMgrFin, null,
            LoanForm(), Impact(loans: true, createsLoan: true, finance: true), "HandCoins", "#A78BFA", ct);
        created += await EnsureRequest("ATTENDANCE_CORRECTION", "تصحيح حضور", "Attendance Correction", catTimeOff, null, wfManager, null,
            AttendanceCorrectionForm(), Impact(attendance: true), "Clock", "#22D3EE", ct);
        created += await EnsureRequest("BUSINESS_TRIP", "رحلة عمل", "Business Trip", catHr, null, wfMgrDeptHr, null,
            BusinessTripForm(), Impact(document: false), "Briefcase", "#94A3B8", ct);
        created += await EnsureRequest("EXPENSE_CLAIM", "مطالبة مصروف", "Expense Claim", catFinance, null, wfMgrFin, null,
            ExpenseForm(), Impact(expenses: true, finance: true), "Receipt", "#F472B6", ct);
        created += await EnsureRequest("EMPLOYEE_DATA_UPDATE", "تحديث بيانات", "Employee Data Update", catHr, null, wfHr, null,
            DataUpdateForm(), Impact(), "UserCog", "#A3E635", ct);

        await _db.SaveChangesAsync(ct);
        return created;
    }

    // ── Ensure helpers ──────────────────────────────────────────────────────────

    private async Task<Guid> EnsureCategory(string code, string ar, string en, CancellationToken ct)
        => await EnsureMasterData(MasterDataObjectType.RequestCategory, code, ar, en, ct);
    private async Task<Guid> EnsureLeaveType(string code, string ar, string en, CancellationToken ct)
        => await EnsureMasterData(MasterDataObjectType.LeaveType, code, ar, en, ct);

    private async Task<Guid> EnsureMasterData(string objectType, string code, string ar, string en, CancellationToken ct)
    {
        var existing = await _db.MasterDataItems.FirstOrDefaultAsync(m => m.ObjectType == objectType && m.Code == code, ct);
        if (existing is not null) return existing.Id;
        var item = new MasterDataItem { ObjectType = objectType, Code = code, NameAr = ar, NameEn = en, IsSystemDefault = true, IsActive = true };
        _db.MasterDataItems.Add(item);
        await _db.SaveChangesAsync(ct);
        return item.Id;
    }

    private async Task<Guid> EnsureDocTemplate(string code, string en, string ar, string html, CancellationToken ct)
    {
        var existing = await _db.DocumentTemplates.FirstOrDefaultAsync(d => d.Code == code, ct);
        if (existing is not null) return existing.Id;
        var tpl = new DocumentTemplate
        {
            Code = code, NameEn = en, NameAr = ar, Module = "Requests",
            Status = DocumentTemplateStatus.Published, OutputFormat = DocumentOutputFormat.Pdf,
            BodyTemplate = html, UseBranding = true, IsActive = true,
        };
        _db.DocumentTemplates.Add(tpl);
        await _db.SaveChangesAsync(ct);
        return tpl.Id;
    }

    private static WorkflowStepConfig Step(ApproverType type, string ar, string en)
        => new() { ApproverType = (int)type, NameAr = ar, NameEn = en };

    private async Task<Guid> EnsureWorkflow(string code, string en, string ar, WorkflowStepConfig[] steps, CancellationToken ct)
    {
        var existing = await _db.WorkflowDefinitions.FirstOrDefaultAsync(w => w.Code == code, ct);
        if (existing is not null) return existing.Id;
        var def = new WorkflowDefinition { Code = code, NameEn = en, NameAr = ar, TriggerEntityType = "RequestInstance", IsActive = true };
        _db.WorkflowDefinitions.Add(def);
        await _db.SaveChangesAsync(ct);
        var version = new WorkflowVersion
        {
            WorkflowDefinitionId = def.Id, VersionNumber = 1, IsPublished = true, PublishedAt = DateTime.UtcNow,
            Configuration = JsonSerializer.Serialize(new WorkflowChainConfig { Steps = steps.ToList() }, Json),
        };
        _db.WorkflowVersions.Add(version);
        await _db.SaveChangesAsync(ct);
        return def.Id;
    }

    private async Task<Guid> EnsureForm(FormSpec spec, CancellationToken ct)
    {
        var existing = await _db.FormDefinitions.FirstOrDefaultAsync(f => f.Code == spec.Code, ct);
        if (existing is not null) return existing.Id;
        var form = new FormDefinition { Code = spec.Code, NameEn = spec.NameEn, NameAr = spec.NameAr, Module = "Requests", IsPublished = true, IsActive = true };
        for (int i = 0; i < spec.Fields.Count; i++)
        {
            var f = spec.Fields[i];
            form.Fields.Add(new FormField
            {
                Code = f.Code, NameAr = f.NameAr, NameEn = f.NameEn, FieldType = f.Type,
                IsRequired = f.Required, SortOrder = i, Placeholder = f.Placeholder, Options = f.Options,
            });
        }
        _db.FormDefinitions.Add(form);
        await _db.SaveChangesAsync(ct);
        return form.Id;
    }

    private async Task<int> EnsureRequest(string code, string ar, string en, Guid categoryId, Guid? leaveTypeId,
        Guid workflowId, Guid? printTemplateId, FormSpec formSpec, ImpactSpec impact, string icon, string color, CancellationToken ct)
    {
        if (await _db.RequestTypes.AnyAsync(t => t.Code == code, ct)) return 0;
        var formId = await EnsureForm(formSpec, ct);
        var type = new RequestType
        {
            Code = code, NameAr = ar, NameEn = en, Kind = RequestKind.System, IsSystem = true, IsActive = true,
            CategoryId = categoryId, FormDefinitionId = formId, WorkflowDefinitionId = workflowId,
            PrintTemplateId = printTemplateId, LeaveTypeId = leaveTypeId, Icon = icon, Color = color,
            ImpactMapping = new RequestImpactMapping
            {
                AffectsLeaveBalance = impact.Leave, AffectsAttendance = impact.Attendance, AffectsPayroll = impact.Payroll,
                AffectsExpenses = impact.Expenses, AffectsLoans = impact.Loans, CreatesLoanRecord = impact.CreatesLoan,
                RequiresFinanceApproval = impact.Finance, GeneratesDocument = impact.Document,
            },
        };
        _db.RequestTypes.Add(type);
        return 1;
    }

    // ── Form definitions (system fields use canonical codes the engine understands) ──

    private static FormSpec LeaveForm(string code, string ar, string en, bool attachment = false)
    {
        var fields = new List<FieldSpec>
        {
            F("startDate", "تاريخ البداية", "Start Date", FieldType.Date, true),
            F("endDate", "تاريخ النهاية", "End Date", FieldType.Date, true),
            F("notes", "ملاحظات", "Notes", FieldType.TextArea, false),
        };
        if (attachment) fields.Add(F("attachment", "مرفق", "Attachment", FieldType.File, false));
        return new FormSpec(code, ar, en, fields);
    }

    private static FormSpec SalaryCertificateForm() => new("FORM_SALARY_CERTIFICATE", "نموذج شهادة راتب", "Salary Certificate Form", new()
    {
        F("addressedTo", "موجهة إلى", "Addressed To", FieldType.Text, true),
        F("purpose", "الغرض", "Purpose", FieldType.TextArea, true),
    });

    private static FormSpec AmountForm(string code, string ar, string en) => new(code, ar, en, new()
    {
        F("amount", "المبلغ", "Amount", FieldType.Currency, true),
        F("reason", "السبب", "Reason", FieldType.TextArea, true),
    });

    private static FormSpec LoanForm() => new("FORM_LOAN_REQUEST", "نموذج طلب قرض", "Loan Request Form", new()
    {
        F("amount", "المبلغ", "Amount", FieldType.Currency, true),
        F("installmentMonths", "عدد الأشهر", "Installment Months", FieldType.Number, true),
        F("reason", "السبب", "Reason", FieldType.TextArea, true),
    });

    private static FormSpec AttendanceCorrectionForm() => new("FORM_ATTENDANCE_CORRECTION", "نموذج تصحيح حضور", "Attendance Correction Form", new()
    {
        F("startDate", "التاريخ", "Date", FieldType.Date, true),
        F("reason", "السبب", "Reason", FieldType.TextArea, true),
    });

    private static FormSpec BusinessTripForm() => new("FORM_BUSINESS_TRIP", "نموذج رحلة عمل", "Business Trip Form", new()
    {
        F("destination", "الوجهة", "Destination", FieldType.Text, true),
        F("startDate", "تاريخ البداية", "Start Date", FieldType.Date, true),
        F("endDate", "تاريخ النهاية", "End Date", FieldType.Date, true),
        F("purpose", "الغرض", "Purpose", FieldType.TextArea, true),
    });

    private static FormSpec ExpenseForm() => new("FORM_EXPENSE_CLAIM", "نموذج مطالبة مصروف", "Expense Claim Form", new()
    {
        F("amount", "المبلغ", "Amount", FieldType.Currency, true),
        F("description", "الوصف", "Description", FieldType.TextArea, true),
        F("receipt", "الإيصال", "Receipt", FieldType.File, false),
    });

    private static FormSpec DataUpdateForm() => new("FORM_DATA_UPDATE", "نموذج تحديث بيانات", "Data Update Form", new()
    {
        F("reason", "ما الذي تريد تحديثه؟", "What do you want to update?", FieldType.TextArea, true),
    });

    private static FieldSpec F(string code, string ar, string en, FieldType type, bool required, string? placeholder = null, string? options = null)
        => new(code, ar, en, type, required, placeholder, options);

    private static ImpactSpec Impact(bool leave = false, bool attendance = false, bool payroll = false,
        bool expenses = false, bool loans = false, bool createsLoan = false, bool finance = false, bool document = false)
        => new(leave, attendance, payroll, expenses, loans, createsLoan, finance, document);

    private sealed record FormSpec(string Code, string NameAr, string NameEn, List<FieldSpec> Fields);
    private sealed record FieldSpec(string Code, string NameAr, string NameEn, FieldType Type, bool Required, string? Placeholder = null, string? Options = null);
    private sealed record ImpactSpec(bool Leave, bool Attendance, bool Payroll, bool Expenses, bool Loans, bool CreatesLoan, bool Finance, bool Document);

    // ── Official document HTML (tokens resolved at generation; styled for print) ──

    private const string LeaveApprovalHtml = @"
<div style='font-family:sans-serif;direction:rtl'>
  <h2 style='text-align:center'>موافقة على طلب إجازة</h2>
  <p>الشركة: {{CompanyName}} — السجل التجاري: {{CRNumber}} — الرقم الضريبي: {{VATNumber}}</p>
  <hr/>
  <p>نفيد بأنه تمت الموافقة على طلب الإجازة المقدّم من الموظف <b>{{EmployeeName}}</b> (رقم {{EmployeeNumber}})،
     القسم {{Department}}، نوع الإجازة {{LeaveType}} من {{StartDate}} إلى {{EndDate}}.</p>
  <p>تاريخ الإصدار: {{GeneratedDate}}</p>
</div>";

    private const string SalaryCertificateHtml = @"
<div style='font-family:sans-serif;direction:rtl'>
  <h2 style='text-align:center'>شهادة تعريف بالراتب</h2>
  <p>الشركة: {{CompanyName}} — السجل التجاري: {{CRNumber}} — الرقم الضريبي: {{VATNumber}}</p>
  <hr/>
  <p>تشهد {{CompanyName}} بأن الموظف <b>{{EmployeeName}}</b> (رقم {{EmployeeNumber}})،
     يعمل لدينا بمسمى {{JobTitle}} في قسم {{Department}}.</p>
  <p>تاريخ الإصدار: {{GeneratedDate}}</p>
</div>";
}
