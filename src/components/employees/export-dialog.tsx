"use client";

import { useEffect, useState } from "react";
import { Download, Loader2, X } from "lucide-react";
import { toast } from "sonner";
import { usePermissions } from "@/lib/permissions";
import { exportEmployees } from "@/lib/api/employees";
import { getLookup, lookupLabel, LookupItem } from "@/lib/api/lookups";
import { getDepartments, getBranches, OrgOption, orgLabel } from "@/lib/api/org";

const ALL_GROUPS = [
  { key: "personal", label: "البيانات الشخصية", salary: false },
  { key: "employment", label: "بيانات التوظيف", salary: false },
  { key: "salary", label: "بيانات الراتب", salary: true },
  { key: "bank", label: "البيانات البنكية", salary: true },
  { key: "leave", label: "رصيد الإجازات", salary: false },
  { key: "attendance", label: "ملخص الحضور", salary: false },
];

const STATUS_OPTIONS = ["نشط", "في إجازة", "موقوف", "منتهي", "مستقيل"];

export function ExportDialog({ open, onClose }: { open: boolean; onClose: () => void }) {
  const { hasAny } = usePermissions();
  const canSeeComp = hasAny("Payroll.View", "Payroll.Edit", "Employees.Edit", "Employees.Create");
  const groups = ALL_GROUPS.filter((g) => !g.salary || canSeeComp);

  const [selected, setSelected] = useState<Record<string, boolean>>({ personal: true, employment: true });
  const [departmentId, setDepartmentId] = useState("");
  const [jobTitleId, setJobTitleId] = useState("");
  const [branchId, setBranchId] = useState("");
  const [status, setStatus] = useState("");
  const [jobTitles, setJobTitles] = useState<LookupItem[]>([]);
  const [departments, setDepartments] = useState<OrgOption[]>([]);
  const [branches, setBranches] = useState<OrgOption[]>([]);
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    if (!open) return;
    getLookup("job-titles").then(setJobTitles).catch(() => {});
    getDepartments().then(setDepartments).catch(() => {});
    getBranches().then(setBranches).catch(() => {});
  }, [open]);

  if (!open) return null;

  const toggle = (k: string) => setSelected((s) => ({ ...s, [k]: !s[k] }));

  const run = async () => {
    const chosen = groups.filter((g) => selected[g.key]).map((g) => g.key);
    if (chosen.length === 0) { toast.error("اختر حقلاً واحداً على الأقل"); return; }
    setBusy(true);
    try {
      await exportEmployees({
        groups: chosen,
        departmentId: departmentId || null,
        jobTitleId: jobTitleId || null,
        branchId: branchId || null,
        status: status || null,
      });
      toast.success("تم التصدير");
      onClose();
    } catch { toast.error("تعذر التصدير"); }
    finally { setBusy(false); }
  };

  const sel = "h-9 w-full border border-border bg-secondary px-3 text-sm";

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/50" onClick={onClose} />
      <div className="relative z-10 w-full max-w-lg border border-border bg-background p-5" dir="rtl">
        <div className="mb-4 flex items-center justify-between">
          <h3 className="text-lg font-bold">تصدير الموظفين إلى Excel</h3>
          <button onClick={onClose} className="text-muted-foreground hover:text-foreground"><X className="h-5 w-5" /></button>
        </div>

        <p className="mb-2 text-xs font-bold uppercase tracking-wider text-muted-foreground">الحقول</p>
        <div className="mb-4 grid grid-cols-2 gap-2">
          {groups.map((g) => (
            <label key={g.key} className="flex items-center gap-2 border border-border px-3 py-2 text-sm">
              <input type="checkbox" checked={!!selected[g.key]} onChange={() => toggle(g.key)} /> {g.label}
            </label>
          ))}
        </div>
        {!canSeeComp && <p className="mb-3 text-xs text-amber-500">لا تملك صلاحية تصدير بيانات الراتب — لن تُصدّر.</p>}

        <p className="mb-2 text-xs font-bold uppercase tracking-wider text-muted-foreground">التصفية (اختياري)</p>
        <div className="mb-4 grid grid-cols-2 gap-2">
          <select value={departmentId} onChange={(e) => setDepartmentId(e.target.value)} className={sel}>
            <option value="">كل الأقسام</option>
            {departments.map((d) => <option key={d.id} value={d.id}>{orgLabel(d)}</option>)}
          </select>
          <select value={jobTitleId} onChange={(e) => setJobTitleId(e.target.value)} className={sel}>
            <option value="">كل المسميات</option>
            {jobTitles.map((t) => <option key={t.id} value={t.id}>{lookupLabel(t)}</option>)}
          </select>
          <select value={branchId} onChange={(e) => setBranchId(e.target.value)} className={sel}>
            <option value="">كل الفروع</option>
            {branches.map((b) => <option key={b.id} value={b.id}>{orgLabel(b)}</option>)}
          </select>
          <select value={status} onChange={(e) => setStatus(e.target.value)} className={sel}>
            <option value="">كل الحالات</option>
            {STATUS_OPTIONS.map((s) => <option key={s} value={s}>{s}</option>)}
          </select>
        </div>

        <div className="flex justify-end gap-2">
          <button onClick={onClose} className="h-10 border border-border px-4 text-sm hover:bg-muted">إلغاء</button>
          <button onClick={run} disabled={busy} className="inline-flex h-10 items-center gap-2 bg-primary px-5 text-sm font-bold text-primary-foreground hover:bg-primary/80 disabled:opacity-50">
            {busy ? <Loader2 className="h-4 w-4 animate-spin" /> : <Download className="h-4 w-4" />} تصدير
          </button>
        </div>
      </div>
    </div>
  );
}
