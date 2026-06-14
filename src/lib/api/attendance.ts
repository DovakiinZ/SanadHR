import { apiFetch } from "../api-client";

export interface AttendanceRecord {
  id: string;
  employeeId: string;
  employeeName?: string | null;
  date: string;
  status: string;       // Present | Absent | OnLeave | …
  checkIn?: string | null;
  checkOut?: string | null;
  source?: string | null;
  notes?: string | null;
}

export function getAttendance(opts?: { scope?: "mine" | "all"; employeeId?: string; year?: number; month?: number }) {
  const q = new URLSearchParams();
  if (opts?.scope === "all") q.set("scope", "all");
  if (opts?.employeeId) q.set("employeeId", opts.employeeId);
  if (opts?.year) q.set("year", String(opts.year));
  if (opts?.month) q.set("month", String(opts.month));
  const qs = q.toString();
  return apiFetch<AttendanceRecord[]>(`/api/attendance${qs ? `?${qs}` : ""}`);
}

export const ATTENDANCE_STATUS_AR: Record<string, string> = {
  Present: "حاضر", Absent: "غائب", OnLeave: "إجازة", Holiday: "عطلة",
  Weekend: "نهاية أسبوع", Late: "متأخر", Remote: "عن بُعد",
};
