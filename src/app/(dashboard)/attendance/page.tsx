"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import {
  Loader2, Clock, ChevronRight, ChevronLeft, Download, Eye, Plus, Pencil,
  UserCheck, UserX, AlarmClock, FileWarning, TimerReset, TrendingUp, Settings2, Filter, X,
} from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { usePermissions } from "@/lib/permissions";
import { getDepartments, getBranches, type OrgOption, orgLabel } from "@/lib/api/org";
import { getEmployees } from "@/lib/api/employees";
import { listShifts, type Shift } from "@/lib/api/shifts";
import {
  getDailyAttendance, getWeeklyAttendance, getMonthlyAttendance, exportAttendance,
  syncAttendancePayrollImpact,
  fmtMinutes, fmtTime,
  ATTENDANCE_STATUS_AR, ATTENDANCE_STATUS_STYLE, ATTENDANCE_SOURCE_AR,
  type AttendanceFilters, type AttendanceDay, type AttendanceSummary, type AttendanceKpi,
} from "@/lib/api/attendance";
import { AttendanceDetailDrawer } from "@/components/attendance/attendance-detail-drawer";
import { PunchDialog } from "@/components/attendance/punch-dialog";
import { Employee } from "@/types";

type View = "daily" | "weekly" | "monthly";

const STATUS_OPTIONS = [
  "Present", "Absent", "Late", "OnLeave", "MissingCheckIn", "MissingCheckOut",
  "ShortHours", "Overtime", "Weekend", "Holiday", "WorkFromHome",
];

function todayIso() {
  return new Date().toISOString().slice(0, 10);
}

function StatusChip({ status }: { status: string }) {
  return (
    <span className={`inline-flex items-center rounded-md px-2 py-0.5 text-xs font-medium ${ATTENDANCE_STATUS_STYLE[status] ?? "bg-muted text-muted-foreground"}`}>
      {ATTENDANCE_STATUS_AR[status] ?? status}
    </span>
  );
}

function KpiCard({ label, value, icon: Icon, tone }: { label: string; value: number; icon: typeof UserCheck; tone: string }) {
  return (
    <div className="border border-border bg-card p-3">
      <div className="flex items-center justify-between">
        <p className="text-[0.7rem] font-bold text-muted-foreground">{label}</p>
        <div className={`flex h-7 w-7 items-center justify-center rounded-md ${tone}`}><Icon className="h-4 w-4" /></div>
      </div>
      <p className="mt-2 text-2xl font-bold tabular-nums">{value}</p>
    </div>
  );
}

