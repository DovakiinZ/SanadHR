import { Task, TaskTemplate, TaskStatus, TaskPriority, TaskSource } from "@/types";

export const taskStatuses: { value: TaskStatus; label: string; color: string }[] = [
  { value: "جديدة", label: "جديدة", color: "bg-blue-500/10 text-blue-400 border-blue-500/20" },
  { value: "قيد التنفيذ", label: "قيد التنفيذ", color: "bg-amber-500/10 text-amber-400 border-amber-500/20" },
  { value: "بانتظار الموافقة", label: "بانتظار الموافقة", color: "bg-purple-500/10 text-purple-400 border-purple-500/20" },
  { value: "بانتظار الموظف", label: "بانتظار الموظف", color: "bg-cyan-500/10 text-cyan-400 border-cyan-500/20" },
  { value: "بانتظار الموارد البشرية", label: "بانتظار HR", color: "bg-indigo-500/10 text-indigo-400 border-indigo-500/20" },
  { value: "بانتظار المالية", label: "بانتظار المالية", color: "bg-teal-500/10 text-teal-400 border-teal-500/20" },
  { value: "مكتملة", label: "مكتملة", color: "bg-green-500/10 text-green-400 border-green-500/20" },
  { value: "ملغاة", label: "ملغاة", color: "bg-zinc-500/10 text-zinc-400 border-zinc-500/20" },
  { value: "متأخرة", label: "متأخرة", color: "bg-red-500/10 text-red-400 border-red-500/20" },
];

export const taskPriorities: { value: TaskPriority; label: string; color: string; icon: string }[] = [
  { value: "منخفضة", label: "منخفضة", color: "bg-zinc-500/10 text-zinc-400 border-zinc-500/20", icon: "↓" },
  { value: "متوسطة", label: "متوسطة", color: "bg-blue-500/10 text-blue-400 border-blue-500/20", icon: "→" },
  { value: "عالية", label: "عالية", color: "bg-amber-500/10 text-amber-400 border-amber-500/20", icon: "↑" },
  { value: "عاجلة", label: "عاجلة", color: "bg-orange-500/10 text-orange-400 border-orange-500/20", icon: "⚡" },
  { value: "حرجة", label: "حرجة", color: "bg-red-500/10 text-red-400 border-red-500/20", icon: "🔴" },
];

export const taskSources: { value: TaskSource; label: string }[] = [
  { value: "يدوي", label: "يدوي" },
  { value: "سير العمل", label: "سير العمل" },
  { value: "طلب", label: "طلب" },
  { value: "انتهاء مستند", label: "انتهاء مستند" },
  { value: "الرواتب", label: "الرواتب" },
  { value: "الحضور", label: "الحضور" },
  { value: "تهيئة موظف", label: "تهيئة موظف" },
  { value: "إنهاء خدمات", label: "إنهاء خدمات" },
  { value: "التوظيف", label: "التوظيف" },
  { value: "أتمتة النظام", label: "أتمتة النظام" },
];

