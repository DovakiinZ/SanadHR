"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { ArrowRight, Plus, Pencil, Trash2, Loader2, Star, MapPin } from "lucide-react";
import { toast } from "sonner";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { ApiError } from "@/lib/api-client";
import { Branch, BranchInput, listBranches, createBranch, updateBranch, deleteBranch } from "@/lib/api/org";

function notifyError(err: unknown, fallback: string) {
  if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
    toast.error(err instanceof ApiError ? err.message : fallback);
  }
}

const emptyForm: BranchInput = {
  name: "", nameAr: "", code: "", city: "", address: "", phone: "", isMainBranch: false,
  latitude: null, longitude: null, geofenceRadiusMeters: null, isActive: true,
};

const numOrNull = (v: string): number | null => (v.trim() === "" ? null : Number(v));

export default function BranchesPage() {
  const [branches, setBranches] = useState<Branch[]>([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<Branch | null>(null);
  const [form, setForm] = useState<BranchInput>(emptyForm);
  const [saving, setSaving] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<Branch | null>(null);
  const [deleting, setDeleting] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try { setBranches(await listBranches()); }
    catch (err) { notifyError(err, "تعذر تحميل الفروع"); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => { load(); }, [load]);

  function openCreate() { setEditing(null); setForm(emptyForm); setDialogOpen(true); }
  function openEdit(b: Branch) {
    setEditing(b);
    setForm({
      name: b.name, nameAr: b.nameAr ?? "", code: b.code ?? "", city: b.city ?? "",
      address: b.address ?? "", phone: b.phone ?? "", isMainBranch: b.isMainBranch,
      latitude: b.latitude ?? null, longitude: b.longitude ?? null,
      geofenceRadiusMeters: b.geofenceRadiusMeters ?? null, isActive: b.isActive,
    });
    setDialogOpen(true);
  }

  async function save() {
    if (!form.name.trim()) { toast.error("اسم الفرع مطلوب"); return; }
    setSaving(true);
    try {
      if (editing) { await updateBranch(editing.id, form); toast.success("تم تحديث الفرع"); }
      else { await createBranch(form); toast.success("تمت إضافة الفرع"); }
      setDialogOpen(false); await load();
    } catch (err) { notifyError(err, "تعذر حفظ الفرع"); } finally { setSaving(false); }
  }

  async function confirmDelete() {
    if (!deleteTarget) return;
    setDeleting(true);
    try { await deleteBranch(deleteTarget.id); toast.success("تم حذف الفرع"); setDeleteTarget(null); await load(); }
    catch (err) { notifyError(err, "تعذر حذف الفرع"); } finally { setDeleting(false); }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2 text-sm">
        <Link href="/settings/organization" className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1">
          <ArrowRight className="h-4 w-4" /> إعدادات المؤسسة
        </Link>
        <span className="text-muted-foreground">/</span>
        <span>الفروع</span>
      </div>

      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">الفروع</h1>
          <p className="text-sm text-muted-foreground mt-1">فروع المؤسسة ومواقعها</p>
        </div>
        <Button onClick={openCreate} className="h-10 gap-2 font-bold uppercase tracking-wider text-sm"><Plus className="h-4 w-4" /> إضافة فرع</Button>
      </div>

      <div className="border border-border">
        <Table>
          <TableHeader>
            <TableRow className="border-border hover:bg-transparent">
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">الفرع</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">المدينة</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">الإحداثيات / النطاق</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">الحالة</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground w-24"></TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              <TableRow className="hover:bg-transparent"><TableCell colSpan={5} className="py-12 text-center text-sm text-muted-foreground"><Loader2 className="h-4 w-4 animate-spin inline" /> جاري التحميل...</TableCell></TableRow>
            ) : branches.length === 0 ? (
              <TableRow className="hover:bg-transparent"><TableCell colSpan={5} className="py-12 text-center text-sm text-muted-foreground">لا توجد فروع</TableCell></TableRow>
            ) : branches.map((b) => (
              <TableRow key={b.id} className="border-border hover:bg-card/50">
                <TableCell className="font-medium">
                  <span className="flex items-center gap-2">
                    {b.nameAr || b.name}
                    {b.isMainBranch && <Star className="h-3.5 w-3.5 text-yellow-500 fill-yellow-500" />}
                  </span>
                </TableCell>
                <TableCell className="text-sm text-muted-foreground">{b.city || "—"}</TableCell>
                <TableCell className="text-xs text-muted-foreground font-mono">
                  {b.latitude != null && b.longitude != null
                    ? <span className="inline-flex items-center gap-1"><MapPin className="h-3 w-3" />{b.latitude.toFixed(4)}, {b.longitude.toFixed(4)}{b.geofenceRadiusMeters ? ` · ${b.geofenceRadiusMeters}m` : ""}</span>
                    : "—"}
                </TableCell>
                <TableCell>
                  {b.isActive
                    ? <span className="text-xs text-green-500">نشط</span>
                    : <span className="text-xs text-zinc-400">غير نشط</span>}
                </TableCell>
                <TableCell>
                  <div className="flex items-center gap-1 justify-end">
                    <button onClick={() => openEdit(b)} className="h-8 w-8 inline-flex items-center justify-center text-muted-foreground hover:text-foreground" title="تعديل"><Pencil className="h-4 w-4" /></button>
                    <button onClick={() => setDeleteTarget(b)} className="h-8 w-8 inline-flex items-center justify-center text-destructive hover:text-destructive/80" title="حذف"><Trash2 className="h-4 w-4" /></button>
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <Dialog open={dialogOpen} onOpenChange={(o) => { if (!o && !saving) setDialogOpen(false); }}>
        <DialogContent>
          <DialogHeader><DialogTitle>{editing ? "تعديل فرع" : "إضافة فرع"}</DialogTitle></DialogHeader>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 py-2 max-h-[65vh] overflow-y-auto pl-1">
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الاسم (عربي)</Label>
              <Input value={form.nameAr} onChange={(e) => setForm({ ...form, nameAr: e.target.value })} className="bg-secondary border-border" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الاسم (إنجليزي)</Label>
              <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} className="bg-secondary border-border" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الرمز</Label>
              <Input value={form.code ?? ""} onChange={(e) => setForm({ ...form, code: e.target.value })} className="bg-secondary border-border font-mono" placeholder="RUH" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">المدينة</Label>
              <Input value={form.city} onChange={(e) => setForm({ ...form, city: e.target.value })} className="bg-secondary border-border" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الهاتف</Label>
              <Input value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} className="bg-secondary border-border" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">العنوان</Label>
              <Input value={form.address} onChange={(e) => setForm({ ...form, address: e.target.value })} className="bg-secondary border-border" />
            </div>

            <div className="sm:col-span-2 border-t border-border pt-3 mt-1">
              <p className="text-xs font-bold uppercase tracking-wider text-primary mb-1">الموقع الجغرافي (لحضور النطاق الجغرافي)</p>
              <p className="text-[11px] text-muted-foreground">تُستخدم الإحداثيات للسماح بتسجيل الحضور من موقع الفرع عند تفعيل الحضور بالنطاق الجغرافي.</p>
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">خط العرض (Latitude)</Label>
              <Input type="number" step="any" value={form.latitude ?? ""} onChange={(e) => setForm({ ...form, latitude: numOrNull(e.target.value) })} className="bg-secondary border-border font-mono" placeholder="24.7136" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">خط الطول (Longitude)</Label>
              <Input type="number" step="any" value={form.longitude ?? ""} onChange={(e) => setForm({ ...form, longitude: numOrNull(e.target.value) })} className="bg-secondary border-border font-mono" placeholder="46.6753" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">نطاق الحضور (متر)</Label>
              <Input type="number" min={0} value={form.geofenceRadiusMeters ?? ""} onChange={(e) => setForm({ ...form, geofenceRadiusMeters: numOrNull(e.target.value) })} className="bg-secondary border-border" placeholder="150" />
            </div>

            <div className="sm:col-span-2 flex items-center gap-6 border-t border-border pt-3 mt-1">
              <label className="flex items-center gap-2 text-sm cursor-pointer">
                <input type="checkbox" checked={form.isMainBranch} onChange={(e) => setForm({ ...form, isMainBranch: e.target.checked })} /> الفرع الرئيسي
              </label>
              <label className="flex items-center gap-2 text-sm cursor-pointer">
                <input type="checkbox" checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} /> نشط
              </label>
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
            <DialogTitle>حذف فرع</DialogTitle>
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