export default function AttendancePage() {
  const { hasAny } = usePermissions();
  const canEdit = hasAny("Attendance.Edit", "Attendance.Create");
  const canExport = hasAny("Attendance.Export", "Attendance.View");
  const canPayrollImpact = hasAny("Attendance.PayrollImpact.Create");

  const [view, setView] = useState<View>("daily");
  const [anchor, setAnchor] = useState<string>(todayIso());

  // Target month for payroll impact sync (defaults to the currently viewed month)
  const payrollMonth = useMemo(() => {
    const d = new Date(anchor);
    return { year: d.getFullYear(), month: d.getMonth() + 1 };
  }, [anchor]);
  const [filters, setFilters] = useState<AttendanceFilters>({});
  const [showFilters, setShowFilters] = useState(false);

  const [dayRows, setDayRows] = useState<AttendanceDay[]>([]);
  const [summaryRows, setSummaryRows] = useState<AttendanceSummary[]>([]);
  const [kpis, setKpis] = useState<AttendanceKpi | null>(null);
  const [loading, setLoading] = useState(true);

  // Filter option sources
  const [departments, setDepartments] = useState<OrgOption[]>([]);
  const [branches, setBranches] = useState<OrgOption[]>([]);
  const [shifts, setShifts] = useState<Shift[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);

  const [drawerRow, setDrawerRow] = useState<AttendanceDay | null>(null);
  const [punch, setPunch] = useState<{ mode: "manual" | "correct"; row: AttendanceDay } | null>(null);

  useEffect(() => {
    getDepartments().then(setDepartments).catch(() => {});
    getBranches().then(setBranches).catch(() => {});
    listShifts().then(setShifts).catch(() => {});
    getEmployees({ pageSize: 500 }).then(setEmployees).catch(() => {});
  }, []);

  const jobTitles = useMemo(() => {
    const map = new Map<string, string>();
    employees.forEach((e) => { if (e.jobTitleId) map.set(e.jobTitleId, e.position); });
    return Array.from(map, ([id, name]) => ({ id, name }));
  }, [employees]);

  const apiFilters = useCallback((): AttendanceFilters => {
    const base: AttendanceFilters = { ...filters };
    if (view === "monthly") {
      const d = new Date(anchor);
      base.year = d.getFullYear();
      base.month = d.getMonth() + 1;
    } else {
      base.date = anchor;
    }
    return base;
  }, [filters, view, anchor]);

  const load = useCallback(() => {
    setLoading(true);
    const f = apiFilters();
    const run =
      view === "daily" ? getDailyAttendance(f).then((r) => { setDayRows(r.rows); setKpis(r.kpis); })
      : view === "weekly" ? getWeeklyAttendance(f).then((r) => { setSummaryRows(r.rows); setKpis(r.kpis); })
      : getMonthlyAttendance(f).then((r) => { setSummaryRows(r.rows); setKpis(r.kpis); });
    run.catch(() => { setDayRows([]); setSummaryRows([]); setKpis(null); }).finally(() => setLoading(false));
  }, [apiFilters, view]);

  useEffect(() => { load(); }, [load]);

  function step(dir: number) {
    const d = new Date(anchor);
    if (view === "daily") d.setDate(d.getDate() + dir);
    else if (view === "weekly") d.setDate(d.getDate() + dir * 7);
    else d.setMonth(d.getMonth() + dir);
    setAnchor(d.toISOString().slice(0, 10));
  }

  const rangeLabel = useMemo(() => {
    const d = new Date(anchor);
    if (view === "monthly") return d.toLocaleDateString("ar", { month: "long", year: "numeric" });
    if (view === "weekly") {
      const start = new Date(d); start.setDate(d.getDate() - d.getDay());
      const end = new Date(start); end.setDate(start.getDate() + 6);
      return `${start.toISOString().slice(0, 10)} ← ${end.toISOString().slice(0, 10)}`;
    }
    return d.toLocaleDateString("ar", { weekday: "long", day: "numeric", month: "long", year: "numeric" });
  }, [anchor, view]);

  async function doExport() {
    try {
      const v = view === "daily" ? "daily" : view;
      await exportAttendance(apiFilters(), v);
    } catch (e) {
      toast.error((e as Error)?.message || "تعذر التصدير");
    }
  }

  function setF(key: keyof AttendanceFilters, value: string) {
    setFilters((p) => ({ ...p, [key]: value || undefined }));
  }

  const activeFilterCount = Object.values(filters).filter(Boolean).length;

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold">الحضور والانصراف</h1>
          <p className="mt-1 text-sm text-muted-foreground">محرك الحضور اليومي والأسبوعي والشهري لجميع الموظفين</p>
        </div>
        <div className="flex items-center gap-2">
          <Link href="/settings/attendance/shifts">
            <Button variant="outline" size="sm"><Settings2 /> إعدادات الورديات</Button>
          </Link>
          {canExport && <Button variant="outline" size="sm" onClick={doExport}><Download /> تصدير Excel</Button>}
        </div>
      </div>

      {/* View switch + date nav */}
      <div className="flex flex-wrap items-center justify-between gap-3 border border-border bg-card p-2">
        <div className="flex items-center gap-1 rounded-lg bg-muted p-0.5">
          {(["daily", "weekly", "monthly"] as View[]).map((v) => (
            <button
              key={v}
              onClick={() => setView(v)}
              className={`rounded-md px-3 py-1 text-sm font-medium transition-colors ${view === v ? "bg-background shadow-sm" : "text-muted-foreground hover:text-foreground"}`}
            >
              {v === "daily" ? "يومي" : v === "weekly" ? "أسبوعي" : "شهري"}
            </button>
          ))}
        </div>

        <div className="flex items-center gap-2">
          <Button variant="ghost" size="icon-sm" onClick={() => step(-1)}><ChevronRight /></Button>
          <span className="min-w-[12rem] text-center text-sm font-medium">{rangeLabel}</span>
          <Button variant="ghost" size="icon-sm" onClick={() => step(1)}><ChevronLeft /></Button>
          <input
            type="date"
            value={anchor}
            onChange={(e) => setAnchor(e.target.value || todayIso())}
            className="h-7 rounded-lg border border-border bg-background px-2 text-xs tabular-nums"
          />
          <Button variant={activeFilterCount ? "default" : "outline"} size="sm" onClick={() => setShowFilters((s) => !s)}>
            <Filter /> تصفية {activeFilterCount > 0 && `(${activeFilterCount})`}
          </Button>
        </div>
      </div>

      {/* Filters */}
      {showFilters && (
        <div className="flex flex-wrap items-center gap-2 border border-border bg-card p-3">
          <select value={filters.employeeId ?? ""} onChange={(e) => setF("employeeId", e.target.value)} className="h-8 rounded-lg border border-border bg-background px-2 text-xs">
            <option value="">كل الموظفين</option>
            {employees.map((e) => <option key={e.id} value={e.id}>{e.name}</option>)}
          </select>
          <select value={filters.departmentId ?? ""} onChange={(e) => setF("departmentId", e.target.value)} className="h-8 rounded-lg border border-border bg-background px-2 text-xs">
            <option value="">كل الإدارات</option>
            {departments.map((d) => <option key={d.id} value={d.id}>{orgLabel(d)}</option>)}
          </select>
          <select value={filters.branchId ?? ""} onChange={(e) => setF("branchId", e.target.value)} className="h-8 rounded-lg border border-border bg-background px-2 text-xs">
            <option value="">كل الفروع</option>
            {branches.map((b) => <option key={b.id} value={b.id}>{orgLabel(b)}</option>)}
          </select>
          <select value={filters.jobTitleId ?? ""} onChange={(e) => setF("jobTitleId", e.target.value)} className="h-8 rounded-lg border border-border bg-background px-2 text-xs">
            <option value="">كل المسميات</option>
            {jobTitles.map((j) => <option key={j.id} value={j.id}>{j.name}</option>)}
          </select>
          <select value={filters.shiftId ?? ""} onChange={(e) => setF("shiftId", e.target.value)} className="h-8 rounded-lg border border-border bg-background px-2 text-xs">
            <option value="">كل الورديات</option>
            {shifts.map((s) => <option key={s.id} value={s.id}>{s.nameAr}</option>)}
          </select>
          <select value={filters.status ?? ""} onChange={(e) => setF("status", e.target.value)} className="h-8 rounded-lg border border-border bg-background px-2 text-xs">
            <option value="">كل الحالات</option>
            {STATUS_OPTIONS.map((s) => <option key={s} value={s}>{ATTENDANCE_STATUS_AR[s]}</option>)}
          </select>
          {activeFilterCount > 0 && (
            <button onClick={() => setFilters({})} className="flex items-center gap-1 text-xs text-muted-foreground hover:text-foreground">
              <X className="h-3.5 w-3.5" /> مسح
            </button>
          )}
        </div>
      )}

      {/* KPIs */}
      {kpis && (
        <div className="grid grid-cols-2 gap-2 sm:grid-cols-3 lg:grid-cols-6">
          <KpiCard label="حاضر" value={kpis.present} icon={UserCheck} tone="bg-green-500/10 text-green-600" />
          <KpiCard label="غائب" value={kpis.absent} icon={UserX} tone="bg-red-500/10 text-red-600" />
          <KpiCard label="متأخر" value={kpis.late} icon={AlarmClock} tone="bg-amber-500/10 text-amber-600" />
          <KpiCard label="بصمات ناقصة" value={kpis.missingPunches} icon={FileWarning} tone="bg-orange-500/10 text-orange-600" />
          <KpiCard label="ساعات ناقصة" value={kpis.shortHours} icon={TimerReset} tone="bg-yellow-500/10 text-yellow-700" />
          <KpiCard label="وقت إضافي" value={kpis.overtime} icon={TrendingUp} tone="bg-indigo-500/10 text-indigo-600" />
        </div>
      )}

      {/* Table */}
      {loading ? (
        <div className="flex h-64 items-center justify-center"><Loader2 className="h-6 w-6 animate-spin text-muted-foreground" /></div>
      ) : view === "daily" ? (
        <DailyTable rows={dayRows} canEdit={canEdit} canPayrollImpact={canPayrollImpact} payrollYear={payrollMonth.year} payrollMonth={payrollMonth.month} onView={setDrawerRow} onPunch={setPunch} />
      ) : (
        <SummaryTable rows={summaryRows} />
      )}

      <AttendanceDetailDrawer row={drawerRow} open={!!drawerRow} onClose={() => setDrawerRow(null)} />
      {punch && (
        <PunchDialog
          mode={punch.mode}
          row={punch.row}
          open
          onClose={() => setPunch(null)}
          onSaved={load}
        />
      )}
    </div>
  );
}

