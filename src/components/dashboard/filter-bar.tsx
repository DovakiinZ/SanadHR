"use client";

import { useEffect, useState } from "react";
import { Filter, X } from "lucide-react";
import { getBranches, getDepartments, orgLabel, type OrgOption } from "@/lib/api/org";
import { WidgetFilterSpec } from "@/types/dashboard";

// Global dashboard filters. Each filter targets a canonical field; the engine simply
// ignores it for widgets whose object lacks that field, so one bar drives every widget.
interface FilterBarProps {
  onChange: (filters: WidgetFilterSpec[]) => void;
}

const DATE_PRESETS = [
  { label: "كل الوقت", value: "" },
  { label: "آخر ٧ أيام", value: "7" },
  { label: "آخر ٣٠ يوم", value: "30" },
  { label: "آخر ٩٠ يوم", value: "90" },
  { label: "هذا العام", value: "365" },
];

export function DashboardFilterBar({ onChange }: FilterBarProps) {
  const [departments, setDepartments] = useState<OrgOption[]>([]);
  const [branches, setBranches] = useState<OrgOption[]>([]);
  const [datePreset, setDatePreset] = useState("");
  const [departmentId, setDepartmentId] = useState("");
  const [branchId, setBranchId] = useState("");

  useEffect(() => {
    getDepartments().then(setDepartments).catch(() => {});
    getBranches().then(setBranches).catch(() => {});
  }, []);

  useEffect(() => {
    const filters: WidgetFilterSpec[] = [];
    if (datePreset) filters.push({ field: "CreatedAt", operator: "last_n_days", value: datePreset });
    if (departmentId) filters.push({ field: "DepartmentId", operator: "eq", value: departmentId });
    if (branchId) filters.push({ field: "BranchId", operator: "eq", value: branchId });
    onChange(filters);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [datePreset, departmentId, branchId]);

  const hasActive = datePreset || departmentId || branchId;
  const reset = () => { setDatePreset(""); setDepartmentId(""); setBranchId(""); };

  const selectCls = "h-9 border border-border bg-secondary px-3 text-sm text-foreground";

  return (
    <div className="flex flex-wrap items-center gap-2 border border-border bg-card px-3 py-2">
      <span className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-muted-foreground">
        <Filter className="h-3.5 w-3.5" /> تصفية
      </span>

      <select value={datePreset} onChange={(e) => setDatePreset(e.target.value)} className={selectCls}>
        {DATE_PRESETS.map((p) => <option key={p.value} value={p.value}>{p.label}</option>)}
      </select>

      <select value={departmentId} onChange={(e) => setDepartmentId(e.target.value)} className={selectCls}>
        <option value="">كل الإدارات</option>
        {departments.map((d) => <option key={d.id} value={d.id}>{orgLabel(d)}</option>)}
      </select>

      <select value={branchId} onChange={(e) => setBranchId(e.target.value)} className={selectCls}>
        <option value="">كل الفروع</option>
        {branches.map((b) => <option key={b.id} value={b.id}>{orgLabel(b)}</option>)}
      </select>

      {hasActive && (
        <button onClick={reset} className="flex items-center gap-1 text-xs text-muted-foreground hover:text-foreground">
          <X className="h-3.5 w-3.5" /> مسح
        </button>
      )}
    </div>
  );
}
