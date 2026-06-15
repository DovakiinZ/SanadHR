"use client";

import { useCallback, useEffect, useState } from "react";
import {
  Loader2, CalendarDays, Filter, X, Plus, Eye, Printer, Pencil, Ban,
} from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { usePermissions } from "@/lib/permissions";
import { getDepartments, getBranches, type OrgOption, orgLabel } from "@/lib/api/org";
import { getMasterDataItems, type MasterDataItem } from "@/lib/api/master-data";
import {
  listLeaves, cancelLeave, printLeaveRecord,
  LEAVE_STATUS_AR, LEAVE_STATUS_STYLE, LEAVE_SOURCE_AR,
  type LeaveRecordRow, type LeaveFilters,
} from "@/lib/api/leaves";
import { LeaveDetailDrawer } from "@/components/leaves/leave-detail-drawer";
import { LeaveAssignDialog } from "@/components/leaves/leave-assign-dialog";
import { LeaveEditDialog } from "@/components/leaves/leave-edit-dialog";

const STATUSES = ["Approved", "Assigned", "Canceled", "Edited"];
const SOURCES = ["Request", "HRAssignment", "Import", "System"];

function StatusChip({ status }: { status: string }) {
  return (
    <span className={`inline-flex items-center rounded-md px-2 py-0.5 text-xs font-medium ${LEAVE_STATUS_STYLE[status] ?? "bg-muted text-muted-foreground"}`}>
      {LEAVE_STATUS_AR[status] ?? status}
    </span>
  );
}

