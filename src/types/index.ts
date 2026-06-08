export interface Employee {
  id: string;
  employeeId: string;
  name: string;
  email: string;
  phone: string;
  nationalId: string;
  dateOfBirth: string;
  gender: "ذكر" | "أنثى";
  nationality: string;
  department: string;
  position: string;
  joinDate: string;
  contractType: "دوام كامل" | "دوام جزئي" | "عقد مؤقت";
  salary: number;
  status: "نشط" | "إجازة" | "منتهي العقد";
  avatar?: string;
  leaveBalance: number;
  attendanceRate: number;
  // Governed reference ids (for prefilling edit-form dropdowns)
  jobTitleId?: string | null;
  nationalityId?: string | null;
  contractTypeId?: string | null;
  departmentId?: string | null;
  branchId?: string | null;
  managerId?: string | null;
}

export interface Department {
  id: string;
  name: string;
  employeeCount: number;
  manager: string;
}

export interface NavItem {
  label: string;
  href: string;
  icon: string;
}

// ── Tasks Module Types ──

export type TaskStatus =
  | "جديدة"
  | "قيد التنفيذ"
  | "بانتظار الموافقة"
  | "بانتظار الموظف"
  | "بانتظار الموارد البشرية"
  | "بانتظار المالية"
  | "مكتملة"
  | "ملغاة"
  | "متأخرة";

export type TaskPriority = "منخفضة" | "متوسطة" | "عالية" | "عاجلة" | "حرجة";

export type TaskSource =
  | "يدوي"
  | "سير العمل"
  | "طلب"
  | "انتهاء مستند"
  | "الرواتب"
  | "الحضور"
  | "تهيئة موظف"
  | "إنهاء خدمات"
  | "التوظيف"
  | "أتمتة النظام";

export interface TaskChecklistItem {
  id: string;
  title: string;
  completed: boolean;
  assignedTo?: string;
  dueDate?: string;
  completedBy?: string;
  completedAt?: string;
}

export interface TaskComment {
  id: string;
  author: string;
  content: string;
  createdAt: string;
}

export interface TaskAttachment {
  id: string;
  name: string;
  size: string;
  type: string;
  uploadedBy: string;
  uploadedAt: string;
}

export interface TaskActivityLog {
  id: string;
  action: string;
  user: string;
  timestamp: string;
  details?: string;
}

export interface Task {
  id: string;
  taskId: string;
  title: string;
  description: string;
  status: TaskStatus;
  priority: TaskPriority;
  source: TaskSource;
  assignee: string;
  createdBy: string;
  department: string;
  branch?: string;
  relatedEmployee?: string;
  relatedRequest?: string;
  relatedDocument?: string;
  relatedPayrollRun?: string;
  relatedCandidate?: string;
  dueDate: string;
  reminderDate?: string;
  tags: string[];
  checklist: TaskChecklistItem[];
  comments: TaskComment[];
  attachments: TaskAttachment[];
  activityLog: TaskActivityLog[];
  createdAt: string;
  updatedAt: string;
}

export interface TaskTemplate {
  id: string;
  name: string;
  description: string;
  relatedModule: string;
  automationTrigger?: string;
  items: TaskTemplateItem[];
  createdAt: string;
}

export interface TaskTemplateItem {
  id: string;
  title: string;
  defaultAssignee: string;
  relativeDueDays: number;
  priority: TaskPriority;
  checklist: string[];
}
