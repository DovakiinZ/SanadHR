"use client";

import { useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { Loader2, Plus, Pencil, Trash2, Clock, ChevronRight, CalendarClock, CalendarOff, Save } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { usePermissions } from "@/lib/permissions";
import { getDepartments, getBranches, type OrgOption, orgLabel } from "@/lib/api/org";
import { getEmployees } from "@/lib/api/employees";
import { Employee } from "@/types";
import {
  listShifts, deleteShift,
  listShiftAssignments, assignShift, deleteShiftAssignment,
  weekendLabel,
  type Shift, type ShiftAssignment, type AssignShiftInput,
} from "@/lib/api/shifts";
import {
  listHolidays, createHoliday, updateHoliday, deleteHoliday,
  getAttendancePolicy, updateAttendancePolicy,
  type AttendanceHoliday, type AttendancePolicy, type PolicyInput,
} from "@/lib/api/attendance";
import { ShiftFormDialog } from "@/components/attendance/shift-form-dialog";

type Tab = "shifts" | "assignments" | "holidays" | "policy";
const TAB_LABELS: Record<Tab, string> = { shifts: "الورديات", assignments: "التعيينات", holidays: "العطلات الرسمية", policy: "السياسة" };

function todayIso() { return new Date().toISOString().slice(0, 10); }
function hours(min: number) { return `${Math.floor(min / 60)}:${String(min % 60).padStart(2, "0")}`; }

export default function ShiftsSettingsPage() {
  const { hasAny } = usePermissions();
  const canEdit = hasAny("Attendance.Edit", "Attendance.Create");

  const [tab, setTab] = useState<Tab>("shifts");
  const [shifts, setShifts] = useState<Shift[]>([]);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState<Shift | null>(null);
  const [creating, setCreating] = useState(false);

  function loadShifts() {
    setLoading(true);
    listShifts().then(setShifts).catch(() => setShifts([])).finally(() => setLoading(false));
  }
  useEffect(() => { loadShifts(); }, []);

  async function remove(s: Shift) {
    if (!confirm(`حذف الوردية "${s.nameAr}"؟`)) return;
    try { await deleteShift(s.id); toast.success("تم الحذف"); loadShifts(); }
    catch (e) { toast.error((e as Error)?.message || "تعذر الحذف"); }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-2 text-sm text-muted-foreground">
        <Link href="/settings" className="hover:text-foreground">الإعدادات</Link>
        <ChevronRight className="h-3.5 w-3.5" />
        <span className="text-foreground">إعدادات الورديات والحضور</span>
      </div>

      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold">الورديات وتعيينها</h1>
          <p className="mt-1 text-sm text-muted-foreground">إنشاء الورديات وتعيينها للموظفين والإدارات والفروع والمسميات الوظيفية</p>
        </div>
        {tab === "shifts" && canEdit && (
          <Button size="sm" onClick={() => setCreating(true)}><Plus /> إضافة وردية</Button>
        )}
      </div>

      <div className="flex flex-wrap items-center gap-1 rounded-lg bg-muted p-0.5 w-fit">
        {(["shifts", "assignments", "holidays", "policy"] as Tab[]).map((t) => (
          <button
            key={t}
            onClick={() => setTab(t)}
            className={`rounded-md px-3 py-1 text-sm font-medium transition-colors ${tab === t ? "bg-background shadow-sm" : "text-muted-foreground hover:text-foreground"}`}
          >
            {TAB_LABELS[t]}
          </button>
        ))}
      </div>

      {tab === "shifts" ? (
        loading ? (
          <div className="flex h-48 items-center justify-center"><Loader2 className="h-6 w-6 animate-spin text-muted-foreground" /></div>
        ) : shifts.length === 0 ? (
          <div className="flex flex-col items-center justify-center border border-dashed border-border p-12 text-center">
            <Clock className="mb-3 h-10 w-10 text-muted-foreground" />
            <p className="text-sm text-muted-foreground">لا توجد ورديات بعد</p>
          </div>
        ) : (
          <div className="overflow-x-auto border border-border bg-card">
            <table className="w-full min-w-[820px] text-sm">
              <thead>
                <tr className="border-b border-border text-right text-xs text-muted-foreground">
                  <th className="px-3 py-2 font-medium">الوردية</th>
                  <th className="px-3 py-2 font-medium">النوع</th>
                  <th className="px-3 py-2 font-medium">الدوام</th>
                  <th className="px-3 py-2 font-medium">المطلوب</th>
                  <th className="px-3 py-2 font-medium">نهاية الأسبوع</th>
                  <th className="px-3 py-2 font-medium">معيّن لـ</th>
                  <th className="px-3 py-2 font-medium">الحالة</th>
                  <th className="px-3 py-2 font-medium"></th>
                </tr>
              </thead>
              <tbody>
                {shifts.map((s) => (
                  <tr key={s.id} className="border-b border-border/40 hover:bg-muted/30">
                    <td className="px-3 py-2">
                      <div className="font-medium">{s.nameAr}</div>
                      <div className="text-xs text-muted-foreground">{s.nameEn}</div>
                    </td>
                    <td className="px-3 py-2">
                      {s.isFlexible
                        ? <span className="rounded-md bg-cyan-500/10 px-2 py-0.5 text-xs text-cyan-600">مرنة</span>
                        : <span className="rounded-md bg-muted px-2 py-0.5 text-xs text-muted-foreground">ثابتة</span>}
                    </td>
                    <td className="px-3 py-2 tabular-nums text-muted-foreground">{s.isFlexible ? "—" : `${s.startTime} - ${s.endTime}`}</td>
                    <td className="px-3 py-2 tabular-nums">{hours(s.requiredMinutes)}</td>
                    <td className="px-3 py-2 text-xs text-muted-foreground">{weekendLabel(s.weekendDays)}</td>
                    <td className="px-3 py-2 tabular-nums">{s.assignedCount}</td>
                    <td className="px-3 py-2">
                      {s.isActive
                        ? <span className="rounded-md bg-green-500/10 px-2 py-0.5 text-xs text-green-600">نشطة</span>
                        : <span className="rounded-md bg-red-500/10 px-2 py-0.5 text-xs text-red-600">معطّلة</span>}
                    </td>
                    <td className="px-3 py-2">
                      {canEdit && (
                        <div className="flex items-center justify-end gap-1">
                          <Button variant="ghost" size="icon-xs" onClick={() => setEditing(s)}><Pencil /></Button>
                          <Button variant="ghost" size="icon-xs" onClick={() => remove(s)}><Trash2 /></Button>
                        </div>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )
      ) : tab === "assignments" ? (
        <AssignmentsTab shifts={shifts} canEdit={canEdit} />
      ) : tab === "holidays" ? (
        <HolidaysTab canEdit={canEdit} />
      ) : (
        <PolicyTab canEdit={canEdit} />
      )}

      {(creating || editing) && (
        <ShiftFormDialog
          shift={editing}
          open
          onClose={() => { setCreating(false); setEditing(null); }}
          onSaved={() => { setCreating(false); setEditing(null); loadShifts(); }}
        />
      )}
    </div>
  );
}

function AssignmentsTab({ shifts, canEdit }: { shifts: Shift[]; canEdit: boolean }) {
  const [assignments, setAssignments] = useState<ShiftAssignment[]>([]);
  const [loading, setLoading] = useState(true);
  const [departments, setDepartments] = useState<OrgOption[]>([]);
  const [branches, setBranches] = useState<OrgOption[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [saving, setSaving] = useState(false);

  const [form, setForm] = useState<AssignShiftInput>({
    shiftId: "", employeeIds: [], departmentId: null, branchId: null, jobTitleId: null,
    effectiveFrom: todayIso(), effectiveTo: null, priority: 0, isActive: true,
  });
  const [scope, setScope] = useState<"employees" | "department" | "branch" | "jobTitle">("employees");

  function load() {
    setLoading(true);
    listShiftAssignments().then(setAssignments).catch(() => setAssignments([])).finally(() => setLoading(false));
  }
  useEffect(() => {
    load();
    getDepartments().then(setDepartments).catch(() => {});
    getBranches().then(setBranches).catch(() => {});
    getEmployees({ pageSize: 500 }).then(setEmployees).catch(() => {});
  }, []);

  const jobTitles = useMemo(() => {
    const map = new Map<string, string>();
    employees.forEach((e) => { if (e.jobTitleId) map.set(e.jobTitleId, e.position); });
    return Array.from(map, ([id, name]) => ({ id, name }));
  }, [employees]);

  async function submit() {
    if (!form.shiftId) { toast.error("اختر الوردية"); return; }
    const payload: AssignShiftInput = {
      ...form,
      employeeIds: scope === "employees" ? form.employeeIds : [],
      departmentId: scope === "department" ? form.departmentId : null,
      branchId: scope === "branch" ? form.branchId : null,
      jobTitleId: scope === "jobTitle" ? form.jobTitleId : null,
    };
    if (scope === "employees" && payload.employeeIds.length === 0) { toast.error("اختر موظفاً واحداً على الأقل"); return; }
    if (scope === "department" && !payload.departmentId) { toast.error("اختر الإدارة"); return; }
    if (scope === "branch" && !payload.branchId) { toast.error("اختر الفرع"); return; }
    if (scope === "jobTitle" && !payload.jobTitleId) { toast.error("اختر المسمى الوظيفي"); return; }

    setSaving(true);
    try {
      const n = await assignShift(payload);
      toast.success(`تم إنشاء ${n} تعيين`);
      setForm((p) => ({ ...p, employeeIds: [], departmentId: null, branchId: null, jobTitleId: null }));
      load();
    } catch (e) { toast.error((e as Error)?.message || "تعذر التعيين"); }
    finally { setSaving(false); }
  }

  async function remove(a: ShiftAssignment) {
    if (!confirm("حذف هذا التعيين؟")) return;
    try { await deleteShiftAssignment(a.id); toast.success("تم الحذف"); load(); }
    catch (e) { toast.error((e as Error)?.message || "تعذر الحذف"); }
  }

  function target(a: ShiftAssignment): string {
    if (a.employeeName) return `موظف: ${a.employeeName}`;
    if (a.departmentName) return `إدارة: ${a.departmentName}`;
    if (a.branchName) return `فرع: ${a.branchName}`;
    if (a.jobTitleName) return `مسمى: ${a.jobTitleName}`;
    return "—";
  }

  const sel = "h-8 w-full rounded-lg border border-border bg-background px-2 text-xs";

  return (
    <div className="grid gap-4 lg:grid-cols-[20rem_1fr]">
      {/* Assign form */}
      {canEdit && (
        <div className="space-y-3 border border-border bg-card p-4">
          <h3 className="flex items-center gap-1.5 text-sm font-bold"><CalendarClock className="h-4 w-4" /> تعيين وردية</h3>

          <div className="space-y-1">
            <label className="text-xs font-bold text-muted-foreground">الوردية</label>
            <select value={form.shiftId} onChange={(e) => setForm((p) => ({ ...p, shiftId: e.target.value }))} className={sel}>
              <option value="">اختر الوردية</option>
              {shifts.map((s) => <option key={s.id} value={s.id}>{s.nameAr}</option>)}
            </select>
          </div>

          <div className="space-y-1">
            <label className="text-xs font-bold text-muted-foreground">نطاق التعيين</label>
            <div className="grid grid-cols-2 gap-1">
              {(["employees", "department", "branch", "jobTitle"] as const).map((s) => (
                <button
                  key={s}
                  onClick={() => setScope(s)}
                  className={`rounded-md border px-2 py-1 text-xs ${scope === s ? "border-primary bg-primary/10 text-primary" : "border-border text-muted-foreground"}`}
                >
                  {s === "employees" ? "موظفون" : s === "department" ? "إدارة" : s === "branch" ? "فرع" : "مسمى وظيفي"}
                </button>
              ))}
            </div>
          </div>

          {scope === "employees" && (
            <div className="space-y-1">
              <label className="text-xs font-bold text-muted-foreground">الموظفون ({form.employeeIds.length})</label>
              <div className="max-h-44 space-y-0.5 overflow-y-auto rounded-lg border border-border p-1.5">
                {employees.map((e) => (
                  <label key={e.id} className="flex cursor-pointer items-center gap-2 rounded px-1.5 py-1 text-xs hover:bg-muted/50">
                    <input
                      type="checkbox"
                      checked={form.employeeIds.includes(e.id)}
                      onChange={(ev) => setForm((p) => ({
                        ...p,
                        employeeIds: ev.target.checked ? [...p.employeeIds, e.id] : p.employeeIds.filter((x) => x !== e.id),
                      }))}
                    />
                    {e.name}
                  </label>
                ))}
              </div>
            </div>
          )}
          {scope === "department" && (
            <select value={form.departmentId ?? ""} onChange={(e) => setForm((p) => ({ ...p, departmentId: e.target.value || null }))} className={sel}>
              <option value="">اختر الإدارة</option>
              {departments.map((d) => <option key={d.id} value={d.id}>{orgLabel(d)}</option>)}
            </select>
          )}
          {scope === "branch" && (
            <select value={form.branchId ?? ""} onChange={(e) => setForm((p) => ({ ...p, branchId: e.target.value || null }))} className={sel}>
              <option value="">اختر الفرع</option>
              {branches.map((b) => <option key={b.id} value={b.id}>{orgLabel(b)}</option>)}
            </select>
          )}
          {scope === "jobTitle" && (
            <select value={form.jobTitleId ?? ""} onChange={(e) => setForm((p) => ({ ...p, jobTitleId: e.target.value || null }))} className={sel}>
              <option value="">اختر المسمى</option>
              {jobTitles.map((j) => <option key={j.id} value={j.id}>{j.name}</option>)}
            </select>
          )}

          <div className="grid grid-cols-2 gap-2">
            <div className="space-y-1">
              <label className="text-xs font-bold text-muted-foreground">من تاريخ</label>
              <input type="date" value={form.effectiveFrom} onChange={(e) => setForm((p) => ({ ...p, effectiveFrom: e.target.value }))} className={sel} />
            </div>
            <div className="space-y-1">
              <label className="text-xs font-bold text-muted-foreground">إلى تاريخ</label>
              <input type="date" value={form.effectiveTo ?? ""} onChange={(e) => setForm((p) => ({ ...p, effectiveTo: e.target.value || null }))} className={sel} />
            </div>
          </div>
          <div className="space-y-1">
            <label className="text-xs font-bold text-muted-foreground">الأولوية</label>
            <input type="number" value={form.priority} onChange={(e) => setForm((p) => ({ ...p, priority: Number(e.target.value) || 0 }))} className={sel} />
          </div>

          <Button className="w-full" onClick={submit} disabled={saving}>
            {saving && <Loader2 className="animate-spin" />} تعيين
          </Button>
        </div>
      )}

      {/* Assignment list */}
      <div className={canEdit ? "" : "lg:col-span-2"}>
        {loading ? (
          <div className="flex h-48 items-center justify-center"><Loader2 className="h-6 w-6 animate-spin text-muted-foreground" /></div>
        ) : assignments.length === 0 ? (
          <div className="flex flex-col items-center justify-center border border-dashed border-border p-12 text-center">
            <CalendarClock className="mb-3 h-10 w-10 text-muted-foreground" />
            <p className="text-sm text-muted-foreground">لا توجد تعيينات بعد</p>
          </div>
        ) : (
          <div className="overflow-x-auto border border-border bg-card">
            <table className="w-full min-w-[640px] text-sm">
              <thead>
                <tr className="border-b border-border text-right text-xs text-muted-foreground">
                  <th className="px-3 py-2 font-medium">الوردية</th>
                  <th className="px-3 py-2 font-medium">الهدف</th>
                  <th className="px-3 py-2 font-medium">الفترة</th>
                  <th className="px-3 py-2 font-medium">الأولوية</th>
                  <th className="px-3 py-2 font-medium">الحالة</th>
                  <th className="px-3 py-2 font-medium"></th>
                </tr>
              </thead>
              <tbody>
                {assignments.map((a) => (
                  <tr key={a.id} className="border-b border-border/40 hover:bg-muted/30">
                    <td className="px-3 py-2 font-medium">{a.shiftName ?? "—"}</td>
                    <td className="px-3 py-2">{target(a)}</td>
                    <td className="px-3 py-2 text-xs tabular-nums text-muted-foreground">
                      {a.effectiveFrom.slice(0, 10)}{a.effectiveTo ? ` ← ${a.effectiveTo.slice(0, 10)}` : " ← مفتوح"}
                    </td>
                    <td className="px-3 py-2 tabular-nums">{a.priority}</td>
                    <td className="px-3 py-2">
                      {a.isActive
                        ? <span className="rounded-md bg-green-500/10 px-2 py-0.5 text-xs text-green-600">نشط</span>
                        : <span className="rounded-md bg-muted px-2 py-0.5 text-xs text-muted-foreground">معطّل</span>}
                    </td>
                    <td className="px-3 py-2">
                      {canEdit && <Button variant="ghost" size="icon-xs" onClick={() => remove(a)}><Trash2 /></Button>}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}

const sel = "h-8 w-full rounded-lg border border-border bg-background px-2 text-xs";

function HolidaysTab({ canEdit }: { canEdit: boolean }) {
  const [rows, setRows] = useState<AttendanceHoliday[]>([]);
  const [loading, setLoading] = useState(true);
  const [nameAr, setNameAr] = useState("");
  const [nameEn, setNameEn] = useState("");
  const [date, setDate] = useState("");
  const [recurring, setRecurring] = useState(false);
  const [saving, setSaving] = useState(false);

  function load() { setLoading(true); listHolidays().then(setRows).catch(() => setRows([])).finally(() => setLoading(false)); }
  useEffect(() => { load(); }, []);

  async function add() {
    if (!nameAr.trim() || !date) { toast.error("أدخل الاسم والتاريخ"); return; }
    setSaving(true);
    try {
      await createHoliday({ nameAr, nameEn: nameEn || nameAr, date, isRecurring: recurring, isActive: true });
      toast.success("تمت إضافة العطلة");
      setNameAr(""); setNameEn(""); setDate(""); setRecurring(false); load();
    } catch (e) { toast.error((e as Error)?.message || "تعذر الحفظ"); }
    finally { setSaving(false); }
  }
  async function del(h: AttendanceHoliday) {
    if (!confirm(`حذف العطلة "${h.nameAr}"؟`)) return;
    try { await deleteHoliday(h.id); toast.success("تم الحذف"); load(); }
    catch (e) { toast.error((e as Error)?.message || "تعذر الحذف"); }
  }

  return (
    <div className="grid gap-4 lg:grid-cols-[20rem_1fr]">
      {canEdit && (
        <div className="space-y-3 border border-border bg-card p-4 h-fit">
          <h3 className="flex items-center gap-1.5 text-sm font-bold"><CalendarOff className="h-4 w-4" /> إضافة عطلة رسمية</h3>
          <div className="space-y-1">
            <label className="text-xs font-bold text-muted-foreground">الاسم (عربي)</label>
            <input value={nameAr} onChange={(e) => setNameAr(e.target.value)} className={sel} />
          </div>
          <div className="space-y-1">
            <label className="text-xs font-bold text-muted-foreground">الاسم (إنجليزي)</label>
            <input value={nameEn} onChange={(e) => setNameEn(e.target.value)} className={sel} dir="ltr" />
          </div>
          <div className="space-y-1">
            <label className="text-xs font-bold text-muted-foreground">التاريخ</label>
            <input type="date" value={date} onChange={(e) => setDate(e.target.value)} className={`${sel} tabular-nums`} />
          </div>
          <label className="flex items-center gap-2 text-xs">
            <input type="checkbox" checked={recurring} onChange={(e) => setRecurring(e.target.checked)} />
            تتكرر سنوياً (نفس اليوم/الشهر)
          </label>
          <Button className="w-full" size="sm" onClick={add} disabled={saving}>{saving && <Loader2 className="animate-spin" />} إضافة</Button>
        </div>
      )}

      <div className={canEdit ? "" : "lg:col-span-2"}>
        {loading ? (
          <div className="flex h-40 items-center justify-center"><Loader2 className="h-6 w-6 animate-spin text-muted-foreground" /></div>
        ) : rows.length === 0 ? (
          <div className="flex flex-col items-center justify-center border border-dashed border-border p-12 text-center">
            <CalendarOff className="mb-3 h-10 w-10 text-muted-foreground" />
            <p className="text-sm text-muted-foreground">لا توجد عطلات رسمية</p>
          </div>
        ) : (
          <div className="overflow-x-auto border border-border bg-card">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-border text-right text-xs text-muted-foreground">
                  <th className="px-3 py-2 font-medium">العطلة</th>
                  <th className="px-3 py-2 font-medium">التاريخ</th>
                  <th className="px-3 py-2 font-medium">التكرار</th>
                  <th className="px-3 py-2 font-medium">الحالة</th>
                  <th className="px-3 py-2 font-medium"></th>
                </tr>
              </thead>
              <tbody>
                {rows.map((h) => (
                  <tr key={h.id} className="border-b border-border/40 hover:bg-muted/30">
                    <td className="px-3 py-2">
                      <div className="font-medium">{h.nameAr}</div>
                      <div className="text-xs text-muted-foreground">{h.nameEn}</div>
                    </td>
                    <td className="px-3 py-2 tabular-nums text-muted-foreground">{h.date}</td>
                    <td className="px-3 py-2 text-xs">{h.isRecurring ? "سنوية" : "لمرة واحدة"}</td>
                    <td className="px-3 py-2">
                      {h.isActive
                        ? <span className="rounded-md bg-green-500/10 px-2 py-0.5 text-xs text-green-600">نشطة</span>
                        : <span className="rounded-md bg-muted px-2 py-0.5 text-xs text-muted-foreground">معطّلة</span>}
                    </td>
                    <td className="px-3 py-2">
                      {canEdit && <Button variant="ghost" size="icon-xs" onClick={() => del(h)}><Trash2 /></Button>}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}

function PolicyTab({ canEdit }: { canEdit: boolean }) {
  const [p, setP] = useState<AttendancePolicy | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => { getAttendancePolicy().then(setP).catch(() => {}).finally(() => setLoading(false)); }, []);
  function set<K extends keyof PolicyInput>(k: K, v: PolicyInput[K]) { setP((prev) => (prev ? { ...prev, [k]: v } : prev)); }

  async function save() {
    if (!p) return;
    setSaving(true);
    try {
      const u = await updateAttendancePolicy({
        defaultGraceMinutes: p.defaultGraceMinutes, roundingMinutes: p.roundingMinutes,
        autoMarkAbsent: p.autoMarkAbsent, countOvertime: p.countOvertime,
      });
      setP(u);
      toast.success("تم حفظ السياسة");
    } catch (e) { toast.error((e as Error)?.message || "تعذر الحفظ"); }
    finally { setSaving(false); }
  }

  if (loading || !p) return <div className="flex h-40 items-center justify-center"><Loader2 className="h-6 w-6 animate-spin text-muted-foreground" /></div>;

  return (
    <div className="max-w-xl space-y-4 border border-border bg-card p-5">
      <p className="text-sm text-muted-foreground">قواعد عامة تُطبَّق على كل الورديات عند عدم تحديدها في الوردية نفسها.</p>
      <div className="grid grid-cols-2 gap-4">
        <div className="space-y-1">
          <label className="text-xs font-bold text-muted-foreground">فترة السماح الافتراضية (دقائق)</label>
          <input type="number" min={0} value={p.defaultGraceMinutes} disabled={!canEdit}
            onChange={(e) => set("defaultGraceMinutes", Number(e.target.value) || 0)}
            className="h-9 w-full rounded-lg border border-border bg-background px-2 text-sm" />
          <p className="text-[0.7rem] text-muted-foreground">تُستخدم عندما لا تحدد الوردية فترة سماح.</p>
        </div>
        <div className="space-y-1">
          <label className="text-xs font-bold text-muted-foreground">تقريب ساعات العمل (دقائق)</label>
          <input type="number" min={0} value={p.roundingMinutes} disabled={!canEdit}
            onChange={(e) => set("roundingMinutes", Number(e.target.value) || 0)}
            className="h-9 w-full rounded-lg border border-border bg-background px-2 text-sm" />
          <p className="text-[0.7rem] text-muted-foreground">0 = بدون تقريب (مثال: 5 أو 15).</p>
        </div>
      </div>
      <label className="flex items-center justify-between rounded-lg border border-border p-3">
        <div>
          <span className="text-sm font-medium">احتساب الغياب تلقائياً</span>
          <p className="text-xs text-muted-foreground">تعليم أيام العمل بدون بصمة كـ"غائب".</p>
        </div>
        <input type="checkbox" checked={p.autoMarkAbsent} disabled={!canEdit} onChange={(e) => set("autoMarkAbsent", e.target.checked)} className="h-4 w-4" />
      </label>
      <label className="flex items-center justify-between rounded-lg border border-border p-3">
        <div>
          <span className="text-sm font-medium">احتساب الوقت الإضافي</span>
          <p className="text-xs text-muted-foreground">يُحتسب فقط للورديات التي تسمح بالإضافي.</p>
        </div>
        <input type="checkbox" checked={p.countOvertime} disabled={!canEdit} onChange={(e) => set("countOvertime", e.target.checked)} className="h-4 w-4" />
      </label>
      {canEdit && (
        <div className="flex justify-end">
          <Button onClick={save} disabled={saving}>{saving ? <Loader2 className="animate-spin" /> : <Save />} حفظ السياسة</Button>
        </div>
      )}
    </div>
  );
}