export const mockTasks: Task[] = [
  {
    id: "1",
    taskId: "TSK-001",
    title: "تهيئة الموظف الجديد - أحمد محمد العلي",
    description: "إتمام جميع إجراءات تهيئة الموظف الجديد أحمد محمد العلي في قسم الهندسة. يشمل ذلك جمع المستندات وإنشاء العقد وتعيين المدير والسياسات.",
    status: "قيد التنفيذ",
    priority: "عالية",
    source: "تهيئة موظف",
    assignee: "سارة الأحمد",
    createdBy: "النظام",
    department: "الموارد البشرية",
    relatedEmployee: "أحمد محمد العلي",
    dueDate: "2026-06-02",
    reminderDate: "2026-05-30",
    tags: ["تهيئة", "موظف جديد"],
    checklist: [
      { id: "c1", title: "جمع صورة الهوية الوطنية", completed: true, completedBy: "سارة الأحمد", completedAt: "2026-05-26" },
      { id: "c2", title: "جمع صورة جواز السفر", completed: true, completedBy: "سارة الأحمد", completedAt: "2026-05-26" },
      { id: "c3", title: "إنشاء العقد الوظيفي", completed: false },
      { id: "c4", title: "تعيين المدير المباشر", completed: false },
      { id: "c5", title: "تعيين الوردية", completed: false },
      { id: "c6", title: "تعيين سياسة الإجازات", completed: false },
      { id: "c7", title: "تعيين مجموعة الرواتب", completed: false },
      { id: "c8", title: "إنشاء حساب النظام", completed: false },
      { id: "c9", title: "تسليم الأصول", completed: false },
      { id: "c10", title: "إرسال بريد الترحيب", completed: false },
    ],
    comments: [
      { id: "cm1", author: "سارة الأحمد", content: "تم استلام المستندات الأساسية من الموظف", createdAt: "2026-05-26T10:30:00" },
      { id: "cm2", author: "خالد المنصور", content: "يرجى التأكد من توفر مكتب في الطابق الثالث", createdAt: "2026-05-27T09:15:00" },
    ],
    attachments: [
      { id: "a1", name: "هوية_وطنية.pdf", size: "1.2 MB", type: "pdf", uploadedBy: "سارة الأحمد", uploadedAt: "2026-05-26" },
      { id: "a2", name: "جواز_سفر.pdf", size: "2.1 MB", type: "pdf", uploadedBy: "سارة الأحمد", uploadedAt: "2026-05-26" },
    ],
    activityLog: [
      { id: "al1", action: "إنشاء المهمة", user: "النظام", timestamp: "2026-05-25T08:00:00" },
      { id: "al2", action: "تعيين المهمة", user: "النظام", timestamp: "2026-05-25T08:00:00", details: "تم التعيين إلى سارة الأحمد" },
      { id: "al3", action: "إكمال عنصر القائمة", user: "سارة الأحمد", timestamp: "2026-05-26T10:30:00", details: "جمع صورة الهوية الوطنية" },
      { id: "al4", action: "إضافة تعليق", user: "سارة الأحمد", timestamp: "2026-05-26T10:30:00" },
    ],
    createdAt: "2026-05-25",
    updatedAt: "2026-05-27",
  },
  {
    id: "2",
    taskId: "TSK-002",
    title: "مراجعة كشف رواتب شهر مايو",
    description: "مراجعة واعتماد كشف رواتب شهر مايو 2026 لجميع الموظفين. يشمل التحقق من الخصومات والإضافات والعمل الإضافي.",
    status: "جديدة",
    priority: "حرجة",
    source: "الرواتب",
    assignee: "فهد العتيبي",
    createdBy: "النظام",
    department: "المالية",
    relatedPayrollRun: "PAY-2026-05",
    dueDate: "2026-05-30",
    tags: ["رواتب", "مراجعة شهرية"],
    checklist: [
      { id: "c1", title: "مراجعة خصومات الحضور", completed: false },
      { id: "c2", title: "مراجعة العمل الإضافي", completed: false },
      { id: "c3", title: "مراجعة السلف", completed: false },
      { id: "c4", title: "مراجعة المصروفات", completed: false },
      { id: "c5", title: "اعتماد الكشف", completed: false },
      { id: "c6", title: "تصدير الكشف للبنك", completed: false },
    ],
    comments: [],
    attachments: [],
    activityLog: [
      { id: "al1", action: "إنشاء المهمة", user: "النظام", timestamp: "2026-05-28T00:00:00" },
      { id: "al2", action: "تعيين المهمة", user: "النظام", timestamp: "2026-05-28T00:00:00", details: "تم التعيين إلى فهد العتيبي" },
    ],
    createdAt: "2026-05-28",
    updatedAt: "2026-05-28",
  },
  {
    id: "3",
    taskId: "TSK-003",
    title: "تجديد إقامة - محمد أحمد خان",
    description: "إقامة الموظف محمد أحمد خان ستنتهي خلال 30 يوم. يجب البدء بإجراءات التجديد.",
    status: "بانتظار الموظف",
    priority: "عاجلة",
    source: "انتهاء مستند",
    assignee: "نورة السعيد",
    createdBy: "النظام",
    department: "الموارد البشرية",
    relatedEmployee: "محمد أحمد خان",
    relatedDocument: "DOC-EQM-045",
    dueDate: "2026-06-15",
    reminderDate: "2026-06-01",
    tags: ["إقامة", "تجديد", "مستندات"],
    checklist: [
      { id: "c1", title: "طلب المستندات المحدثة من الموظف", completed: true, completedBy: "نورة السعيد", completedAt: "2026-05-20" },
      { id: "c2", title: "التحقق من المستندات المرفوعة", completed: false },
      { id: "c3", title: "تقديم طلب التجديد", completed: false },
      { id: "c4", title: "تحديث ملف الموظف", completed: false },
    ],
    comments: [
      { id: "cm1", author: "نورة السعيد", content: "تم إبلاغ الموظف وبانتظار المستندات", createdAt: "2026-05-20T14:00:00" },
    ],
    attachments: [],
    activityLog: [
      { id: "al1", action: "إنشاء المهمة", user: "النظام", timestamp: "2026-05-18T08:00:00", details: "تنبيه انتهاء مستند تلقائي" },
    ],
    createdAt: "2026-05-18",
    updatedAt: "2026-05-20",
  },
  {
    id: "4",
    taskId: "TSK-004",
    title: "مراجعة طلب إجازة - عبدالله السبيعي",
    description: "طلب إجازة سنوية لمدة 5 أيام من الموظف عبدالله السبيعي بحاجة لمراجعة واعتماد.",
    status: "بانتظار الموافقة",
    priority: "متوسطة",
    source: "طلب",
    assignee: "خالد المنصور",
    createdBy: "النظام",
    department: "الهندسة",
    relatedEmployee: "عبدالله السبيعي",
    relatedRequest: "REQ-2026-089",
    dueDate: "2026-05-29",
    tags: ["إجازة", "طلب"],
    checklist: [],
    comments: [
      { id: "cm1", author: "عبدالله السبيعي", content: "أحتاج الإجازة لظروف عائلية", createdAt: "2026-05-27T11:00:00" },
    ],
    attachments: [],
    activityLog: [
      { id: "al1", action: "إنشاء المهمة", user: "النظام", timestamp: "2026-05-27T11:00:00", details: "من سير عمل طلب الإجازة" },
    ],
    createdAt: "2026-05-27",
    updatedAt: "2026-05-27",
  },
  {
    id: "5",
    taskId: "TSK-005",
    title: "إنهاء خدمات - يوسف العمري",
    description: "تم اعتماد استقالة الموظف يوسف العمري. يجب إتمام إجراءات إنهاء الخدمات.",
    status: "قيد التنفيذ",
    priority: "عالية",
    source: "إنهاء خدمات",
    assignee: "سارة الأحمد",
    createdBy: "النظام",
    department: "الموارد البشرية",
    relatedEmployee: "يوسف العمري",
    dueDate: "2026-06-10",
    tags: ["إنهاء خدمات", "استقالة"],
    checklist: [
      { id: "c1", title: "استلام الأصول", completed: true, completedBy: "سارة الأحمد", completedAt: "2026-05-26" },
      { id: "c2", title: "تعطيل الوصول للنظام", completed: true, completedBy: "سارة الأحمد", completedAt: "2026-05-27" },
      { id: "c3", title: "حساب مكافأة نهاية الخدمة", completed: false },
      { id: "c4", title: "إصدار نموذج إخلاء الطرف", completed: false },
      { id: "c5", title: "إبلاغ المالية", completed: false },
      { id: "c6", title: "أرشفة ملف الموظف", completed: false },
    ],
    comments: [],
    attachments: [],
    activityLog: [
      { id: "al1", action: "إنشاء المهمة", user: "النظام", timestamp: "2026-05-25T08:00:00", details: "من سير عمل إنهاء الخدمات" },
    ],
    createdAt: "2026-05-25",
    updatedAt: "2026-05-27",
  },
  {
    id: "6",
    taskId: "TSK-006",
    title: "مراجعة سيرة ذاتية - مرشح وظيفة مطور",
    description: "مراجعة السيرة الذاتية للمرشح لوظيفة مطور برمجيات أول في قسم الهندسة.",
    status: "جديدة",
    priority: "متوسطة",
    source: "التوظيف",
    assignee: "منى الحربي",
    createdBy: "منى الحربي",
    department: "الموارد البشرية",
    relatedCandidate: "CAND-078",
    dueDate: "2026-06-01",
    tags: ["توظيف", "مراجعة"],
    checklist: [
      { id: "c1", title: "مراجعة السيرة الذاتية", completed: false },
      { id: "c2", title: "جدولة المقابلة", completed: false },
      { id: "c3", title: "إرسال العرض الوظيفي", completed: false },
      { id: "c4", title: "جمع مستندات المرشح", completed: false },
      { id: "c5", title: "تحويل المرشح لموظف", completed: false },
    ],
    comments: [],
    attachments: [
      { id: "a1", name: "CV_مرشح.pdf", size: "850 KB", type: "pdf", uploadedBy: "منى الحربي", uploadedAt: "2026-05-27" },
    ],
    activityLog: [
      { id: "al1", action: "إنشاء المهمة", user: "منى الحربي", timestamp: "2026-05-27T09:00:00" },
    ],
    createdAt: "2026-05-27",
    updatedAt: "2026-05-27",
  },
  {
    id: "7",
    taskId: "TSK-007",
    title: "مراجعة تصحيح حضور - سعد الدوسري",
    description: "الموظف سعد الدوسري قدم طلب تصحيح حضور ليوم 2026-05-22 بسبب خلل في البصمة.",
    status: "جديدة",
    priority: "منخفضة",
    source: "الحضور",
    assignee: "نورة السعيد",
    createdBy: "النظام",
    department: "الموارد البشرية",
    relatedEmployee: "سعد الدوسري",
    dueDate: "2026-05-31",
    tags: ["حضور", "تصحيح"],
    checklist: [
      { id: "c1", title: "مراجعة سجل البصمة", completed: false },
      { id: "c2", title: "التحقق من الخلل التقني", completed: false },
      { id: "c3", title: "اعتماد التصحيح", completed: false },
    ],
    comments: [],
    attachments: [],
    activityLog: [
      { id: "al1", action: "إنشاء المهمة", user: "النظام", timestamp: "2026-05-23T08:00:00" },
    ],
    createdAt: "2026-05-23",
    updatedAt: "2026-05-23",
  },
  {
    id: "8",
    taskId: "TSK-008",
    title: "مراجعة مصروفات - رحلة عمل فريق المبيعات",
    description: "مراجعة واعتماد مصروفات رحلة عمل فريق المبيعات إلى جدة.",
    status: "بانتظار المالية",
    priority: "متوسطة",
    source: "طلب",
    assignee: "فهد العتيبي",
    createdBy: "النظام",
    department: "المالية",
    relatedRequest: "EXP-2026-034",
    dueDate: "2026-06-03",
    tags: ["مصروفات", "رحلة عمل"],
    checklist: [
      { id: "c1", title: "مراجعة الفواتير", completed: true, completedBy: "فهد العتيبي", completedAt: "2026-05-27" },
      { id: "c2", title: "التحقق من السياسة", completed: true, completedBy: "فهد العتيبي", completedAt: "2026-05-27" },
      { id: "c3", title: "اعتماد الصرف", completed: false },
    ],
    comments: [
      { id: "cm1", author: "فهد العتيبي", content: "الفواتير مطابقة للسياسة. بانتظار اعتماد المدير المالي.", createdAt: "2026-05-27T16:00:00" },
    ],
    attachments: [
      { id: "a1", name: "فواتير_رحلة_جدة.pdf", size: "3.5 MB", type: "pdf", uploadedBy: "أحمد المبيعات", uploadedAt: "2026-05-25" },
    ],
    activityLog: [
      { id: "al1", action: "إنشاء المهمة", user: "النظام", timestamp: "2026-05-25T08:00:00" },
    ],
    createdAt: "2026-05-25",
    updatedAt: "2026-05-27",
  },
  {
    id: "9",
    taskId: "TSK-009",
    title: "إعداد تقرير الحضور الشهري",
    description: "إعداد تقرير الحضور الشهري لشهر مايو 2026 وتقديمه للإدارة.",
    status: "مكتملة",
    priority: "متوسطة",
    source: "يدوي",
    assignee: "نورة السعيد",
    createdBy: "سارة الأحمد",
    department: "الموارد البشرية",
    dueDate: "2026-05-27",
    tags: ["تقارير", "حضور"],
    checklist: [
      { id: "c1", title: "جمع بيانات الحضور", completed: true, completedBy: "نورة السعيد", completedAt: "2026-05-25" },
      { id: "c2", title: "مراجعة البيانات", completed: true, completedBy: "نورة السعيد", completedAt: "2026-05-26" },
      { id: "c3", title: "إعداد التقرير", completed: true, completedBy: "نورة السعيد", completedAt: "2026-05-27" },
      { id: "c4", title: "تقديم التقرير للإدارة", completed: true, completedBy: "نورة السعيد", completedAt: "2026-05-27" },
    ],
    comments: [],
    attachments: [
      { id: "a1", name: "تقرير_حضور_مايو.xlsx", size: "1.8 MB", type: "xlsx", uploadedBy: "نورة السعيد", uploadedAt: "2026-05-27" },
    ],
    activityLog: [
      { id: "al1", action: "إنشاء المهمة", user: "سارة الأحمد", timestamp: "2026-05-20T08:00:00" },
      { id: "al2", action: "إكمال المهمة", user: "نورة السعيد", timestamp: "2026-05-27T17:00:00" },
    ],
    createdAt: "2026-05-20",
    updatedAt: "2026-05-27",
  },
  {
    id: "10",
    taskId: "TSK-010",
    title: "تحديث بيانات التأمين الطبي",
    description: "تحديث بيانات التأمين الطبي للموظفين الجدد المنضمين في شهر مايو.",
    status: "متأخرة",
    priority: "عالية",
    source: "أتمتة النظام",
    assignee: "سارة الأحمد",
    createdBy: "النظام",
    department: "الموارد البشرية",
    dueDate: "2026-05-25",
    tags: ["تأمين", "تحديث بيانات"],
    checklist: [
      { id: "c1", title: "جمع بيانات الموظفين الجدد", completed: true, completedBy: "سارة الأحمد", completedAt: "2026-05-24" },
      { id: "c2", title: "إرسال البيانات لشركة التأمين", completed: false },
      { id: "c3", title: "استلام البطاقات", completed: false },
      { id: "c4", title: "توزيع البطاقات", completed: false },
    ],
    comments: [
      { id: "cm1", author: "سارة الأحمد", content: "تأخرنا بسبب عدم اكتمال بيانات بعض الموظفين", createdAt: "2026-05-26T09:00:00" },
    ],
    attachments: [],
    activityLog: [
      { id: "al1", action: "إنشاء المهمة", user: "النظام", timestamp: "2026-05-15T08:00:00" },
      { id: "al2", action: "تغيير الحالة إلى متأخرة", user: "النظام", timestamp: "2026-05-26T00:00:00" },
    ],
    createdAt: "2026-05-15",
    updatedAt: "2026-05-26",
  },
  {
    id: "11",
    taskId: "TSK-011",
    title: "اعتماد عمل إضافي - فريق العمليات",
    description: "اعتماد ساعات العمل الإضافي لفريق العمليات لأسبوع 19-23 مايو.",
    status: "ملغاة",
    priority: "متوسطة",
    source: "الحضور",
    assignee: "خالد المنصور",
    createdBy: "النظام",
    department: "العمليات",
    dueDate: "2026-05-26",
    tags: ["عمل إضافي", "حضور"],
    checklist: [],
    comments: [
      { id: "cm1", author: "خالد المنصور", content: "تم إلغاء طلب العمل الإضافي من قبل مدير العمليات", createdAt: "2026-05-25T10:00:00" },
    ],
    attachments: [],
    activityLog: [
      { id: "al1", action: "إنشاء المهمة", user: "النظام", timestamp: "2026-05-24T08:00:00" },
      { id: "al2", action: "إلغاء المهمة", user: "خالد المنصور", timestamp: "2026-05-25T10:00:00" },
    ],
    createdAt: "2026-05-24",
    updatedAt: "2026-05-25",
  },
  {
    id: "12",
    taskId: "TSK-012",
    title: "جدولة مقابلة - مرشح محاسب أول",
    description: "جدولة مقابلة مع المرشح لوظيفة محاسب أول في قسم المالية.",
    status: "قيد التنفيذ",
    priority: "متوسطة",
    source: "التوظيف",
    assignee: "منى الحربي",
    createdBy: "منى الحربي",
    department: "الموارد البشرية",
    relatedCandidate: "CAND-079",
    dueDate: "2026-06-05",
    tags: ["توظيف", "مقابلة"],
    checklist: [
      { id: "c1", title: "مراجعة السيرة الذاتية", completed: true, completedBy: "منى الحربي", completedAt: "2026-05-26" },
      { id: "c2", title: "التنسيق مع مدير المالية", completed: true, completedBy: "منى الحربي", completedAt: "2026-05-27" },
      { id: "c3", title: "إرسال دعوة المقابلة", completed: false },
      { id: "c4", title: "إجراء المقابلة", completed: false },
      { id: "c5", title: "تقييم المرشح", completed: false },
    ],
    comments: [],
    attachments: [],
    activityLog: [
      { id: "al1", action: "إنشاء المهمة", user: "منى الحربي", timestamp: "2026-05-26T08:00:00" },
    ],
    createdAt: "2026-05-26",
    updatedAt: "2026-05-27",
  },
];

