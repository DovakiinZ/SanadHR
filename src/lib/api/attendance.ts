import { apiFetch, API_BASE_URL } from "../api-client";
import { getAccessToken } from "../auth-storage";

// ── Types (mirror HR.Modules.Attendance.DTOs) ──────────────────────────────

export interface AttendanceDay {
  recordId?: string | null;
  employeeId: string;
  employeeName?: string | null;
  employeeNumber?: string | null;
  departmentName?: string | null;
  branchName?: string | null;
  jobTitleName?: string | null;
  date: string;
  shiftId?: string | null;
  shiftName?: string | null;
  isFlexible: boolean;
  checkIn?: string | null;
  checkOut?: string | null;
  requiredMinutes: number;
  workedMinutes: number;
  lateMinutes: number;
  shortageMinutes: number;
  overtimeMinutes: number;
  breakMinutes: number;
  status: string;
  source?: string | null;
  referenceId?: string | null;
  notes?: string | null;
}

export interface AttendanceKpi {
  total: number;
  present: number;
  absent: number;
  late: number;
  onLeave: number;
  missingPunches: number;
  shortHours: number;
  overtime: number;
  weekend: number;
  holiday: number;
}

export interface AttendanceDailyResponse {
  date: string;
  kpis: AttendanceKpi;
  rows: AttendanceDay[];
}

export interface AttendanceSummary {
  employeeId: string;
  employeeName?: string | null;
  employeeNumber?: string | null;
  departmentName?: string | null;
  branchName?: string | null;
  presentDays: number;
  absentDays: number;
  leaveDays: number;
  lateDays: number;
  shortDays: number;
  overtimeDays: number;
  weekendDays: number;
  holidayDays: number;
  missingPunchDays: number;
  workedMinutes: number;
  requiredMinutes: number;
  lateMinutes: number;
  shortageMinutes: number;
  overtimeMinutes: number;
}

export interface AttendanceSummaryResponse {
  from: string;
  to: string;
  kpis: AttendanceKpi;
  rows: AttendanceSummary[];
}

export interface AttendancePunchView {
  id: string;
  punchTime: string;
  direction: string;
  source: string;
  latitude?: number | null;
  longitude?: number | null;
  notes?: string | null;
}

export interface AttendanceAuditView {
  action: string;
  details?: string | null;
  at: string;
}

export interface AttendanceDetail {
  day: AttendanceDay;
  punches: AttendancePunchView[];
  audit: AttendanceAuditView[];
  relatedRequestId?: string | null;
  relatedRequestNumber?: string | null;
}

export interface AttendanceFilters {
  date?: string;
  from?: string;
  to?: string;
  year?: number;
  month?: number;
  employeeId?: string;
  departmentId?: string;
  branchId?: string;
  jobTitleId?: string;
  shiftId?: string;
  status?: string;
}

function qs(filters: AttendanceFilters): string {
  const q = new URLSearchParams();
  Object.entries(filters).forEach(([k, v]) => {
    if (v !== undefined && v !== null && v !== "") q.set(k, String(v));
  });
  const s = q.toString();
  return s ? `?${s}` : "";
}

export function getDailyAttendance(filters: AttendanceFilters = {}) {
  return apiFetch<AttendanceDailyResponse>(`/api/attendance/daily${qs(filters)}`);
}

export function getWeeklyAttendance(filters: AttendanceFilters = {}) {
  return apiFetch<AttendanceSummaryResponse>(`/api/attendance/weekly${qs(filters)}`);
}

export function getMonthlyAttendance(filters: AttendanceFilters = {}) {
  return apiFetch<AttendanceSummaryResponse>(`/api/attendance/monthly${qs(filters)}`);
}

export function getAttendanceDetail(id: string) {
  return apiFetch<AttendanceDetail>(`/api/attendance/${id}`);
}

export interface ManualPunchInput {
  employeeId: string;
  date: string;       // yyyy-MM-dd
  checkIn?: string;   // HH:mm
  checkOut?: string;  // HH:mm
  notes?: string;
}

