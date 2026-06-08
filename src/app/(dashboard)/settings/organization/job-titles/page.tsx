"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { ArrowRight, Plus, Pencil, Trash2, Loader2, Lock } from "lucide-react";
import { toast } from "sonner";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { ApiError } from "@/lib/api-client";
import {
  MasterDataItem, getMasterDataItems, createMasterDataItem, updateMasterDataItem, deleteMasterDataItem, parseMetadata,
} from "@/lib/api/master-data";
import { getLookup, lookupLabel, LookupItem } from "@/lib/api/lookups";
import { getDepartments, OrgOption, orgLabel } from "@/lib/api/org";

function notifyError(err: unknown, fallback: string) {
  if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
    toast.error(err instanceof ApiError ? err.message : fallback);
  }
}

interface JobTitleMeta {
  defaultDepartmentId: string;
  gradeId: string;
  requiredSkills: string[];
  responsibilities: string[];
  defaultTasks: string[];
  requiredDocuments: string[];
}

const emptyMeta: JobTitleMeta = {
  defaultDepartmentId: "", gradeId: "", requiredSkills: [], responsibilities: [], defaultTasks: [], requiredDocuments: [],
};

interface JobTitleForm {
  code: string;
  nameAr: string;
  nameEn: string;
  description: string;
  isActive: boolean;
  defaultDepartmentId: string;
  gradeId: string;
  requiredSkills: string;      // comma separated
  responsibilities: string;    // newline separated
  defaultTasks: string;        // newline separated
  requiredDocuments: string;   // newline separated
}

const emptyForm: JobTitleForm = {
  code: "", nameAr: "", nameEn: "", description: "", isActive: true,
  defaultDepartmentId: "", gradeId: "", requiredSkills: "", responsibilities: "", defaultTasks: "", requiredDocuments: "",
};

const linesToArr = (s: string) => s.split("\n").map((x) => x.trim()).filter(Boolean);
const commaToArr = (s: string) => s.split(",").map((x) => x.trim()).filter(Boolean);

