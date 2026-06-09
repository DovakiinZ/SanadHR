"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { ArrowRight, Plus, Pencil, Trash2, Loader2, Lock, Percent, Coins } from "lucide-react";
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

function notifyError(err: unknown, fallback: string) {
  if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
    toast.error(err instanceof ApiError ? err.message : fallback);
  }
}

type CalcType = "Fixed" | "Percentage";
type PctBase = "Basic" | "Gross";

interface AllowanceMeta {
  calculationType: CalcType;
  defaultValue: number | null;
  percentageBase: PctBase;
  recurring: boolean;
  gosiApplicable: boolean;
  taxable: boolean;
  allowEmployeeOverride: boolean;
  effectiveDate: string;
}

const emptyMeta: AllowanceMeta = {
  calculationType: "Fixed", defaultValue: null, percentageBase: "Basic",
  recurring: true, gosiApplicable: false, taxable: false, allowEmployeeOverride: true, effectiveDate: "",
};

interface AllowanceForm extends AllowanceMeta {
  code: string; nameAr: string; nameEn: string; description: string; isActive: boolean;
}

const emptyForm: AllowanceForm = {
  ...emptyMeta, code: "", nameAr: "", nameEn: "", description: "", isActive: true,
};

export default function AllowancesPage() {
  const [items, setItems] = useState<MasterDataItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<MasterDataItem | null>(null);
  const [form, setForm] = useState<AllowanceForm>(emptyForm);
  const [saving, setSaving] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<MasterDataItem | null>(null);
  const [deleting, setDeleting] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try { setItems(await getMasterDataItems("AllowanceType", { includeInactive: true })); }
    catch (err) { notifyError(err, "تعذر تحميل البدلات"); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => { load(); }, [load]);

  function openCreate() { setEditing(null); setForm(emptyForm); setDialogOpen(true); }
  function openEdit(i: MasterDataItem) {
    setEditing(i);
    const meta = parseMetadata<AllowanceMeta>(i, emptyMeta);
    setForm({
      ...meta,
      code: i.code, nameAr: i.nameAr, nameEn: i.nameEn, description: i.description ?? "", isActive: i.isActive,
    });
    setDialogOpen(true);
  }

  async function save() {
    if (!form.nameAr.trim() || !form.nameEn.trim()) { toast.error("الاسم بالعربية والإنجليزية مطلوبان"); return; }
    if (!editing && !form.code.trim()) { toast.error("الرمز مطلوب"); return; }
    setSaving(true);
    try {
      const meta: AllowanceMeta = {
        calculationType: form.calculationType,
        defaultValue: form.defaultValue === null || Number.isNaN(form.defaultValue) ? null : Number(form.defaultValue),
        percentageBase: form.percentageBase,
        recurring: form.recurring,
        gosiApplicable: form.gosiApplicable,
        taxable: form.taxable,
        allowEmployeeOverride: form.allowEmployeeOverride,
        effectiveDate: form.effectiveDate,
      };
      const payload = {
        code: form.code.trim().toUpperCase(),
        nameAr: form.nameAr.trim(),
        nameEn: form.nameEn.trim(),
        description: form.description.trim() || undefined,
        isActive: form.isActive,
        metadata: meta as unknown as Record<string, unknown>,
      };
      if (editing) { await updateMasterDataItem(editing.id, payload); toast.success("تم تحديث البدل"); }
      else { await createMasterDataItem("AllowanceType", payload); toast.success("تمت إضافة البدل"); }
      setDialogOpen(false); await load();
    } catch (err) { notifyError(err, "تعذر حفظ البدل"); } finally { setSaving(false); }
  }

  async function confirmDelete() {
    if (!deleteTarget) return;
    setDeleting(true);
    try { await deleteMasterDataItem(deleteTarget.id); toast.success("تم الحذف"); setDeleteTarget(null); await load(); }
    catch (err) { notifyError(err, "تعذر الحذف (قد يكون مستخدماً)"); } finally { setDeleting(false); }
  }

  const selectClass = "w-full h-9 bg-secondary border border-border px-3 text-sm text-foreground";

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2 text-sm">
        <Link href="/settings/payroll" className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1">
          <ArrowRight className="h-4 w-4" /> إعدادات الرواتب
        </Link>
        <span className="text-muted-foreground">/</span>
        <span>البدلات</span>
      </div>

      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">أنواع البدلات</h1>
          <p className="text-sm text-muted-foreground mt-1">عرّف البدلات وقواعد احتسابها — تُستخدم في قسم التعويضات بملف الموظف</p>
        </div>
        <Button onClick={openCreate} className="h-10 gap-2 font-bold uppercase tracking-wider text-sm"><Plus className="h-4 w-4" /> بدل</Button>
      </div>

      <div className="border border-border">
        <Table>
          <TableHeader>
            <TableRow className="border-border hover:bg-transparent">
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">البدل</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">الاحتساب</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">الافتراضي</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">خصائص</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">الحالة</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground w-24"></TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              <TableRow className="hover:bg-transparent"><TableCell colSpan={6} className="py-12 text-center text-sm text-muted-foreground"><Loader2 className="h-4 w-4 animate-spin inline" /> جاري التحميل...</TableCell></TableRow>
            ) : items.length === 0 ? (
              <TableRow className="hover:bg-transparent"><TableCell colSpan={6} className="py-12 text-center text-sm text-muted-foreground">لا توجد بدلات</TableCell></TableRow>
            ) : items.map((i) => {
              const meta = parseMetadata<AllowanceMeta>(i, emptyMeta);
              const pct = meta.calculationType === "Percentage";
              return (
                <TableRow key={i.id} className="border-border hover:bg-card/50">
                  <TableCell><div className="font-medium">{i.nameAr}</div><div className="font-mono text-[10px] text-muted-foreground">{i.code}</div></TableCell>
                  <TableCell className="text-sm text-muted-foreground">
                    <span className="inline-flex items-center gap-1">
                      {pct ? <Percent className="h-3.5 w-3.5" /> : <Coins className="h-3.5 w-3.5" />}
                      {pct ? `نسبة (${meta.percentageBase === "Gross" ? "الإجمالي" : "الأساسي"})` : "مبلغ ثابت"}
                    </span>
                  </TableCell>
                  <TableCell className="text-sm text-muted-foreground font-mono">{meta.defaultValue != null ? (pct ? `${meta.defaultValue}%` : meta.defaultValue) : "—"}</TableCell>
                  <TableCell>
                    <div className="flex flex-wrap gap-1">
                      {meta.gosiApplicable && <Badge variant="outline" className="text-[10px] border-border text-muted-foreground">GOSI</Badge>}
                      {meta.taxable && <Badge variant="outline" className="text-[10px] border-border text-muted-foreground">ضريبة</Badge>}
                      {meta.allowEmployeeOverride && <Badge variant="outline" className="text-[10px] border-border text-muted-foreground">تجاوز</Badge>}
                    </div>
                  </TableCell>
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
          <DialogHeader><DialogTitle>{editing ? "تعديل بدل" : "بدل جديد"}</DialogTitle></DialogHeader>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 py-2 max-h-[68vh] overflow-y-auto pl-1">
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الرمز</Label>
              <Input value={form.code} onChange={(e) => setForm({ ...form, code: e.target.value })} disabled={!!editing} className="bg-secondary border-border font-mono disabled:opacity-60" placeholder="HOUSING" />
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
            <div className="space-y-2 sm:col-span-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الوصف</Label>
              <Input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} className="bg-secondary border-border" />
            </div>

            <div className="sm:col-span-2 border-t border-border pt-2 text-xs font-bold uppercase tracking-wider text-primary">قاعدة الاحتساب</div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">نوع الاحتساب</Label>
              <select value={form.calculationType} onChange={(e) => setForm({ ...form, calculationType: e.target.value as CalcType })} className={selectClass}>
                <option value="Fixed">مبلغ ثابت</option>
                <option value="Percentage">نسبة مئوية</option>
              </select>
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">{form.calculationType === "Percentage" ? "النسبة الافتراضية %" : "المبلغ الافتراضي"}</Label>
              <Input type="number" step="any" min={0} value={form.defaultValue ?? ""} onChange={(e) => setForm({ ...form, defaultValue: e.target.value === "" ? null : Number(e.target.value) })} className="bg-secondary border-border" />
            </div>
            {form.calculationType === "Percentage" && (
              <div className="space-y-2">
                <Label className="text-xs font-bold uppercase tracking-wider">أساس النسبة</Label>
                <select value={form.percentageBase} onChange={(e) => setForm({ ...form, percentageBase: e.target.value as PctBase })} className={selectClass}>
                  <option value="Basic">الراتب الأساسي</option>
                  <option value="Gross">الراتب الإجمالي</option>
                </select>
              </div>
            )}
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">تاريخ السريان</Label>
              <Input type="date" value={form.effectiveDate} onChange={(e) => setForm({ ...form, effectiveDate: e.target.value })} className="bg-secondary border-border" />
            </div>

            <div className="sm:col-span-2 border-t border-border pt-2 text-xs font-bold uppercase tracking-wider text-primary">الخصائص</div>
            <div className="sm:col-span-2 grid grid-cols-1 sm:grid-cols-2 gap-2">
              <label className="flex items-center gap-2 text-sm cursor-pointer border border-border px-3 py-2"><input type="checkbox" checked={form.recurring} onChange={(e) => setForm({ ...form, recurring: e.target.checked })} /> متكرر شهرياً</label>
              <label className="flex items-center gap-2 text-sm cursor-pointer border border-border px-3 py-2"><input type="checkbox" checked={form.gosiApplicable} onChange={(e) => setForm({ ...form, gosiApplicable: e.target.checked })} /> خاضع للتأمينات (GOSI)</label>
              <label className="flex items-center gap-2 text-sm cursor-pointer border border-border px-3 py-2"><input type="checkbox" checked={form.taxable} onChange={(e) => setForm({ ...form, taxable: e.target.checked })} /> خاضع للضريبة</label>
              <label className="flex items-center gap-2 text-sm cursor-pointer border border-border px-3 py-2"><input type="checkbox" checked={form.allowEmployeeOverride} onChange={(e) => setForm({ ...form, allowEmployeeOverride: e.target.checked })} /> يسمح بالتجاوز لكل موظف</label>
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
            <DialogTitle>حذف بدل</DialogTitle>
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