function DailyTable({
  rows, canEdit, canPayrollImpact, payrollYear, payrollMonth, onView, onPunch,
}: {
  rows: AttendanceDay[];
  canEdit: boolean;
  canPayrollImpact: boolean;
  payrollYear: number;
  payrollMonth: number;
  onView: (r: AttendanceDay) => void;
  onPunch: (p: { mode: "manual" | "correct"; row: AttendanceDay }) => void;
}) {
  async function syncImpact(row: AttendanceDay, includeOvertime: boolean) {
    try {
      const r = await syncAttendancePayrollImpact({
        employeeId: row.employeeId,
        year: payrollYear,
        month: payrollMonth,
        includeOvertime,
      });
      toast.success(`تم الاحتساب — أُنشئ ${r.created}، حُدِّث ${r.updated}، أُزيل ${r.removed}، تجاوز ${r.skippedPosted}`);
    } catch {
      // errors already toasted by apiFetch
    }
  }
  if (rows.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center border border-dashed border-border p-12 text-center">
        <Clock className="mb-3 h-10 w-10 text-muted-foreground" />
        <p className="text-sm text-muted-foreground">لا توجد سجلات حضور مطابقة</p>
      </div>
    );
  }
  return (
    <div className="overflow-x-auto border border-border bg-card">
      <table className="w-full min-w-[1000px] text-sm">
        <thead>
          <tr className="border-b border-border text-right text-xs text-muted-foreground">
            <th className="px-3 py-2 font-medium">الموظف</th>
            <th className="px-3 py-2 font-medium">الوردية</th>
            <th className="px-3 py-2 font-medium">الحضور</th>
            <th className="px-3 py-2 font-medium">الانصراف</th>
            <th className="px-3 py-2 font-medium">العمل</th>
            <th className="px-3 py-2 font-medium">المطلوب</th>
            <th className="px-3 py-2 font-medium">التأخير</th>
            <th className="px-3 py-2 font-medium">النقص</th>
            <th className="px-3 py-2 font-medium">الإضافي</th>
            <th className="px-3 py-2 font-medium">الحالة</th>
            <th className="px-3 py-2 font-medium">المصدر</th>
            <th className="px-3 py-2 font-medium"></th>
          </tr>
        </thead>
        <tbody>
          {rows.map((r, i) => (
            <tr key={(r.recordId ?? "v") + r.employeeId + i} className="border-b border-border/40 hover:bg-muted/30">
              <td className="px-3 py-2">
                <div className="font-medium">{r.employeeName ?? "—"}</div>
                <div className="text-xs text-muted-foreground">{r.departmentName ?? "—"}</div>
              </td>
              <td className="px-3 py-2 text-xs text-muted-foreground">{r.shiftName ?? "—"}</td>
              <td className="px-3 py-2 tabular-nums">{fmtTime(r.checkIn)}</td>
              <td className="px-3 py-2 tabular-nums">{fmtTime(r.checkOut)}</td>
              <td className="px-3 py-2 tabular-nums">{fmtMinutes(r.workedMinutes)}</td>
              <td className="px-3 py-2 tabular-nums text-muted-foreground">{fmtMinutes(r.requiredMinutes)}</td>
              <td className={`px-3 py-2 tabular-nums ${r.lateMinutes ? "text-amber-600" : "text-muted-foreground"}`}>{r.lateMinutes ? fmtMinutes(r.lateMinutes) : "—"}</td>
              <td className={`px-3 py-2 tabular-nums ${r.shortageMinutes ? "text-red-600" : "text-muted-foreground"}`}>{r.shortageMinutes ? fmtMinutes(r.shortageMinutes) : "—"}</td>
              <td className={`px-3 py-2 tabular-nums ${r.overtimeMinutes ? "text-indigo-600" : "text-muted-foreground"}`}>{r.overtimeMinutes ? fmtMinutes(r.overtimeMinutes) : "—"}</td>
              <td className="px-3 py-2"><StatusChip status={r.status} /></td>
              <td className="px-3 py-2 text-xs text-muted-foreground">{r.source ? (ATTENDANCE_SOURCE_AR[r.source] ?? r.source) : "—"}</td>
              <td className="px-3 py-2">
                <div className="flex items-center justify-end gap-1">
                  <Button variant="ghost" size="icon-xs" title="عرض التفاصيل" onClick={() => onView(r)}><Eye /></Button>
                  {canEdit && <Button variant="ghost" size="icon-xs" title="بصمة يدوية" onClick={() => onPunch({ mode: "manual", row: r })}><Plus /></Button>}
                  {canEdit && r.recordId && <Button variant="ghost" size="icon-xs" title="تصحيح" onClick={() => onPunch({ mode: "correct", row: r })}><Pencil /></Button>}
                  {canPayrollImpact && (
                    <>
                      <Button variant="ghost" size="icon-xs" title="احتساب خصم الغياب" onClick={() => syncImpact(r, false)}>
                        <UserX className="h-3.5 w-3.5 text-red-500" />
                      </Button>
                      <Button variant="ghost" size="icon-xs" title="احتساب خصم التأخير" onClick={() => syncImpact(r, false)}>
                        <AlarmClock className="h-3.5 w-3.5 text-amber-500" />
                      </Button>
                      <Button variant="ghost" size="icon-xs" title="احتساب خصم النقص" onClick={() => syncImpact(r, false)}>
                        <TimerReset className="h-3.5 w-3.5 text-yellow-600" />
                      </Button>
                      <Button variant="ghost" size="icon-xs" title="احتساب إضافة الوقت الإضافي" onClick={() => syncImpact(r, true)}>
                        <TrendingUp className="h-3.5 w-3.5 text-indigo-500" />
                      </Button>
                    </>
                  )}
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function SummaryTable({ rows }: { rows: AttendanceSummary[] }) {
  if (rows.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center border border-dashed border-border p-12 text-center">
        <Clock className="mb-3 h-10 w-10 text-muted-foreground" />
        <p className="text-sm text-muted-foreground">لا توجد بيانات للفترة المحددة</p>
      </div>
    );
  }
  return (
    <div className="overflow-x-auto border border-border bg-card">
      <table className="w-full min-w-[900px] text-sm">
        <thead>
          <tr className="border-b border-border text-right text-xs text-muted-foreground">
            <th className="px-3 py-2 font-medium">الموظف</th>
            <th className="px-3 py-2 font-medium">حضور</th>
            <th className="px-3 py-2 font-medium">غياب</th>
            <th className="px-3 py-2 font-medium">إجازة</th>
            <th className="px-3 py-2 font-medium">تأخير</th>
            <th className="px-3 py-2 font-medium">ساعات ناقصة</th>
            <th className="px-3 py-2 font-medium">إضافي</th>
            <th className="px-3 py-2 font-medium">إجمالي العمل</th>
            <th className="px-3 py-2 font-medium">المطلوب</th>
            <th className="px-3 py-2 font-medium">إجمالي التأخير</th>
            <th className="px-3 py-2 font-medium">إجمالي الإضافي</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((r) => (
            <tr key={r.employeeId} className="border-b border-border/40 hover:bg-muted/30">
              <td className="px-3 py-2">
                <div className="font-medium">{r.employeeName ?? "—"}</div>
                <div className="text-xs text-muted-foreground">{r.departmentName ?? "—"}</div>
              </td>
              <td className="px-3 py-2 tabular-nums text-green-600">{r.presentDays}</td>
              <td className="px-3 py-2 tabular-nums text-red-600">{r.absentDays}</td>
              <td className="px-3 py-2 tabular-nums text-blue-600">{r.leaveDays}</td>
              <td className="px-3 py-2 tabular-nums text-amber-600">{r.lateDays}</td>
              <td className="px-3 py-2 tabular-nums">{r.shortDays}</td>
              <td className="px-3 py-2 tabular-nums text-indigo-600">{r.overtimeDays}</td>
              <td className="px-3 py-2 tabular-nums">{fmtMinutes(r.workedMinutes)}</td>
              <td className="px-3 py-2 tabular-nums text-muted-foreground">{fmtMinutes(r.requiredMinutes)}</td>
              <td className="px-3 py-2 tabular-nums">{fmtMinutes(r.lateMinutes)}</td>
              <td className="px-3 py-2 tabular-nums">{fmtMinutes(r.overtimeMinutes)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