export const mockTemplates: TaskTemplate[] = [
  {
    id: "t1",
    name: "تهيئة موظف جديد",
    description: "قالب شامل لإتمام جميع إجراءات تهيئة الموظف الجديد",
    relatedModule: "الموظفين",
    automationTrigger: "عند إنشاء موظف جديد",
    items: [
      { id: "ti1", title: "جمع المستندات", defaultAssignee: "مسؤول HR", relativeDueDays: 3, priority: "عالية", checklist: ["صورة الهوية", "صورة الجواز", "شهادات التعليم", "شهادات الخبرة"] },
      { id: "ti2", title: "إنشاء العقد", defaultAssignee: "مسؤول HR", relativeDueDays: 5, priority: "عالية", checklist: ["تحديد نوع العقد", "تحديد الراتب", "تحديد المزايا", "مراجعة قانونية"] },
      { id: "ti3", title: "إعداد بيئة العمل", defaultAssignee: "مدير القسم", relativeDueDays: 7, priority: "متوسطة", checklist: ["تعيين المكتب", "تعيين المعدات", "إنشاء حساب البريد"] },
      { id: "ti4", title: "تعيين السياسات", defaultAssignee: "مسؤول HR", relativeDueDays: 5, priority: "عالية", checklist: ["سياسة الإجازات", "مجموعة الرواتب", "الوردية", "التأمين الطبي"] },
    ],
    createdAt: "2026-01-15",
  },
  {
    id: "t2",
    name: "إنهاء خدمات موظف",
    description: "قالب لإتمام إجراءات إنهاء خدمات الموظف",
    relatedModule: "الموظفين",
    automationTrigger: "عند اعتماد الاستقالة",
    items: [
      { id: "ti1", title: "استلام الأصول", defaultAssignee: "مسؤول IT", relativeDueDays: 3, priority: "عالية", checklist: ["اللابتوب", "الهاتف", "البطاقة الأمنية", "المفاتيح"] },
      { id: "ti2", title: "تعطيل الوصول", defaultAssignee: "مسؤول IT", relativeDueDays: 1, priority: "حرجة", checklist: ["البريد الإلكتروني", "النظام الداخلي", "VPN", "الأنظمة السحابية"] },
      { id: "ti3", title: "التسوية المالية", defaultAssignee: "المحاسب", relativeDueDays: 10, priority: "عالية", checklist: ["مكافأة نهاية الخدمة", "الإجازات المستحقة", "السلف المتبقية", "المصروفات المعلقة"] },
      { id: "ti4", title: "إخلاء الطرف", defaultAssignee: "مسؤول HR", relativeDueDays: 14, priority: "عالية", checklist: ["نموذج إخلاء الطرف", "شهادة الخبرة", "أرشفة الملف"] },
    ],
    createdAt: "2026-01-15",
  },
  {
    id: "t3",
    name: "مراجعة رواتب شهرية",
    description: "قالب للمراجعة الشهرية لكشف الرواتب",
    relatedModule: "الرواتب",
    automationTrigger: "عند إنشاء كشف رواتب",
    items: [
      { id: "ti1", title: "مراجعة الحضور", defaultAssignee: "مسؤول HR", relativeDueDays: 2, priority: "عالية", checklist: ["خصومات الغياب", "التأخير", "العمل الإضافي"] },
      { id: "ti2", title: "مراجعة الاستقطاعات", defaultAssignee: "المحاسب", relativeDueDays: 3, priority: "عالية", checklist: ["السلف", "التأمينات", "الضرائب"] },
      { id: "ti3", title: "اعتماد وتصدير", defaultAssignee: "المدير المالي", relativeDueDays: 5, priority: "حرجة", checklist: ["مراجعة نهائية", "اعتماد الكشف", "تصدير للبنك"] },
    ],
    createdAt: "2026-02-01",
  },
  {
    id: "t4",
    name: "تجديد مستندات",
    description: "قالب لمتابعة تجديد مستندات الموظفين (إقامة، جواز، رخصة)",
    relatedModule: "المستندات",
    automationTrigger: "قبل 30 يوم من انتهاء المستند",
    items: [
      { id: "ti1", title: "إبلاغ الموظف", defaultAssignee: "مسؤول HR", relativeDueDays: -30, priority: "عالية", checklist: ["إرسال إشعار", "تحديد المستندات المطلوبة"] },
      { id: "ti2", title: "جمع المستندات", defaultAssignee: "مسؤول HR", relativeDueDays: -20, priority: "عالية", checklist: ["استلام المستندات", "التحقق من الصلاحية"] },
      { id: "ti3", title: "تقديم الطلب", defaultAssignee: "مسؤول HR", relativeDueDays: -14, priority: "عاجلة", checklist: ["تقديم الطلب الرسمي", "متابعة الحالة"] },
      { id: "ti4", title: "تحديث النظام", defaultAssignee: "مسؤول HR", relativeDueDays: 0, priority: "عالية", checklist: ["تحديث تاريخ الانتهاء", "رفع المستند الجديد"] },
    ],
    createdAt: "2026-02-01",
  },
  {
    id: "t5",
    name: "مقابلة توظيف",
    description: "قالب لإدارة عملية المقابلة مع المرشحين",
    relatedModule: "التوظيف",
    items: [
      { id: "ti1", title: "فرز السير الذاتية", defaultAssignee: "مسؤول التوظيف", relativeDueDays: 3, priority: "متوسطة", checklist: ["مراجعة المؤهلات", "مراجعة الخبرات", "التصفية الأولية"] },
      { id: "ti2", title: "المقابلة الأولية", defaultAssignee: "مسؤول التوظيف", relativeDueDays: 7, priority: "عالية", checklist: ["جدولة المقابلة", "إجراء المقابلة", "تقييم المرشح"] },
      { id: "ti3", title: "المقابلة التقنية", defaultAssignee: "مدير القسم", relativeDueDays: 10, priority: "عالية", checklist: ["تحضير الاختبار", "إجراء المقابلة", "التقييم الفني"] },
      { id: "ti4", title: "العرض الوظيفي", defaultAssignee: "مسؤول HR", relativeDueDays: 14, priority: "عاجلة", checklist: ["إعداد العرض", "إرسال العرض", "متابعة القبول"] },
    ],
    createdAt: "2026-03-01",
  },
];

