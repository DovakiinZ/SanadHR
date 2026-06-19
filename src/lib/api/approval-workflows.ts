import { apiFetch } from "../api-client";

// ── Business-user approval workflows (api/approval-workflows) ──
// A workflow is a flat ordered chain of approval steps. It is stored server-side as the
// WorkflowChainConfig the Request Center engine executes; this client never deals with GUIDs/graphs.

// Mirrors HR.Domain.Enums.ApproverType. Only the business-friendly subset is offered in the wizard.
export enum ApproverType {
  SpecificUser = 1,
  DirectManager = 2,
  DepartmentHead = 3,
  BranchManager = 4,
  HrManager = 5,
  Role = 6,
  ManagerChain = 7,
}

export interface ApprovalCondition {
  field: string;
  operator: string; // eq, neq, gt, gte, lt, lte, contains
  value: string;
}

export interface ApprovalWorkflowStep {
  approverType: number;
  nameAr: string;
  nameEn: string;
  specificEntityId: string | null; // employee id / role id depending on approverType
  chainLevel: number;
  required: boolean;
  canReject: boolean;
  canReturn: boolean;
  canDelegate: boolean;
  conditions: ApprovalCondition[];
}

export interface ApprovalWorkflowDetail {
  id: string;
  code: string;
  name: string;
  description: string | null;
  isActive: boolean;
  steps: ApprovalWorkflowStep[];
  requestTypeIds: string[];
}

export interface ApprovalWorkflowListItem {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  stepCount: number;
  assignedRequestTypes: string[];
}

export interface UpsertApprovalWorkflow {
  name: string;
  description?: string | null;
  isActive: boolean;
  steps: ApprovalWorkflowStep[];
  requestTypeIds: string[];
}

const BASE = "/api/approval-workflows";

export const listApprovalWorkflows = () => apiFetch<ApprovalWorkflowListItem[]>(BASE);
export const getApprovalWorkflow = (id: string) => apiFetch<ApprovalWorkflowDetail>(`${BASE}/${id}`);
export const createApprovalWorkflow = (body: UpsertApprovalWorkflow) =>
  apiFetch<ApprovalWorkflowDetail>(BASE, { method: "POST", body });
export const updateApprovalWorkflow = (id: string, body: UpsertApprovalWorkflow) =>
  apiFetch<ApprovalWorkflowDetail>(`${BASE}/${id}`, { method: "PUT", body });
export const duplicateApprovalWorkflow = (id: string) =>
  apiFetch<ApprovalWorkflowDetail>(`${BASE}/${id}/duplicate`, { method: "POST" });
export const deleteApprovalWorkflow = (id: string) =>
  apiFetch<unknown>(`${BASE}/${id}`, { method: "DELETE" });

// ── Request-type admin (assignment + activation) ──
export interface RequestTypeAdmin {
  id: string;
  code: string;
  nameAr: string;
  nameEn: string;
  categoryId: string | null;
  isActive: boolean;
  isSystem: boolean;
  formDefinitionId: string;
  workflowDefinitionId: string | null;
  workflowName: string | null;
  printTemplateId: string | null;
  printTemplateName: string | null;
  activationReady: boolean;
}

export const getRequestTypesAdmin = () => apiFetch<RequestTypeAdmin[]>(`/api/requests/types/admin`);
export const setRequestTypeWorkflow = (typeId: string, workflowDefinitionId: string | null) =>
  apiFetch<unknown>(`/api/requests/types/${typeId}/workflow`, { method: "PUT", body: { workflowDefinitionId } });
export const setRequestTypeActive = (typeId: string, isActive: boolean) =>
  apiFetch<unknown>(`/api/requests/types/${typeId}/active`, { method: "PUT", body: { isActive } });
export const setRequestTypePrintTemplate = (typeId: string, templateId: string | null) =>
  apiFetch<unknown>(`/api/requests/types/${typeId}/print-template`, { method: "PUT", body: { templateId } });

// ── Display metadata for the wizard (no free text — everything is a labelled choice) ──
export const APPROVER_TYPES: { value: ApproverType; label: string; needs: "none" | "employee" | "role" | "level" }[] = [
  { value: ApproverType.DirectManager, label: "المدير المباشر", needs: "none" },
  { value: ApproverType.DepartmentHead, label: "مدير الإدارة", needs: "none" },
  { value: ApproverType.HrManager, label: "الموارد البشرية", needs: "none" },
  { value: ApproverType.Role, label: "دور / فريق محدد (مالية، الرئيس التنفيذي…)", needs: "role" },
  { value: ApproverType.SpecificUser, label: "موظف محدد", needs: "employee" },
  { value: ApproverType.ManagerChain, label: "سلسلة المدراء (حسب المستوى)", needs: "level" },
];

export function approverTypeLabel(t: number): string {
  return APPROVER_TYPES.find((a) => a.value === t)?.label ?? "غير محدد";
}

export const CONDITION_FIELDS: { value: string; label: string; kind: "number" | "department" | "branch" | "leaveType" | "employmentType" | "jobTitle" }[] = [
  { value: "leaveDays", label: "عدد أيام الإجازة", kind: "number" },
  { value: "amount", label: "المبلغ", kind: "number" },
  { value: "department", label: "الإدارة", kind: "department" },
  { value: "branch", label: "الفرع", kind: "branch" },
  { value: "leaveType", label: "نوع الإجازة", kind: "leaveType" },
  { value: "employmentType", label: "نوع التوظيف", kind: "employmentType" },
  { value: "jobTitle", label: "المسمى الوظيفي", kind: "jobTitle" },
];

export const CONDITION_OPERATORS: { value: string; label: string }[] = [
  { value: "eq", label: "يساوي" },
  { value: "neq", label: "لا يساوي" },
  { value: "gt", label: "أكبر من" },
  { value: "gte", label: "أكبر من أو يساوي" },
  { value: "lt", label: "أصغر من" },
  { value: "lte", label: "أصغر من أو يساوي" },
  { value: "contains", label: "يحتوي على" },
];