export default function JobTitlesPage() {
  const [items, setItems] = useState<MasterDataItem[]>([]);
  const [departments, setDepartments] = useState<OrgOption[]>([]);
  const [grades, setGrades] = useState<LookupItem[]>([]);
  const [loading, setLoading] = useState(true);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<MasterDataItem | null>(null);
  const [form, setForm] = useState<JobTitleForm>(emptyForm);
  const [saving, setSaving] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<MasterDataItem | null>(null);
  const [deleting, setDeleting] = useState(false);

  const loadItems = useCallback(async () => {
    try { setItems(await getMasterDataItems("JobTitle", { includeInactive: true })); }
    catch (err) { notifyError(err, "تعذر تحميل المسميات"); }
  }, []);

  useEffect(() => {
    (async () => {
      setLoading(true);
      try {
        const [its, deps, grs] = await Promise.all([
          getMasterDataItems("JobTitle", { includeInactive: true }), getDepartments(), getLookup("grades"),
        ]);
        setItems(its); setDepartments(deps); setGrades(grs);
      } catch (err) { notifyError(err, "تعذر تحميل البيانات"); }
      finally { setLoading(false); }
    })();
  }, []);

  const deptName = useMemo(() => {
    const m = new Map(departments.map((d) => [d.id, orgLabel(d)]));
    return (id?: string) => (id ? m.get(id) ?? "—" : "—");
  }, [departments]);

  function openCreate() { setEditing(null); setForm(emptyForm); setDialogOpen(true); }
  function openEdit(i: MasterDataItem) {
    setEditing(i);
    const meta = parseMetadata<JobTitleMeta>(i, emptyMeta);
    setForm({
      code: i.code, nameAr: i.nameAr, nameEn: i.nameEn, description: i.description ?? "", isActive: i.isActive,
      defaultDepartmentId: meta.defaultDepartmentId ?? "", gradeId: meta.gradeId ?? "",
      requiredSkills: (meta.requiredSkills ?? []).join(", "),
      responsibilities: (meta.responsibilities ?? []).join("\n"),
      defaultTasks: (meta.defaultTasks ?? []).join("\n"),
      requiredDocuments: (meta.requiredDocuments ?? []).join("\n"),
    });
    setDialogOpen(true);
  }

  async function save() {
    if (!form.nameAr.trim() || !form.nameEn.trim()) { toast.error("الاسم بالعربية والإنجليزية مطلوبان"); return; }
    if (!editing && !form.code.trim()) { toast.error("الرمز مطلوب"); return; }
    setSaving(true);
    try {
      const meta: JobTitleMeta = {
        defaultDepartmentId: form.defaultDepartmentId,
        gradeId: form.gradeId,
        requiredSkills: commaToArr(form.requiredSkills),
        responsibilities: linesToArr(form.responsibilities),
        defaultTasks: linesToArr(form.defaultTasks),
        requiredDocuments: linesToArr(form.requiredDocuments),
      };
      const payload = {
        code: form.code.trim().toUpperCase(),
        nameAr: form.nameAr.trim(),
        nameEn: form.nameEn.trim(),
        description: form.description.trim() || undefined,
        isActive: form.isActive,
        metadata: meta as unknown as Record<string, unknown>,
      };
      if (editing) { await updateMasterDataItem(editing.id, payload); toast.success("تم تحديث المسمى"); }
      else { await createMasterDataItem("JobTitle", payload); toast.success("تمت إضافة المسمى"); }
      setDialogOpen(false); await loadItems();
    } catch (err) { notifyError(err, "تعذر حفظ المسمى"); } finally { setSaving(false); }
  }

  async function confirmDelete() {
    if (!deleteTarget) return;
    setDeleting(true);
    try { await deleteMasterDataItem(deleteTarget.id); toast.success("تم الحذف"); setDeleteTarget(null); await loadItems(); }
    catch (err) { notifyError(err, "تعذر الحذف (قد يكون مستخدماً)"); } finally { setDeleting(false); }
  }

  const taClass = "w-full bg-secondary border border-border px-3 py-2 text-sm text-foreground min-h-[72px]";
  const selectClass = "w-full h-9 bg-secondary border border-border px-3 text-sm text-foreground";

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2 text-sm">
        <Link href="/settings/organization" className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1">
          <ArrowRight className="h-4 w-4" /> إعدادات المؤسسة
        </Link>
        <span className="text-muted-foreground">/</span>
        <span>المسميات الوظيفية</span>
      </div>

      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">المسميات الوظيفية</h1>
          <p className="text-sm text-muted-foreground mt-1">المسميات ومسؤولياتها ومهاراتها ومستنداتها المطلوبة</p>
        </div>
        <Button onClick={openCreate} className="h-10 gap-2 font-bold uppercase tracking-wider text-sm"><Plus className="h-4 w-4" /> إضافة مسمى</Button>
      </div>

      <div className="border border-border">
        <Table>
          <TableHeader>
            <TableRow className="border-border hover:bg-transparent">
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">الرمز</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">المسمى</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">القسم الافتراضي</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">الحالة</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground w-24"></TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              <TableRow className="hover:bg-transparent"><TableCell colSpan={5} className="py-12 text-center text-sm text-muted-foreground"><Loader2 className="h-4 w-4 animate-spin inline" /> جاري التحميل...</TableCell></TableRow>
            ) : items.length === 0 ? (
              <TableRow className="hover:bg-transparent"><TableCell colSpan={5} className="py-12 text-center text-sm text-muted-foreground">لا توجد مسميات</TableCell></TableRow>
            ) : items.map((i) => {
              const meta = parseMetadata<JobTitleMeta>(i, emptyMeta);
              return (
                <TableRow key={i.id} className="border-border hover:bg-card/50">
                  <TableCell className="font-mono text-xs text-muted-foreground">{i.code}</TableCell>
                  <TableCell className="font-medium">{i.nameAr}</TableCell>
                  <TableCell className="text-sm text-muted-foreground">{deptName(meta.defaultDepartmentId)}</TableCell>
                  <TableCell>
                    {i.isActive
                      ? <Badge variant="outline" className="text-xs bg-green-500/10 text-green-500 border-green-500/20">نشط</Badge>
                      : <Badge variant="outline" className="text-xs bg-zinc-500/10 text-zinc-400 border-zinc-500/20">غير نشط</Badge>}
                  </TableCell>
                  <TableCell>
                    <div className="flex items-center gap-1 justify-end">
                      <button onClick={() => openEdit(i)} className="h-8 w-8 inline-flex items-center justify-center text-muted-foreground hover:text-foreground" title="تعديل"><Pencil className="h-4 w-4" /></button>
                      {i.isSystemDefault
                        ? <span className="h-8 w-8 inline-flex items-center justify-center text-muted-foreground/40" title="افتراضي"><Lock className="h-4 w-4" /></span>
                        : <button onClick={() => setDeleteTarget(i)} className="h-8 w-8 inline-flex items-center justify-center text-destructive hover:text-destructive/80" title="حذف"><Trash2 className="h-4 w-4" /></button>}
                    </div>
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      </div>

      <Dialog open={dialogOpen} onOpenChange={(o) => { if (!o && !saving) setDialogOpen(false); }}>
        <DialogContent className="sm:max-w-2xl">
          <DialogHeader><DialogTitle>{editing ? "تعديل مسمى وظيفي" : "إضافة مسمى وظيفي"}</DialogTitle></DialogHeader>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 py-2 max-h-[65vh] overflow-y-auto">
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الرمز</Label>
              <Input value={form.code} onChange={(e) => setForm({ ...form, code: e.target.value })} disabled={!!editing} className="bg-secondary border-border font-mono disabled:opacity-60" />
            </div>
            <label className="flex items-center gap-2 text-sm cursor-pointer pt-6">
              <input type="checkbox" checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} /> نشط
            </label>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الاسم (عربي)</Label>
              <Input value={form.nameAr} onChange={(e) => setForm({ ...form, nameAr: e.target.value })} className="bg-secondary border-border" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الاسم (إنجليزي)</Label>
              <Input value={form.nameEn} onChange={(e) => setForm({ ...form, nameEn: e.target.value })} className="bg-secondary border-border" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">القسم الافتراضي</Label>
              <select value={form.defaultDepartmentId} onChange={(e) => setForm({ ...form, defaultDepartmentId: e.target.value })} className={selectClass}>
                <option value="">— لا يوجد —</option>
                {departments.map((d) => <option key={d.id} value={d.id}>{orgLabel(d)}</option>)}
              </select>
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الدرجة الوظيفية</Label>
              <select value={form.gradeId} onChange={(e) => setForm({ ...form, gradeId: e.target.value })} className={selectClass}>
                <option value="">— لا يوجد —</option>
                {grades.map((g) => <option key={g.id} value={g.id}>{lookupLabel(g)}</option>)}
              </select>
            </div>
            <div className="space-y-2 sm:col-span-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الوصف</Label>
              <Input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} className="bg-secondary border-border" />
            </div>
            <div className="space-y-2 sm:col-span-2">
              <Label className="text-xs font-bold uppercase tracking-wider">المهارات المطلوبة (مفصولة بفاصلة)</Label>
              <Input value={form.requiredSkills} onChange={(e) => setForm({ ...form, requiredSkills: e.target.value })} placeholder="القيادة, التواصل, Excel" className="bg-secondary border-border" />
            </div>
            <div className="space-y-2 sm:col-span-2">
              <Label className="text-xs font-bold uppercase tracking-wider">المسؤوليات (سطر لكل عنصر)</Label>
              <textarea value={form.responsibilities} onChange={(e) => setForm({ ...form, responsibilities: e.target.value })} className={taClass} />
            </div>
            <div className="space-y-2 sm:col-span-2">
              <Label className="text-xs font-bold uppercase tracking-wider">المهام الافتراضية عند التعيين (سطر لكل مهمة)</Label>
              <textarea value={form.defaultTasks} onChange={(e) => setForm({ ...form, defaultTasks: e.target.value })} className={taClass} />
            </div>
            <div className="space-y-2 sm:col-span-2">
              <Label className="text-xs font-bold uppercase tracking-wider">المستندات المطلوبة (سطر لكل مستند)</Label>
              <textarea value={form.requiredDocuments} onChange={(e) => setForm({ ...form, requiredDocuments: e.target.value })} className={taClass} />
            </div>
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
            <DialogTitle>حذف مسمى وظيفي</DialogTitle>
            <DialogDescription>هل أنت متأكد من حذف <span className="font-bold text-foreground">{deleteTarget?.nameAr}</span>؟</DialogDescription>
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
