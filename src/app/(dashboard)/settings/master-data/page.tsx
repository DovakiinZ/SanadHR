"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { ArrowRight, Plus, Pencil, Trash2, Search, Loader2, Database, Lock } from "lucide-react";
import { toast } from "sonner";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { ApiError } from "@/lib/api-client";
import {
  MasterDataItem,
  MasterDataType,
  getMasterDataItems,
  getMasterDataTypes,
  createMasterDataItem,
  updateMasterDataItem,
  deactivateMasterDataItem,
  deleteMasterDataItem,
  typeLabelAr,
} from "@/lib/api/master-data";

function notifyError(err: unknown, fallback: string) {
  if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
    toast.error(err instanceof ApiError ? err.message : fallback);
  }
}

interface ItemForm {
  code: string;
  nameAr: string;
  nameEn: string;
  description: string;
  color: string;
  sortOrder: string;
  isActive: boolean;
}

const emptyForm: ItemForm = {
  code: "",
  nameAr: "",
  nameEn: "",
  description: "",
  color: "",
  sortOrder: "0",
  isActive: true,
};

export default function MasterDataAdminPage() {
  const [types, setTypes] = useState<MasterDataType[]>([]);
  const [selected, setSelected] = useState<string | null>(null);
  const [items, setItems] = useState<MasterDataItem[]>([]);
  const [loadingTypes, setLoadingTypes] = useState(true);
  const [loadingItems, setLoadingItems] = useState(false);
  const [search, setSearch] = useState("");
  const [includeInactive, setIncludeInactive] = useState(true);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<MasterDataItem | null>(null);
  const [form, setForm] = useState<ItemForm>(emptyForm);
  const [saving, setSaving] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<MasterDataItem | null>(null);
  const [deleting, setDeleting] = useState(false);

  const loadTypes = useCallback(async () => {
    setLoadingTypes(true);
    try {
      const t = await getMasterDataTypes();
      setTypes(t);
      setSelected((cur) => cur ?? t[0]?.objectType ?? null);
    } catch (err) {
      notifyError(err, "تعذر تحميل أنواع البيانات");
    } finally {
      setLoadingTypes(false);
    }
  }, []);

  const loadItems = useCallback(async (objectType: string) => {
    setLoadingItems(true);
    try {
      const list = await getMasterDataItems(objectType, { includeInactive: true });
      setItems(list);
    } catch (err) {
      notifyError(err, "تعذر تحميل العناصر");
      setItems([]);
    } finally {
      setLoadingItems(false);
    }
  }, []);

  useEffect(() => {
    loadTypes();
  }, [loadTypes]);

  useEffect(() => {
    if (selected) loadItems(selected);
  }, [selected, loadItems]);

  const filtered = useMemo(() => {
    let r = items;
    if (!includeInactive) r = r.filter((i) => i.isActive);
    if (search) {
      const s = search.toLowerCase();
      r = r.filter(
        (i) =>
          i.nameAr.includes(search) ||
          i.nameEn.toLowerCase().includes(s) ||
          i.code.toLowerCase().includes(s)
      );
    }
    return r;
  }, [items, includeInactive, search]);

  function openCreate() {
    setEditing(null);
    setForm(emptyForm);
    setDialogOpen(true);
  }

  function openEdit(item: MasterDataItem) {
    setEditing(item);
    setForm({
      code: item.code,
      nameAr: item.nameAr,
      nameEn: item.nameEn,
      description: item.description ?? "",
      color: item.color ?? "",
      sortOrder: String(item.sortOrder ?? 0),
      isActive: item.isActive,
    });
    setDialogOpen(true);
  }

  async function saveItem() {
    if (!selected) return;
    if (!form.nameAr.trim() || !form.nameEn.trim()) {
      toast.error("الاسم بالعربية والإنجليزية مطلوبان");
      return;
    }
    if (!editing && !form.code.trim()) {
      toast.error("الرمز (Code) مطلوب");
      return;
    }
    setSaving(true);
    try {
      const payload = {
        code: form.code.trim().toUpperCase(),
        nameAr: form.nameAr.trim(),
        nameEn: form.nameEn.trim(),
        description: form.description.trim() || undefined,
        color: form.color.trim() || undefined,
        sortOrder: Number(form.sortOrder) || 0,
        isActive: form.isActive,
      };
      if (editing) {
        await updateMasterDataItem(editing.id, payload);
        toast.success("تم تحديث العنصر");
      } else {
        await createMasterDataItem(selected, payload);
        toast.success("تمت إضافة العنصر");
      }
      setDialogOpen(false);
      await loadItems(selected);
      await loadTypes();
    } catch (err) {
      notifyError(err, "تعذر حفظ العنصر");
    } finally {
      setSaving(false);
    }
  }

  async function doDeactivate(item: MasterDataItem) {
    if (!selected) return;
    try {
      await deactivateMasterDataItem(item.id);
      toast.success("تم إلغاء تنشيط العنصر");
      await loadItems(selected);
    } catch (err) {
      notifyError(err, "تعذر إلغاء التنشيط");
    }
  }

  async function confirmDelete() {
    if (!deleteTarget || !selected) return;
    setDeleting(true);
    try {
      await deleteMasterDataItem(deleteTarget.id);
      toast.success("تم حذف العنصر");
      setDeleteTarget(null);
      await loadItems(selected);
      await loadTypes();
    } catch (err) {
      notifyError(err, "تعذر حذف العنصر (قد يكون مستخدماً أو افتراضياً)");
    } finally {
      setDeleting(false);
    }
  }

  return (
    <div className="space-y-6">
      {/* Breadcrumb */}
      <div className="flex items-center gap-2 text-sm">
        <Link href="/settings" className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1">
          <ArrowRight className="h-4 w-4" />
          الإعدادات
        </Link>
        <span className="text-muted-foreground">/</span>
        <span>البيانات الرئيسية</span>
      </div>

      <div>
        <h1 className="text-2xl font-bold flex items-center gap-2">
          <Database className="h-6 w-6" /> البيانات الرئيسية
        </h1>
        <p className="text-sm text-muted-foreground mt-1">
          القوائم المرجعية للمؤسسة — تُستخدم في كل القوائم المنسدلة عبر النظام
        </p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-[260px_1fr] gap-6">
        {/* Types list */}
        <div className="border border-border bg-card">
          <div className="px-4 py-3 border-b border-border text-xs font-bold uppercase tracking-wider text-muted-foreground">
            الفئات
          </div>
          <div className="max-h-[70vh] overflow-y-auto">
            {loadingTypes ? (
              <div className="p-6 text-center text-sm text-muted-foreground">
                <Loader2 className="h-4 w-4 animate-spin inline" /> جاري التحميل...
              </div>
            ) : (
              types.map((t) => (
                <button
                  key={t.objectType}
                  onClick={() => { setSelected(t.objectType); setSearch(""); }}
                  className={`w-full flex items-center justify-between px-4 py-2.5 text-sm text-right transition-colors border-b border-border/50 ${
                    selected === t.objectType ? "bg-primary/10 text-primary font-bold" : "hover:bg-card/60"
                  }`}
                >
                  <span>{typeLabelAr(t.objectType)}</span>
                  <span className="text-xs text-muted-foreground font-mono">{t.count}</span>
                </button>
              ))
            )}
          </div>
        </div>

        {/* Items panel */}
        <div className="space-y-4">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <h2 className="text-lg font-bold">{selected ? typeLabelAr(selected) : ""}</h2>
            <Button onClick={openCreate} disabled={!selected} className="h-9 gap-2 font-bold uppercase tracking-wider text-sm">
              <Plus className="h-4 w-4" /> إضافة عنصر
            </Button>
          </div>

          <div className="flex flex-wrap items-center gap-3">
            <div className="relative flex-1 min-w-[200px]">
              <Search className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                placeholder="بحث بالاسم أو الرمز..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                className="pr-10 bg-secondary border-border h-9 text-sm"
              />
            </div>
            <label className="flex items-center gap-2 text-xs text-muted-foreground cursor-pointer">
              <input type="checkbox" checked={includeInactive} onChange={(e) => setIncludeInactive(e.target.checked)} />
              إظهار غير النشط
            </label>
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
                {loadingItems ? (
                  <TableRow className="hover:bg-transparent"><TableCell colSpan={5} className="py-12 text-center text-sm text-muted-foreground"><Loader2 className="h-4 w-4 animate-spin inline" /> جاري التحميل...</TableCell></TableRow>
                ) : filtered.length === 0 ? (
                  <TableRow className="hover:bg-transparent"><TableCell colSpan={5} className="py-12 text-center text-sm text-muted-foreground">لا توجد عناصر</TableCell></TableRow>
                ) : (
                  filtered.map((item) => (
                    <TableRow key={item.id} className="border-border hover:bg-card/50">
                      <TableCell className="font-mono text-xs text-muted-foreground">{item.code}</TableCell>
                      <TableCell className="font-medium">{item.nameAr}</TableCell>
                      <TableCell className="text-sm text-muted-foreground">{item.nameEn}</TableCell>
                      <TableCell>
                        {item.isActive ? (
                          <Badge variant="outline" className="text-xs bg-green-500/10 text-green-500 border-green-500/20">نشط</Badge>
                        ) : (
                          <Badge variant="outline" className="text-xs bg-zinc-500/10 text-zinc-400 border-zinc-500/20">غير نشط</Badge>
                        )}
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center gap-1 justify-end">
                          <button onClick={() => openEdit(item)} className="h-8 w-8 inline-flex items-center justify-center text-muted-foreground hover:text-foreground transition-colors" title="تعديل">
                            <Pencil className="h-4 w-4" />
                          </button>
                          {item.isSystemDefault ? (
                            <span className="h-8 w-8 inline-flex items-center justify-center text-muted-foreground/40" title="عنصر افتراضي للنظام">
                              <Lock className="h-4 w-4" />
                            </span>
                          ) : (
                            <button onClick={() => setDeleteTarget(item)} className="h-8 w-8 inline-flex items-center justify-center text-destructive hover:text-destructive/80 transition-colors" title="حذف">
                              <Trash2 className="h-4 w-4" />
                            </button>
                          )}
                        </div>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </div>
        </div>
      </div>

      {/* Create / Edit dialog */}
      <Dialog open={dialogOpen} onOpenChange={(o) => { if (!o && !saving) setDialogOpen(false); }}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editing ? "تعديل عنصر" : "إضافة عنصر"}</DialogTitle>
            <DialogDescription>{selected ? typeLabelAr(selected) : ""}</DialogDescription>
          </DialogHeader>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 py-2">
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الرمز (Code)</Label>
              <Input
                value={form.code}
                onChange={(e) => setForm({ ...form, code: e.target.value })}
                disabled={!!editing}
                placeholder="ANNUAL"
                className="bg-secondary border-border font-mono disabled:opacity-60"
              />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">ترتيب العرض</Label>
              <Input type="number" value={form.sortOrder} onChange={(e) => setForm({ ...form, sortOrder: e.target.value })} className="bg-secondary border-border" />
            </div>
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
            {editing && (
              <label className="flex items-center gap-2 text-sm cursor-pointer sm:col-span-2">
                <input type="checkbox" checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} />
                نشط
              </label>
            )}
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)} disabled={saving}>إلغاء</Button>
            <Button onClick={saveItem} disabled={saving} className="font-bold">
              {saving ? "جاري الحفظ..." : "حفظ"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete confirm */}
      <Dialog open={!!deleteTarget} onOpenChange={(o) => { if (!o && !deleting) setDeleteTarget(null); }}>
        <DialogContent showCloseButton={false}>
          <DialogHeader>
            <DialogTitle>حذف عنصر</DialogTitle>
            <DialogDescription>
              هل أنت متأكد من حذف <span className="font-bold text-foreground">{deleteTarget?.nameAr}</span>؟ لا يمكن التراجع. (إن كان مستخدماً سيُمنع الحذف.)
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteTarget(null)} disabled={deleting}>إلغاء</Button>
            <Button onClick={confirmDelete} disabled={deleting} className="bg-destructive text-white hover:bg-destructive/90">
              {deleting ? "جاري الحذف..." : "حذف"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
