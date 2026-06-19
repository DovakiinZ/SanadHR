import { apiFetch } from "../api-client";

export interface Shift {
  id: string;
  nameAr: string;
  nameEn: string;
  startTime: string;   // HH:mm
  endTime: string;     // HH:mm
  requiredMinutes: number;
  breakMinutes: number;
  graceBeforeStartMinutes: number;
  graceAfterStartMinutes: number;
  graceBeforeEndMinutes: number;
  graceAfterEndMinutes: number;
  overtimeAllowed: boolean;
  lateDeductionEnabled: boolean;
  isFlexible: boolean;
  weekendDays: string; // "5,6"
  isActive: boolean;
  assignedCount: number;
}

export interface ShiftInput {
  nameAr: string;
  nameEn: string;
  startTime: string;
  endTime: string;
  requiredMinutes: number;
  breakMinutes: number;
  graceBeforeStartMinutes: number;
  graceAfterStartMinutes: number;
  graceBeforeEndMinutes: number;
  graceAfterEndMinutes: number;
  overtimeAllowed: boolean;
  lateDeductionEnabled: boolean;
  isFlexible: boolean;
  weekendDays: string;
  isActive: boolean;
}

export interface ShiftAssignment {
  id: string;
  shiftId: string;
  shiftName?: string | null;
  employeeId?: string | null;
  employeeName?: string | null;
  departmentId?: string | null;
  departmentName?: string | null;
  branchId?: string | null;
  branchName?: string | null;
  jobTitleId?: string | null;
  jobTitleName?: string | null;
  effectiveFrom: string;
  effectiveTo?: string | null;
  priority: number;
  isActive: boolean;
}

export interface AssignShiftInput {
  shiftId: string;
  employeeIds: string[];
  departmentId?: string | null;
  branchId?: string | null;
  jobTitleId?: string | null;
  effectiveFrom: string;
  effectiveTo?: string | null;
  priority: number;
  isActive: boolean;
}

export async function listShifts(): Promise<Shift[]> {
  return (await apiFetch<Shift[]>("/api/shifts")) ?? [];
}

export function createShift(input: ShiftInput) {
  return apiFetch<Shift>("/api/shifts", { method: "POST", body: input });
}

export function updateShift(id: string, input: ShiftInput) {
  return apiFetch<Shift>(`/api/shifts/${id}`, { method: "PUT", body: input });
}

export function deleteShift(id: string) {
  return apiFetch<unknown>(`/api/shifts/${id}`, { method: "DELETE" });
}

export async function listShiftAssignments(shiftId?: string): Promise<ShiftAssignment[]> {
  const q = shiftId ? `?shiftId=${shiftId}` : "";
  return (await apiFetch<ShiftAssignment[]>(`/api/shifts/assignments${q}`)) ?? [];
}

export function assignShift(input: AssignShiftInput) {
  return apiFetch<number>("/api/shifts/assign", { method: "POST", body: input });
}

export function deleteShiftAssignment(id: string) {
  return apiFetch<unknown>(`/api/shifts/assignments/${id}`, { method: "DELETE" });
}

// Weekend day labels (DayOfWeek: Sun=0 .. Sat=6).
export const WEEKDAYS_AR = ["الأحد", "الاثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة", "السبت"];

export function weekendLabel(csv: string): string {
  return csv
    .split(",")
    .map((s) => s.trim())
    .filter(Boolean)
    .map((s) => WEEKDAYS_AR[Number(s)] ?? s)
    .join("، ");
}