export function addManualPunch(body: ManualPunchInput) {
  return apiFetch<string>("/api/attendance/manual-punch", { method: "POST", body });
}

export interface CorrectAttendanceInput {
  checkIn?: string;   // HH:mm
  checkOut?: string;  // HH:mm
  reason?: string;
}

export function correctAttendance(id: string, body: CorrectAttendanceInput) {
  return apiFetch<unknown>(`/api/attendance/${id}/correct`, { method: "PUT", body });
}

/** Download the attendance .xlsx. view = daily | range | weekly | monthly. */
export async function exportAttendance(filters: AttendanceFilters, view: string): Promise<void> {
  const token = getAccessToken();
  const params = qs({ ...filters }).replace(/^\?/, "");
  const sep = params ? "&" : "";
  const res = await fetch(`${API_BASE_URL}/api/attendance/export?view=${view}${sep}${params}`, {
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  });
  if (!res.ok) throw new Error("تعذر تصدير الحضور");
  const blob = await res.blob();
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = `attendance-${view}-${new Date().toISOString().slice(0, 10)}.xlsx`;
  document.body.appendChild(a);
  a.click();
  a.remove();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}

// ── Display helpers ─────────────────────────────────────────────────────────

export const ATTENDANCE_STATUS_AR: Record<string, string> = {
  Present: "حاضر",
  Absent: "غائب",
  OnLeave: "إجازة",
  Holiday: "عطلة رسمية",
  Weekend: "نهاية أسبوع",
  Late: "متأخر",
  Remote: "عن بُعد",
  MissingCheckIn: "حضور ناقص",
  MissingCheckOut: "انصراف ناقص",
  ShortHours: "ساعات ناقصة",
  Overtime: "وقت إضافي",
  WorkFromHome: "عمل عن بُعد",
  Scheduled: "مجدول",
};

// Tailwind chip classes per status.
export const ATTENDANCE_STATUS_STYLE: Record<string, string> = {
  Present: "bg-green-500/10 text-green-600",
  Absent: "bg-red-500/10 text-red-600",
  OnLeave: "bg-blue-500/10 text-blue-600",
  Holiday: "bg-purple-500/10 text-purple-600",
  Weekend: "bg-muted text-muted-foreground",
  Late: "bg-amber-500/10 text-amber-600",
  Remote: "bg-cyan-500/10 text-cyan-600",
  MissingCheckIn: "bg-orange-500/10 text-orange-600",
  MissingCheckOut: "bg-orange-500/10 text-orange-600",
  ShortHours: "bg-yellow-500/10 text-yellow-700",
  Overtime: "bg-indigo-500/10 text-indigo-600",
  WorkFromHome: "bg-cyan-500/10 text-cyan-600",
  Scheduled: "bg-muted text-muted-foreground",
};

export const ATTENDANCE_SOURCE_AR: Record<string, string> = {
  Biometric: "بصمة",
  MobileGeofence: "تطبيق الجوال",
  WebCheckIn: "تسجيل ويب",
  ManualEntry: "إدخال يدوي",
  ApprovedRequest: "طلب معتمد",
  LeaveRequest: "طلب إجازة",
  MissingPunch: "بصمة ناقصة",
  AttendanceCorrection: "تصحيح حضور",
};

/** Minutes → "6h 45m" (Arabic). */
export function fmtMinutes(m: number): string {
  if (!m) return "0";
  const h = Math.floor(m / 60);
  const mm = m % 60;
  if (h && mm) return `${h}س ${mm}د`;
  if (h) return `${h}س`;
  return `${mm}د`;
}

/** ISO time → "HH:mm" (Arabic-Indic off, 24h). */
export function fmtTime(s?: string | null): string {
  if (!s) return "—";
  const d = new Date(s);
  return isNaN(d.getTime()) ? "—" : d.toLocaleTimeString("en-GB", { hour: "2-digit", minute: "2-digit" });
}
