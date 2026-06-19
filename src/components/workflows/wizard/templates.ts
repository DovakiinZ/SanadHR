import { ApproverType, type ApprovalWorkflowStep } from "@/lib/api/approval-workflows";

// Ready-made approval chains an HR officer can start from and tweak. Each step uses a generic
// approver TYPE (no specific person) so a template works for any tenant out of the box.

function step(approverType: ApproverType, nameAr: string, nameEn: string): ApprovalWorkflowStep {
  return {
    approverType, nameAr, nameEn, specificEntityId: null, chainLevel: 1,
    required: true, canReject: true, canReturn: true, canDelegate: false, conditions: [],
  };
}

const MANAGER = () => step(ApproverType.DirectManager, "المدير المباشر", "Direct Manager");
const DEPT = () => step(ApproverType.DepartmentHead, "مدير الإدارة", "Department Manager");
const HR = () => step(ApproverType.HrManager, "الموارد البشرية", "HR");
const FINANCE = () => ({ ...step(ApproverType.Role, "المالية", "Finance"), }); // role chosen on save

export interface WorkflowTemplate {
  key: string;
  nameAr: string;
  descriptionAr: string;
  steps: ApprovalWorkflowStep[];
}

export const WORKFLOW_TEMPLATES: WorkflowTemplate[] = [
  { key: "annual-leave", nameAr: "اعتماد الإجازة السنوية", descriptionAr: "المدير المباشر ثم الموارد البشرية", steps: [MANAGER(), HR()] },
  { key: "sick-leave", nameAr: "اعتماد الإجازة المرضية", descriptionAr: "المدير المباشر ثم الموارد البشرية", steps: [MANAGER(), HR()] },
  { key: "business-trip", nameAr: "اعتماد رحلة عمل", descriptionAr: "المدير المباشر ← مدير الإدارة ← الموارد البشرية ← المالية", steps: [MANAGER(), DEPT(), HR(), FINANCE()] },
  { key: "expense-claim", nameAr: "اعتماد مطالبة مصروفات", descriptionAr: "المدير المباشر ثم المالية", steps: [MANAGER(), FINANCE()] },
  { key: "loan-request", nameAr: "اعتماد طلب سلفة", descriptionAr: "المدير المباشر ← المالية", steps: [MANAGER(), FINANCE()] },
  { key: "salary-certificate", nameAr: "شهادة راتب", descriptionAr: "الموارد البشرية فقط", steps: [HR()] },
  { key: "attendance-adjustment", nameAr: "تعديل حضور", descriptionAr: "المدير المباشر", steps: [MANAGER()] },
  { key: "overtime", nameAr: "طلب عمل إضافي", descriptionAr: "المدير المباشر", steps: [MANAGER()] },
  { key: "remote-work", nameAr: "طلب عمل عن بُعد", descriptionAr: "المدير المباشر ثم الموارد البشرية", steps: [MANAGER(), HR()] },
  { key: "training", nameAr: "طلب تدريب", descriptionAr: "المدير المباشر ثم المالية", steps: [MANAGER(), FINANCE()] },
  { key: "purchase", nameAr: "طلب شراء", descriptionAr: "المدير المباشر ← مدير الإدارة ← المالية", steps: [MANAGER(), DEPT(), FINANCE()] },
  { key: "employee-data-update", nameAr: "تحديث بيانات موظف", descriptionAr: "الموارد البشرية فقط", steps: [HR()] },
  { key: "recruitment", nameAr: "طلب توظيف", descriptionAr: "مدير الإدارة ← الموارد البشرية ← المالية", steps: [DEPT(), HR(), FINANCE()] },
];
