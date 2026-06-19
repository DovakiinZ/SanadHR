"use client";

import { useEffect, useMemo, useState } from "react";
import { Loader2, Paperclip, Check } from "lucide-react";
import { toast } from "sonner";
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { getMasterDataItems, type MasterDataItem } from "@/lib/api/master-data";
import { getDepartments, getBranches, type OrgOption, orgLabel } from "@/lib/api/org";
import { getEmployees } from "@/lib/api/employees";
import { Employee } from "@/types";
import { uploadFile } from "@/lib/api/files";
import { assignLeave, type AssignLeaveInput } from "@/lib/api/leaves";

interface Props {
  open: boolean;
  onClose: () => void;
  onSaved: () => void;
}

type Scope = "Employees" | "Department" | "Branch" | "JobTitle";
const inputCls = "h-9 w-full rounded-lg border border-border bg-background px-2 text-sm";
const labelCls = "text-xs font-bold text-muted-foreground";

export function LeaveAssignDialog({ open, onClose, onSaved }: Props) {
  const [types, setTypes] = useState<MasterDataItem[]>([]);
  const [departments, setDepartments] = useState<OrgOption[]>([]);
  const [branches, setBranches] = useState<OrgOption[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);

  const [leaveTypeId, setLeaveTypeId] = useState("");
  const [scope, setScope] = useState<Scope>("Employees");
  const [employeeIds, setEmployeeIds] = useState<string[]>([]);
  const [departmentId, setDepartmentId] = useState("");
  const [branchId, setBranchId] = useState("");
  const [jobTitleId, setJobTitleId] = useState("");
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");
  const [notes, setNotes] = useState("");
  const [attachmentUrl, setAttachmentUrl] = useState<string | null>(null);
  const [uploading, setUploading] = useState(false);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    getMasterDataItems("LeaveType").then((t) => { setTypes(t); if (t[0]) setLeaveTypeId((p) => p || t[0].id); }).catch(() => {});
    getDepartments().then(setDepartments).catch(() => {});
    getBranches().then(setBranches).catch(() => {});
    getEmployees({ pageSize: 500 }).then(setEmployees).catch(() => {});
  }, []);

  const jobTitles = useMemo(() => {
    const map = new Map<string, string>();
    employees.forEach((e) => { if (e.jobTitleId) map.set(e.jobTitleId, e.position); });
    return Array.from(map, ([id, name]) => ({ id, name }));
  }, [employees]);

  async function onFile(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;
    setUploading(true);
    try {
      const res = await uploadFile(file, "leaves");
      setAttachmentUrl(res.url);
    } catch { toast.error("تعذر رفع المرفق"); }
    finally { setUploading(false); }
  }

  async function submit() {
    if (!leaveTypeId) { toast.error("اختر نوع الإجازة"); return; }
    if (!startDate || !endDate) { toast.error("حدّد تاريخ البداية والنهاية"); return; }
    if (endDate < startDate) { toast.error("تاريخ النهاية قبل البداية"); return; }
    if (scope === "Employees" && employeeIds.length === 0) { toast.error("اختر موظفاً واحداً على الأقل"); return; }
    if (scope === "Department" && !departmentId) { toast.error("اختر الإدارة"); return; }
    if (scope === "Branch" && !branchId) { toast.error("اختر الفرع"); return; }
    if (scope === "JobTitle" && !jobTitleId) { toast.error("اختر المسمى الوظيفي"); return; }

    const body: AssignLeaveInput = {
      leaveTypeId, scope, employeeIds: scope === "Employees" ? employeeIds : [],
      departmentId: scope === "Department" ? departmentId : null,
      branchId: scope === "Branch" ? branchId : null,
      jobTitleId: scope === "JobTitle" ? jobTitleId : null,
      startDate, endDate, notes, attachmentUrl,
    };
    setSaving(true);
    try {
      const n = await assignLeave(body);
      toast.success(`تم تعيين الإجازة لـ ${n} موظف`);
      onSaved();
      onClose();
    } catch (e) {
      toast.error((e as Error)?.message || "تعذر تعيين الإجازة");
    } finally {
      setSaving(false);
    }
  }

  return (
    <Dialog open={open} onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>تعيين إجازة</DialogTitle>
          <DialogDescription>إجازة مباشرة من الموارد البشرية (عطلة، إجازة إجبارية/إدارية) — تختلف عن طلب الموظف</DialogDescription>
        </DialogHeader>

        <div className="space-y-3">
          <div className="space-y-1">
            <label className={labelCls}>نوع الإجازة</label>
            <select value={leaveTypeId} onChange={(e) => setLeaveTypeId(e.target.value)} className={inputCls}>
              <option value="">اختر النوع</option>
              {types.map((t) => <option key={t.id} value={t.id}>{t.nameAr}</option>)}
            </select>
          </div>

          <div className="space-y-1">
            <label className={labelCls}>نطاق التعيين</label>
            <div className="grid grid-cols-2 gap-1">
              {(["Employees", "Department", "Branch", "JobTitle"] as Scope[]).map((s) => (
                <button key={s} onClick={() => setScope(s)}
                  className={`rounded-md border px-2 py-1 text-xs ${scope === s ? "border-primary bg-primary/10 text-primary" : "border-border text-muted-foreground"}`}>
                  {s === "Employees" ? "موظفون" : s === "Department" ? "إدارة" : s === "Branch" ? "فرع" : "مسمى وظيفي"}
                </button>
              ))}
            </div>
          </div>

          {scope === "Employees" && (
            <div className="space-y-1">
              <label className={labelCls}>الموظفون ({employeeIds.length})</label>
              <div className="max-h-40 space-y-0.5 overflow-y-auto rounded-lg border border-border p-1.5">
                {employees.map((e) => (
                  <label key={e.id} className="flex cursor-pointer items-center gap-2 rounded px-1.5 py-1 text-xs hover:bg-muted/50">
                    <input type="checkbox" checked={employeeIds.includes(e.id)}
                      onChange={(ev) => setEmployeeIds((p) => ev.target.checked ? [...p, e.id] : p.filter((x) => x !== e.id))} />
                    {e.name}
                  </label>
                ))}
              </div>
            </div>
          )}
          {scope === "Department" && (
            <select value={departmentId} onChange={(e) => setDepartmentId(e.target.value)} className={inputCls}>
              <option value="">اختر الإدارة</option>
              {departments.map((d) => <option key={d.id} value={d.id}>{orgLabel(d)}</option>)}
            </select>
          )}
          {scope === "Branch" && (
            <select value={branchId} onChange={(e) => setBranchId(e.target.value)} className={inputCls}>
              <option value="">اختر الفرع</option>
              {branches.map((b) => <option key={b.id} value={b.id}>{orgLabel(b)}</option>)}
            </select>
          )}
          {scope === "JobTitle" && (
            <select value={jobTitleId} onChange={(e) => setJobTitleId(e.target.value)} className={inputCls}>
              <option value="">اختر المسمى</option>
              {jobTitles.map((j) => <option key={j.id} value={j.id}>{j.name}</option>)}
            </select>
          )}

          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1">
              <label className={labelCls}>من تاريخ</label>
              <input type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} className={`${inputCls} tabular-nums`} />
            </div>
            <div className="space-y-1">
              <label className={labelCls}>إلى تاريخ</label>
              <input type="date" value={endDate} onChange={(e) => setEndDate(e.target.value)} className={`${inputCls} tabular-nums`} />
            </div>
          </div>

          <div className="space-y-1">
            <label className={labelCls}>ملاحظات</label>
            <textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={2}
              className="w-full rounded-lg border border-border bg-background px-2 py-1.5 text-sm" />
          </div>

          <div className="space-y-1">
            <label className={labelCls}>مرفق (اختياري)</label>
            <label className="flex cursor-pointer items-center gap-2 rounded-lg border border-dashed border-border p-2 text-xs text-muted-foreground hover:bg-muted/30">
              {uploading ? <Loader2 className="h-4 w-4 animate-spin" /> : attachmentUrl ? <Check className="h-4 w-4 text-green-600" /> : <Paperclip className="h-4 w-4" />}
              {attachmentUrl ? "تم الرفع" : "إرفاق ملف"}
              <input type="file" className="hidden" onChange={onFile} />
            </label>
          </div>
        </div>

        <div className="mt-2 flex items-center justify-end gap-2">
          <Button variant="outline" onClick={onClose} disabled={saving}>إلغاء</Button>
          <Button onClick={submit} disabled={saving || uploading}>{saving && <Loader2 className="animate-spin" />} تعيين</Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
