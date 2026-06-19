"use client";

import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import Link from "next/link";
import { ArrowRight, Plus, Pencil, Trash2, Search, Loader2, Lock } from "lucide-react";
import { toast } from "sonner";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import {
  Table, TableBody, TableCell, TableHead, TableHeader, TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import {
  Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle,
} from "@/components/ui/dialog";
import { ApiError } from "@/lib/api-client";
import { transliterateArabic } from "@/lib/transliterate";
import {
  MasterDataItem, getMasterDataItems, createMasterDataItem, updateMasterDataItem, deleteMasterDataItem,
} from "@/lib/api/master-data";

function notifyError(err: unknown, fallback: string) {
  if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
    toast.error(err instanceof ApiError ? err.message : fallback);
  }
}

interface Props {
  objectType: string;
  title: string;
  description?: string;
  backHref?: string;
  backLabel?: string;
}

interface ItemForm {
  code: string;
  nameAr: string;
  nameEn: string;
  description: string;
  isActive: boolean;
}

const emptyForm: ItemForm = { code: "", nameAr: "", nameEn: "", description: "", isActive: true };

export function SimpleMasterDataList({ objectType, title, description, backHref = "/settings", backLabel = "الإعدادات" }: Props) {
  const [items, setItems] = useState<MasterDataItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<MasterDataItem | null>(null);
  const [form, setForm] = useState<ItemForm>(emptyForm);
  const [saving, setSaving] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<MasterDataItem | null>(null);
  const [deleting, setDeleting] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      setItems(await getMasterDataItems(objectType, { includeInactive: true }));
    } catch (err) {
      notifyError(err, "تعذر تحميل البيانات");
      setItems([]);
    } finally {
      setLoading(false);
    }
  }, [objectType]);

  useEffect(() => { load(); }, [load]);

  const filtered = useMemo(() => {
    if (!search) return items;
    const s = search.toLowerCase();
    return items.filter((i) => i.nameAr.includes(search) || i.nameEn.toLowerCase().includes(s) || i.code.toLowerCase().includes(s));
  }, [items, search]);

  // Auto-fill the English name from Arabic (transliteration) until the user edits it.
  const enTouched = useRef(false);
  function openCreate() { enTouched.current = false; setEditing(null); setForm(emptyForm); setDialogOpen(true); }
  function openEdit(i: MasterDataItem) {
    enTouched.current = !!i.nameEn;
    setEditing(i);
    setForm({ code: i.code, nameAr: i.nameAr, nameEn: i.nameEn, description: i.description ?? "", isActive: i.isActive });
    setDialogOpen(true);
  }
  const onNameAr = (v: string) => setForm((f) => ({ ...f, nameAr: v, ...(enTouched.current ? {} : { nameEn: transliterateArabic(v) }) }));
  const onNameEn = (v: string) => { enTouched.current = true; setForm((f) => ({ ...f, nameEn: v })); };

  async function save() {
    if (!form.nameAr.trim() || !form.nameEn.trim()) { toast.error("الاسم بالعربية والإنجليزية مطلوبان"); return; }
    if (!editing && !form.code.trim()) { toast.error("الرمز مطلوب"); return; }
    setSaving(true);
    try {
      const payload = {
        code: form.code.trim().toUpperCase(),
        nameAr: form.nameAr.trim(),
        nameEn: form.nameEn.trim(),
        description: form.description.trim() || undefined,
        isActive: form.isActive,
      };
      if (editing) { await updateMasterDataItem(editing.id, payload); toast.success("تم التحديث"); }
      else { await createMasterDataItem(objectType, payload); toast.success("تمت الإضافة"); }
      setDialogOpen(false);
      await load();
    } catch (err) { notifyError(err, "تعذر الحفظ"); } finally { setSaving(false); }
  }

  async function confirmDelete() {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await deleteMasterDataItem(deleteTarget.id);
      toast.success("تم الحذف");
      setDeleteTarget(null);
      await load();
    } catch (err) { notifyError(err, "تعذر الحذف (قد يكون مستخدماً)"); } finally { setDeleting(false); }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2 text-sm">
        <Link href={backHref} className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1">
          <ArrowRight className="h-4 w-4" /> {backLabel}
        </Link>
        <span className="text-muted-foreground">/</span>
        <span>{title}</span>
      </div>

      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{title}</h1>
          {description && <p className="text-sm text-muted-foreground mt-1">{description}</p>}
        </div>
        <Button onClick={openCreate} className="h-10 gap-2 font-bold uppercase tracking-wider text-sm">
          <Plus className="h-4 w-4" /> إضافة
        </Button>
      </div>

      <div className="relative max-w-sm">
        <Search className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
        <Input placeholder="بحث..." value={search} onChange={(e) => setSearch(e.target.value)} className="pr-10 bg-secondary border-border h-9 text-sm" />
      </div>

      <div className="border border-border">
        <Table>
          <TableHeader>
            <TableRow className="border-border hover:bg-transparent">
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">الرمز</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">الاسم (عربي)</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">الاسم (إنجليزي)</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">الحالة</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground w-24"></TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              <TableRow className="hover:bg-transparent"><TableCell colSpan={5} className="py-12 text-center text-sm text-muted-foreground"><Loader2 className="h-4 w-4 animate-spin inline" /> جاري التحميل...</TableCell></TableRow>
            ) : filtered.length === 0 ? (
              <TableRow className="hover:bg-transparent"><TableCell colSpan={5} className="py-12 text-center text-sm text-muted-foreground">لا توجد عناصر</TableCell></TableRow>
            ) : filtered.map((i) => (
              <TableRow key={i.id} className="border-border hover:bg-card/50">
                <TableCell className="font-mono text-xs text-muted-foreground">{i.code}</TableCell>
                <TableCell className="font-medium">{i.nameAr}</TableCell>
                <TableCell className="text-sm text-muted-foreground">{i.nameEn}</TableCell>
                <TableCell>
                  {i.isActive
                    ? <Badge variant="outline" className="text-xs bg-green-500/10 text-green-500 border-green-500/20">نشط</Badge>
                    : <Badge variant="outline" className="text-xs bg-zinc-500/10 text-zinc-400 border-zinc-500/20">غير نشط</Badge>}
                </TableCell>
                <TableCell>
                  <div className="flex items-center gap-1 justify-end">
                    <button onClick={() => openEdit(i)} className="h-8 w-8 inline-flex items-center justify-center text-muted-foreground hover:text-foreground" title="تعديل"><Pencil className="h-4 w-4" /></button>
                    {i.isSystemDefault
                      ? <span className="h-8 w-8 inline-flex items-center justify-center text-muted-foreground/40" title="افتراضي للنظام"><Lock className="h-4 w-4" /></span>
                      : <button onClick={() => setDeleteTarget(i)} className="h-8 w-8 inline-flex items-center justify-center text-destructive hover:text-destructive/80" title="حذف"><Trash2 className="h-4 w-4" /></button>}
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <Dialog open={dialogOpen} onOpenChange={(o) => { if (!o && !saving) setDialogOpen(false); }}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editing ? "تعديل" : "إضافة"} — {title}</DialogTitle>
          </DialogHeader>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 py-2">
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الرمز</Label>
              <Input value={form.code} onChange={(e) => setForm({ ...form, code: e.target.value })} disabled={!!editing} className="bg-secondary border-border font-mono disabled:opacity-60" />
            </div>
            <div className="space-y-2 flex items-end">
              <label className="flex items-center gap-2 text-sm cursor-pointer">
                <input type="checkbox" checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} /> نشط
              </label>
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الاسم (عربي)</Label>
              <Input value={form.nameAr} onChange={(e) => onNameAr(e.target.value)} className="bg-secondary border-border" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الاسم (إنجليزي)</Label>
              <Input value={form.nameEn} onChange={(e) => onNameEn(e.target.value)} className="bg-secondary border-border" />
            </div>
            <div className="space-y-2 sm:col-span-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الوصف</Label>
              <Input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} className="bg-secondary border-border" />
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
            <DialogTitle>حذف</DialogTitle>
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
