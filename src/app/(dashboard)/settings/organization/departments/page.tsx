"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { ArrowRight, Plus, Pencil, Trash2, Loader2, CornerDownLeft, Network } from "lucide-react";
import { toast } from "sonner";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { ApiError } from "@/lib/api-client";
import {
  Department, listDepartments, createDepartment, updateDepartment, deleteDepartment,
} from "@/lib/api/org";
import { getBranches, OrgOption, orgLabel } from "@/lib/api/org";
import { getEmployees } from "@/lib/api/employees";
import { getLookup, lookupLabel, LookupItem } from "@/lib/api/lookups";
import { Employee } from "@/types";

function notifyError(err: unknown, fallback: string) {
  if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
    toast.error(err instanceof ApiError ? err.message : fallback);
  }
}

interface DeptForm {
  nameAr: string;
  nameEn: string;
  code: string;
  description: string;
  parentDepartmentId: string;
  managerId: string;
  deputyManagerId: string;
  branchId: string;
  costCenterId: string;
  isActive: boolean;
}

const emptyForm: DeptForm = {
  nameAr: "", nameEn: "", code: "", description: "",
  parentDepartmentId: "", managerId: "", deputyManagerId: "", branchId: "", costCenterId: "", isActive: true,
};

export default function DepartmentsPage() {
  const [departments, setDepartments] = useState<Department[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [branches, setBranches] = useState<OrgOption[]>([]);
  const [costCenters, setCostCenters] = useState<LookupItem[]>([]);
  const [loading, setLoading] = useState(true);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<Department | null>(null);
  const [form, setForm] = useState<DeptForm>(emptyForm);
  const [saving, setSaving] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<Department | null>(null);
  const [deleting, setDeleting] = useState(false);

  const loadDepartments = useCallback(async () => {
    try { setDepartments(await listDepartments()); }
    catch (err) { notifyError(err, "تعذر تحميل الأقسام"); }
  }, []);

  useEffect(() => {
    (async () => {
      setLoading(true);
      try {
        const [deps, emps, brs, cc] = await Promise.all([
          listDepartments(), getEmployees(), getBranches(), getLookup("cost-centers"),
        ]);
        setDepartments(deps); setEmployees(emps); setBranches(brs); setCostCenters(cc);
      } catch (err) {
        notifyError(err, "تعذر تحميل البيانات");
      } finally { setLoading(false); }
    })();
  }, []);

  // Build hierarchical (indented) order.
  const ordered = useMemo(() => {
    const byParent = new Map<string | null, Department[]>();
    for (const d of departments) {
      const key = d.parentDepartmentId ?? null;
      if (!byParent.has(key)) byParent.set(key, []);
      byParent.get(key)!.push(d);
    }
    const result: { dept: Department; depth: number }[] = [];
    const visit = (parentId: string | null, depth: number) => {
      const children = (byParent.get(parentId) ?? []).slice().sort((a, b) => (a.nameAr || a.name).localeCompare(b.nameAr || b.name, "ar"));
      for (const c of children) { result.push({ dept: c, depth }); visit(c.id, depth + 1); }
    };
    visit(null, 0);
    const seen = new Set(result.map((r) => r.dept.id));
    for (const d of departments) if (!seen.has(d.id)) result.push({ dept: d, depth: 0 });
    return result;
  }, [departments]);

  const empName = (e: Employee) => e.name;

  function openCreate() { setEditing(null); setForm(emptyForm); setDialogOpen(true); }
  function openEdit(d: Department) {
    setEditing(d);
    setForm({
      nameAr: d.nameAr ?? "", nameEn: d.name, code: d.code ?? "", description: d.description ?? "",
      parentDepartmentId: d.parentDepartmentId ?? "", managerId: d.managerId ?? "",
      deputyManagerId: d.deputyManagerId ?? "", branchId: d.branchId ?? "", costCenterId: d.costCenterId ?? "",
      isActive: d.isActive,
    });
    setDialogOpen(true);
  }

  async function save() {
    if (!form.nameEn.trim() && !form.nameAr.trim()) { toast.error("اسم القسم مطلوب"); return; }
    setSaving(true);
    try {
      const payload = {
        name: form.nameEn.trim() || form.nameAr.trim(),
        nameAr: form.nameAr.trim() || undefined,
        code: form.code.trim() || undefined,
        description: form.description.trim() || undefined,
        parentDepartmentId: form.parentDepartmentId || null,
        managerId: form.managerId || null,
        deputyManagerId: form.deputyManagerId || null,
        branchId: form.branchId || null,
        costCenterId: form.costCenterId || null,
        isActive: form.isActive,
      };
      if (editing) { await updateDepartment(editing.id, payload); toast.success("تم تحديث القسم"); }
      else { await createDepartment(payload); toast.success("تمت إضافة القسم"); }
      setDialogOpen(false); await loadDepartments();
    } catch (err) { notifyError(err, "تعذر حفظ القسم"); } finally { setSaving(false); }
  }

  async function confirmDelete() {
    if (!deleteTarget) return;
    setDeleting(true);
    try { await deleteDepartment(deleteTarget.id); toast.success("تم حذف القسم"); setDeleteTarget(null); await loadDepartments(); }
    catch (err) { notifyError(err, "تعذر حذف القسم"); } finally { setDeleting(false); }
  }

  const parentOptions = departments.filter((d) => d.id !== editing?.id);

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2 text-sm">
        <Link href="/settings/organization" className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1">
          <ArrowRight className="h-4 w-4" /> إعدادات المؤسسة
        </Link>
        <span className="text-muted-foreground">/</span>
        <span>الأقسام</span>
      </div>

      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">الأقسام</h1>
          <p className="text-sm text-muted-foreground mt-1">الهيكل التنظيمي والتسلسل الإداري</p>
        </div>
        <div className="flex items-center gap-2">
          <Link href="/settings/organization/departments/chart" className="inline-flex items-center gap-2 h-10 px-4 border border-border bg-background text-sm font-bold hover:bg-muted transition-colors">
            <Network className="h-4 w-4" /> المخطط التنظيمي
          </Link>
          <Button onClick={openCreate} className="h-10 gap-2 font-bold uppercase tracking-wider text-sm"><Plus className="h-4 w-4" /> إضافة قسم</Button>
        </div>
      </div>

      <div className="border border-border">
        <Table>
          <TableHeader>
            <TableRow className="border-border hover:bg-transparent">
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">القسم</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">الرمز</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">المدير</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">الفرع</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">الحالة</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground w-24"></TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              <TableRow className="hover:bg-transparent"><TableCell colSpan={6} className="py-12 text-center text-sm text-muted-foreground"><Loader2 className="h-4 w-4 animate-spin inline" /> جاري التحميل...</TableCell></TableRow>
            ) : ordered.length === 0 ? (
              <TableRow className="hover:bg-transparent"><TableCell colSpan={6} className="py-12 text-center text-sm text-muted-foreground">لا توجد أقسام</TableCell></TableRow>
            ) : ordered.map(({ dept, depth }) => (
              <TableRow key={dept.id} className="border-border hover:bg-card/50">
                <TableCell className="font-medium">
                  <span style={{ paddingInlineStart: `${depth * 20}px` }} className="inline-flex items-center gap-1.5">
                    {depth > 0 && <CornerDownLeft className="h-3.5 w-3.5 text-muted-foreground rotate-180" />}
                    {dept.nameAr || dept.name}
                  </span>
                </TableCell>
                <TableCell className="font-mono text-xs text-muted-foreground">{dept.code || "—"}</TableCell>
                <TableCell className="text-sm text-muted-foreground">{dept.managerName || "—"}</TableCell>
                <TableCell className="text-sm text-muted-foreground">{dept.branchName || "—"}</TableCell>
                <TableCell>
                  {dept.isActive
                    ? <Badge variant="outline" className="text-xs bg-green-500/10 text-green-500 border-green-500/20">نشط</Badge>
                    : <Badge variant="outline" className="text-xs bg-zinc-500/10 text-zinc-400 border-zinc-500/20">غير نشط</Badge>}
                </TableCell>
                <TableCell>
                  <div className="flex items-center gap-1 justify-end">
                    <button onClick={() => openEdit(dept)} className="h-8 w-8 inline-flex items-center justify-center text-muted-foreground hover:text-foreground" title="تعديل"><Pencil className="h-4 w-4" /></button>
                    <button onClick={() => setDeleteTarget(dept)} className="h-8 w-8 inline-flex items-center justify-center text-destructive hover:text-destructive/80" title="حذف"><Trash2 className="h-4 w-4" /></button>
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <Dialog open={dialogOpen} onOpenChange={(o) => { if (!o && !saving) setDialogOpen(false); }}>
        <DialogContent className="sm:max-w-lg">
          <DialogHeader><DialogTitle>{editing ? "تعديل قسم" : "إضافة قسم"}</DialogTitle></DialogHeader>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 py-2 max-h-[60vh] overflow-y-auto">
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الاسم (عربي)</Label>
              <Input value={form.nameAr} onChange={(e) => setForm({ ...form, nameAr: e.target.value })} className="bg-secondary border-border" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الاسم (إنجليزي)</Label>
              <Input value={form.nameEn} onChange={(e) => setForm({ ...form, nameEn: e.target.value })} className="bg-secondary border-border" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الرمز</Label>
              <Input value={form.code} onChange={(e) => setForm({ ...form, code: e.target.value })} className="bg-secondary border-border font-mono" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">القسم الأب</Label>
              <select value={form.parentDepartmentId} onChange={(e) => setForm({ ...form, parentDepartmentId: e.target.value })} className="w-full h-9 bg-secondary border border-border px-3 text-sm text-foreground">
                <option value="">— لا يوجد —</option>
                {parentOptions.map((d) => <option key={d.id} value={d.id}>{d.nameAr || d.name}</option>)}
              </select>
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">المدير</Label>
              <select value={form.managerId} onChange={(e) => setForm({ ...form, managerId: e.target.value })} className="w-full h-9 bg-secondary border border-border px-3 text-sm text-foreground">
                <option value="">— لا يوجد —</option>
                {employees.map((e) => <option key={e.id} value={e.id}>{empName(e)}</option>)}
              </select>
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">نائب المدير</Label>
              <select value={form.deputyManagerId} onChange={(e) => setForm({ ...form, deputyManagerId: e.target.value })} className="w-full h-9 bg-secondary border border-border px-3 text-sm text-foreground">
                <option value="">— لا يوجد —</option>
                {employees.map((e) => <option key={e.id} value={e.id}>{empName(e)}</option>)}
              </select>
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الفرع</Label>
              <select value={form.branchId} onChange={(e) => setForm({ ...form, branchId: e.target.value })} className="w-full h-9 bg-secondary border border-border px-3 text-sm text-foreground">
                <option value="">— لا يوجد —</option>
                {branches.map((b) => <option key={b.id} value={b.id}>{orgLabel(b)}</option>)}
              </select>
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">مركز التكلفة</Label>
              <select value={form.costCenterId} onChange={(e) => setForm({ ...form, costCenterId: e.target.value })} className="w-full h-9 bg-secondary border border-border px-3 text-sm text-foreground">
                <option value="">— لا يوجد —</option>
                {costCenters.map((c) => <option key={c.id} value={c.id}>{lookupLabel(c)}</option>)}
              </select>
            </div>
            <label className="flex items-center gap-2 text-sm cursor-pointer sm:col-span-2">
              <input type="checkbox" checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} /> نشط
            </label>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)} disabled={saving}>إلغاء</Button>
            <Button onClick={save} disabled={saving} className="font-bold">{saving ? "جاري الحفظ..." : "حفظ"}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={!!deleteTarget} onOpenChange={(o) => { if (!o && !deleting) setDeleteTarget(null); }}>
        <DialogContent showCloseButton={false}>
          <DialogHeader>
            <DialogTitle>حذف قسم</DialogTitle>
            <DialogDescription>هل أنت متأكد من حذف <span className="font-bold text-foreground">{deleteTarget?.nameAr || deleteTarget?.name}</span>؟</DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteTarget(null)} disabled={deleting}>إلغاء</Button>
            <Button onClick={confirmDelete} disabled={deleting} className="bg-destructive text-white hover:bg-destructive/90">{deleting ? "جاري الحذف..." : "حذف"}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