export default function LeavesPage() {
  const { hasAny } = usePermissions();
  const canAssign = hasAny("Leaves.Assign", "Leaves.Create", "Employees.Edit", "Employees.Create");
  const canEdit = hasAny("Leaves.Edit", "Employees.Edit");
  const canCancel = hasAny("Leaves.Cancel", "Leaves.Edit", "Employees.Edit");

  const [rows, setRows] = useState<LeaveRecordRow[]>([]);
  const [loading, setLoading] = useState(true);
  const [filters, setFilters] = useState<LeaveFilters>({});
  const [showFilters, setShowFilters] = useState(false);

  const [departments, setDepartments] = useState<OrgOption[]>([]);
  const [branches, setBranches] = useState<OrgOption[]>([]);
  const [leaveTypes, setLeaveTypes] = useState<MasterDataItem[]>([]);

  const [drawerRow, setDrawerRow] = useState<LeaveRecordRow | null>(null);
  const [editRow, setEditRow] = useState<LeaveRecordRow | null>(null);
  const [assigning, setAssigning] = useState(false);
  const [printingId, setPrintingId] = useState<string | null>(null);

  useEffect(() => {
    getDepartments().then(setDepartments).catch(() => {});
    getBranches().then(setBranches).catch(() => {});
    getMasterDataItems("LeaveType").then(setLeaveTypes).catch(() => {});
  }, []);

  const load = useCallback(() => {
    setLoading(true);
    listLeaves(filters).then(setRows).catch(() => setRows([])).finally(() => setLoading(false));
  }, [filters]);
  useEffect(() => { load(); }, [load]);

  function setF(key: keyof LeaveFilters, value: string) {
    setFilters((p) => ({ ...p, [key]: value || undefined }));
  }
  const activeFilterCount = Object.values(filters).filter(Boolean).length;

  async function doPrint(r: LeaveRecordRow) {
    setPrintingId(r.id);
    try { await printLeaveRecord(r.id); }
    catch (e) { toast.error((e as Error)?.message || "تعذر الطباعة"); }
    finally { setPrintingId(null); }
  }

  async function doCancel(r: LeaveRecordRow) {
    const reason = window.prompt("سبب إلغاء الإجازة (اختياري):", "");
    if (reason === null) return; // user dismissed
    try {
      await cancelLeave(r.id, reason || undefined);
      toast.success("تم إلغاء الإجازة واسترجاع الرصيد");
      load();
    } catch (e) {
      toast.error((e as Error)?.message || "تعذر الإلغاء");
    }
  }

  const d = (s?: string | null) => (s ? s.slice(0, 10) : "—");

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold">الإجازات</h1>
          <p className="mt-1 text-sm text-muted-foreground">إدارة سجلات الإجازات المعتمدة والمُعيّنة — مختلفة عن طلبات الإجازة</p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant={activeFilterCount ? "default" : "outline"} size="sm" onClick={() => setShowFilters((s) => !s)}>
            <Filter /> تصفية {activeFilterCount > 0 && `(${activeFilterCount})`}
          </Button>
          {canAssign && <Button size="sm" onClick={() => setAssigning(true)}><Plus /> تعيين إجازة</Button>}
        </div>
      </div>

      {showFilters && (
        <div className="flex flex-wrap items-center gap-2 border border-border bg-card p-3">
          <input type="date" value={filters.from ?? ""} onChange={(e) => setF("from", e.target.value)} className="h-8 rounded-lg border border-border bg-background px-2 text-xs tabular-nums" title="من" />
          <input type="date" value={filters.to ?? ""} onChange={(e) => setF("to", e.target.value)} className="h-8 rounded-lg border border-border bg-background px-2 text-xs tabular-nums" title="إلى" />
          <select value={filters.departmentId ?? ""} onChange={(e) => setF("departmentId", e.target.value)} className="h-8 rounded-lg border border-border bg-background px-2 text-xs">
            <option value="">كل الإدارات</option>
            {departments.map((x) => <option key={x.id} value={x.id}>{orgLabel(x)}</option>)}
          </select>
          <select value={filters.branchId ?? ""} onChange={(e) => setF("branchId", e.target.value)} className="h-8 rounded-lg border border-border bg-background px-2 text-xs">
            <option value="">كل الفروع</option>
            {branches.map((x) => <option key={x.id} value={x.id}>{orgLabel(x)}</option>)}
          </select>
          <select value={filters.leaveTypeId ?? ""} onChange={(e) => setF("leaveTypeId", e.target.value)} className="h-8 rounded-lg border border-border bg-background px-2 text-xs">
            <option value="">كل الأنواع</option>
            {leaveTypes.map((x) => <option key={x.id} value={x.id}>{x.nameAr}</option>)}
          </select>
          <select value={filters.status ?? ""} onChange={(e) => setF("status", e.target.value)} className="h-8 rounded-lg border border-border bg-background px-2 text-xs">
            <option value="">كل الحالات</option>
            {STATUSES.map((s) => <option key={s} value={s}>{LEAVE_STATUS_AR[s]}</option>)}
          </select>
          <select value={filters.source ?? ""} onChange={(e) => setF("source", e.target.value)} className="h-8 rounded-lg border border-border bg-background px-2 text-xs">
            <option value="">كل المصادر</option>
            {SOURCES.map((s) => <option key={s} value={s}>{LEAVE_SOURCE_AR[s]}</option>)}
          </select>
          {activeFilterCount > 0 && (
            <button onClick={() => setFilters({})} className="flex items-center gap-1 text-xs text-muted-foreground hover:text-foreground">
              <X className="h-3.5 w-3.5" /> مسح
            </button>
          )}
        </div>
      )}

      {loading ? (
        <div className="flex h-64 items-center justify-center"><Loader2 className="h-6 w-6 animate-spin text-muted-foreground" /></div>
      ) : rows.length === 0 ? (
        <div className="flex flex-col items-center justify-center border border-dashed border-border p-12 text-center">
          <CalendarDays className="mb-3 h-10 w-10 text-muted-foreground" />
          <p className="text-sm text-muted-foreground">لا توجد سجلات إجازات مطابقة</p>
        </div>
      ) : (
        <div className="overflow-x-auto border border-border bg-card">
          <table className="w-full min-w-[980px] text-sm">
            <thead>
              <tr className="border-b border-border text-right text-xs text-muted-foreground">
                <th className="px-3 py-2 font-medium">رقم السجل</th>
                <th className="px-3 py-2 font-medium">الموظف</th>
                <th className="px-3 py-2 font-medium">النوع</th>
                <th className="px-3 py-2 font-medium">الفترة</th>
                <th className="px-3 py-2 font-medium">الأيام</th>
                <th className="px-3 py-2 font-medium">الرصيد</th>
                <th className="px-3 py-2 font-medium">الحالة</th>
                <th className="px-3 py-2 font-medium">المصدر</th>
                <th className="px-3 py-2 font-medium"></th>
              </tr>
            </thead>
            <tbody>
              {rows.map((r) => (
                <tr key={r.id} className="border-b border-border/40 hover:bg-muted/30">
                  <td className="px-3 py-2 font-mono text-xs">{r.recordNumber}</td>
                  <td className="px-3 py-2">
                    <div className="font-medium">{r.employeeName ?? "—"}</div>
                    <div className="text-xs text-muted-foreground">{r.departmentName ?? "—"}</div>
                  </td>
                  <td className="px-3 py-2">{r.leaveTypeName ?? "—"}</td>
                  <td className="px-3 py-2 text-xs tabular-nums text-muted-foreground">{d(r.startDate)} ← {d(r.endDate)}</td>
                  <td className="px-3 py-2 tabular-nums">{r.daysCount}</td>
                  <td className="px-3 py-2 text-xs tabular-nums">
                    {r.affectsBalance ? (
                      <span><span className="text-muted-foreground">{r.balanceBefore}</span> ← <span className="font-medium">{r.balanceAfter}</span></span>
                    ) : (
                      <span className="text-muted-foreground">لا يؤثر</span>
                    )}
                  </td>
                  <td className="px-3 py-2"><StatusChip status={r.status} /></td>
                  <td className="px-3 py-2 text-xs text-muted-foreground">{LEAVE_SOURCE_AR[r.source] ?? r.source}</td>
                  <td className="px-3 py-2">
                    <div className="flex items-center justify-end gap-1">
                      <Button variant="ghost" size="icon-xs" title="التفاصيل" onClick={() => setDrawerRow(r)}><Eye /></Button>
                      <Button variant="ghost" size="icon-xs" title="طباعة سجل الإجازة" disabled={printingId === r.id} onClick={() => doPrint(r)}>
                        {printingId === r.id ? <Loader2 className="animate-spin" /> : <Printer />}
                      </Button>
                      {canEdit && r.status !== "Canceled" && <Button variant="ghost" size="icon-xs" title="تعديل" onClick={() => setEditRow(r)}><Pencil /></Button>}
                      {canCancel && r.status !== "Canceled" && <Button variant="ghost" size="icon-xs" title="إلغاء" onClick={() => doCancel(r)}><Ban /></Button>}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <LeaveDetailDrawer row={drawerRow} open={!!drawerRow} onClose={() => setDrawerRow(null)} />
      {assigning && <LeaveAssignDialog open onClose={() => setAssigning(false)} onSaved={load} />}
      {editRow && <LeaveEditDialog row={editRow} open onClose={() => setEditRow(null)} onSaved={load} />}
    </div>
  );
}
