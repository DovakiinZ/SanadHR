using HR.Domain.Engines.MasterData;

namespace HR.Infrastructure.Persistence;

/// <summary>A single default master data row to seed for a new tenant.</summary>
public record MasterDataDefault(
    string ObjectType,
    string Code,
    string NameEn,
    string NameAr,
    string? Color = null,
    string? Icon = null);

/// <summary>
/// Curated starter set of system-default master data, seeded per tenant on first
/// run (idempotently) and editable afterwards by tenant admins. Intentionally a
/// sensible starter — not exhaustive. Saudi-market oriented where relevant.
/// </summary>
public static class MasterDataDefaults
{
    public static IReadOnlyList<MasterDataDefault> All { get; } = Build();

    private static List<MasterDataDefault> Build()
    {
        var list = new List<MasterDataDefault>();

        void Add(string type, params (string Code, string En, string Ar)[] items)
        {
            foreach (var (code, en, ar) in items)
                list.Add(new MasterDataDefault(type, code, en, ar));
        }

        Add(MasterDataObjectType.EmploymentType,
            ("FULL_TIME", "Full Time", "دوام كامل"),
            ("PART_TIME", "Part Time", "دوام جزئي"),
            ("TEMPORARY", "Temporary", "مؤقت"),
            ("INTERN", "Intern", "متدرب"));

        Add(MasterDataObjectType.ContractType,
            ("PERMANENT", "Permanent", "دائم"),
            ("FIXED_TERM", "Fixed Term", "محدد المدة"),
            ("PROBATION", "Probation", "تحت التجربة"),
            ("CONSULTANT", "Consultant", "استشاري"));

        Add(MasterDataObjectType.LeaveType,
            ("ANNUAL", "Annual Leave", "إجازة سنوية"),
            ("SICK", "Sick Leave", "إجازة مرضية"),
            ("UNPAID", "Unpaid Leave", "إجازة بدون راتب"),
            ("MATERNITY", "Maternity Leave", "إجازة أمومة"),
            ("HAJJ", "Hajj Leave", "إجازة حج"),
            ("BEREAVEMENT", "Bereavement Leave", "إجازة وفاة"));

        Add(MasterDataObjectType.AllowanceType,
            ("HOUSING", "Housing Allowance", "بدل سكن"),
            ("TRANSPORT", "Transportation Allowance", "بدل نقل"),
            ("MOBILE", "Mobile Allowance", "بدل جوال"),
            ("FOOD", "Food Allowance", "بدل طعام"));

        Add(MasterDataObjectType.DeductionType,
            ("GOSI", "GOSI", "التأمينات الاجتماعية"),
            ("LOAN", "Loan Repayment", "سداد قرض"),
            ("ABSENCE", "Absence Deduction", "خصم غياب"),
            ("PENALTY", "Penalty", "جزاء"));

        Add(MasterDataObjectType.DocumentType,
            ("CONTRACT", "Employment Contract", "عقد عمل"),
            ("ID_COPY", "National ID Copy", "صورة الهوية"),
            ("IQAMA", "Iqama", "إقامة"),
            ("PASSPORT", "Passport", "جواز سفر"),
            ("CERTIFICATE", "Certificate", "شهادة"),
            ("SALARY_LETTER", "Salary Letter", "خطاب راتب"));

        Add(MasterDataObjectType.RequestType,
            ("LEAVE", "Leave Request", "طلب إجازة"),
            ("LETTER", "Letter Request", "طلب خطاب"),
            ("EXPENSE", "Expense Claim", "مطالبة مصروف"),
            ("LOAN", "Loan Request", "طلب قرض"),
            ("BUSINESS_TRIP", "Business Trip", "رحلة عمل"));

        Add(MasterDataObjectType.RequestCategory,
            ("TIME_OFF", "Time Off & Attendance", "الإجازات والحضور"),
            ("FINANCE", "Finance & Payroll", "المالية والرواتب"),
            ("LETTERS", "Letters & Certificates", "الخطابات والشهادات"),
            ("IT", "IT & Equipment", "تقنية المعلومات والمعدات"),
            ("HR", "HR & Personal", "الموارد البشرية والشخصية"),
            ("GENERAL", "General", "عام"));

        Add(MasterDataObjectType.PaymentMethod,
            ("BANK_TRANSFER", "Bank Transfer", "تحويل بنكي"),
            ("CASH", "Cash", "نقدي"),
            ("SALARY_CARD", "Salary Card", "بطاقة راتب"));

        Add(MasterDataObjectType.PayrollGroup,
            ("MONTHLY", "Monthly Payroll", "رواتب شهرية"),
            ("HOURLY", "Hourly Payroll", "رواتب بالساعة"));

        Add(MasterDataObjectType.LeavePolicy,
            ("STANDARD", "Standard Policy", "السياسة القياسية"),
            ("EXECUTIVE", "Executive Policy", "سياسة تنفيذية"));

        Add(MasterDataObjectType.ShiftType,
            ("MORNING", "Morning Shift", "وردية صباحية"),
            ("EVENING", "Evening Shift", "وردية مسائية"),
            ("NIGHT", "Night Shift", "وردية ليلية"),
            ("FLEXIBLE", "Flexible", "مرن"));

        Add(MasterDataObjectType.ExpenseCategory,
            ("TRAVEL", "Travel", "سفر"),
            ("ACCOMMODATION", "Accommodation", "إقامة"),
            ("MEALS", "Meals", "وجبات"),
            ("OFFICE", "Office Supplies", "مستلزمات مكتبية"),
            ("TRAINING", "Training", "تدريب"));

        Add(MasterDataObjectType.LoanType,
            ("PERSONAL", "Personal Loan", "قرض شخصي"),
            ("EMERGENCY", "Emergency Loan", "قرض طارئ"),
            ("HOUSING", "Housing Loan", "قرض سكن"));

        Add(MasterDataObjectType.AssetType,
            ("LAPTOP", "Laptop", "حاسب محمول"),
            ("PHONE", "Mobile Phone", "هاتف جوال"),
            ("VEHICLE", "Vehicle", "مركبة"),
            ("ACCESS_CARD", "Access Card", "بطاقة دخول"));

        Add(MasterDataObjectType.RecruitmentSource,
            ("LINKEDIN", "LinkedIn", "لينكدإن"),
            ("REFERRAL", "Referral", "ترشيح"),
            ("WEBSITE", "Company Website", "موقع الشركة"),
            ("AGENCY", "Recruitment Agency", "وكالة توظيف"));

        Add(MasterDataObjectType.CandidateStage,
            ("APPLIED", "Applied", "تقدم"),
            ("SCREENING", "Screening", "فرز"),
            ("INTERVIEW", "Interview", "مقابلة"),
            ("OFFER", "Offer", "عرض"),
            ("HIRED", "Hired", "تم التوظيف"),
            ("REJECTED", "Rejected", "مرفوض"));

        Add(MasterDataObjectType.JobTitle,
            ("CEO", "Chief Executive Officer", "الرئيس التنفيذي"),
            ("HR_MANAGER", "HR Manager", "مدير الموارد البشرية"),
            ("ACCOUNTANT", "Accountant", "محاسب"),
            ("SWE", "Software Engineer", "مهندس برمجيات"),
            ("SALES_REP", "Sales Representative", "مندوب مبيعات"));

        Add(MasterDataObjectType.WorkLocation,
            ("HQ", "Head Office", "المكتب الرئيسي"),
            ("REMOTE", "Remote", "عن بُعد"),
            ("HYBRID", "Hybrid", "هجين"));

        Add(MasterDataObjectType.Bank,
            ("ALRAJHI", "Al Rajhi Bank", "مصرف الراجحي"),
            ("SNB", "Saudi National Bank", "البنك الأهلي السعودي"),
            ("RIYAD", "Riyad Bank", "بنك الرياض"),
            ("ALINMA", "Alinma Bank", "مصرف الإنماء"),
            ("SAB", "Saudi Awwal Bank", "البنك السعودي الأول"));

        Add(MasterDataObjectType.Nationality,
            ("SA", "Saudi", "سعودي"),
            ("EG", "Egyptian", "مصري"),
            ("IN", "Indian", "هندي"),
            ("PK", "Pakistani", "باكستاني"),
            ("PH", "Filipino", "فلبيني"),
            ("SD", "Sudanese", "سوداني"),
            ("YE", "Yemeni", "يمني"));

        Add(MasterDataObjectType.Tag,
            ("HIGH_POTENTIAL", "High Potential", "كفاءة واعدة"),
            ("KEY_TALENT", "Key Talent", "موهبة رئيسية"),
            ("NEW_HIRE", "New Hire", "تعيين جديد"));

        Add(MasterDataObjectType.Skill,
            ("LEADERSHIP", "Leadership", "قيادة"),
            ("COMMUNICATION", "Communication", "تواصل"),
            ("EXCEL", "Microsoft Excel", "مايكروسوفت إكسل"),
            ("PROJECT_MGMT", "Project Management", "إدارة المشاريع"));

        Add(MasterDataObjectType.Grade,
            ("G1", "Grade 1", "الدرجة 1"),
            ("G2", "Grade 2", "الدرجة 2"),
            ("G3", "Grade 3", "الدرجة 3"),
            ("G4", "Grade 4", "الدرجة 4"));

        return list;
    }
}