// Helper functions
export function getTaskById(id: string): Task | undefined {
  return mockTasks.find((t) => t.id === id);
}

export function getTasksByStatus(status: TaskStatus): Task[] {
  return mockTasks.filter((t) => t.status === status);
}

export function getTasksByAssignee(assignee: string): Task[] {
  return mockTasks.filter((t) => t.assignee === assignee);
}

export function getTasksByDepartment(department: string): Task[] {
  return mockTasks.filter((t) => t.department === department);
}

export function getTasksByPriority(priority: TaskPriority): Task[] {
  return mockTasks.filter((t) => t.priority === priority);
}

export function getTasksBySource(source: TaskSource): Task[] {
  return mockTasks.filter((t) => t.source === source);
}

export function getOverdueTasks(): Task[] {
  return mockTasks.filter((t) => t.status === "متأخرة");
}

export function getPendingTasksCount(): number {
  return mockTasks.filter((t) => !["مكتملة", "ملغاة"].includes(t.status)).length;
}

export function getTaskStats() {
  const total = mockTasks.length;
  const completed = mockTasks.filter((t) => t.status === "مكتملة").length;
  const overdue = mockTasks.filter((t) => t.status === "متأخرة").length;
  const inProgress = mockTasks.filter((t) => t.status === "قيد التنفيذ").length;
  const newTasks = mockTasks.filter((t) => t.status === "جديدة").length;
  return { total, completed, overdue, inProgress, newTasks };
}

export function getStatusForBoard(status: TaskStatus): string {
  if (status === "جديدة") return "جديدة";
  if (status === "قيد التنفيذ") return "قيد التنفيذ";
  if (["بانتظار الموافقة", "بانتظار الموظف", "بانتظار الموارد البشرية", "بانتظار المالية"].includes(status)) return "بانتظار";
  if (status === "مكتملة") return "مكتملة";
  if (status === "متأخرة") return "متأخرة";
  if (status === "ملغاة") return "ملغاة";
  return "جديدة";
}
