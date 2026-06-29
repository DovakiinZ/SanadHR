"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { ArrowRight, Plus, Loader2, Layers } from "lucide-react";
import { toast } from "sonner";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import {
  Table, TableBody, TableCell, TableHead, TableHeader, TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import {
  Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle,
} from "@/components/ui/dialog";
import { ApiError } from "@/lib/api-client";
import { AccessGuard } from "@/components/access/access-guard";
import { usePermissions } from "@/lib/permissions";
import { getMasterDataItems, type MasterDataItem } from "@/lib/api/master-data";
import { payrollTypesApi, type PayrollTypeListItem } from "@/lib/api/payroll-types";

function notifyError(err: unknown, fallback: string) {
  if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
    toast.error(err instanceof ApiError ? err.message : fallback);
  }
}

const STATUS_AR: Record<string, string> = {
  Active: "نشط",
  Inactive: "غير نشط",
  Draft: "مسودة",
  Archived: "مؤرشف",
};

function statusBadge(status: string) {
  const label = STATUS_AR[status] ?? status;
  if (status === "Active") {
    return (
      <Badge variant="outline" className="text-xs bg-green-500/10 text-green-500 border-green-500/20">
        {label}
      </Badge>
    );
  }
  if (status === "Draft") {
    return (
      <Badge variant="outline" className="text-xs bg-yellow-500/10 text-yellow-500 border-yellow-500/20">
        {label}
      </Badge>
    );
  }
  return (
    <Badge variant="outline" className="text-xs bg-zinc-500/10 text-zinc-400 border-zinc-500/20">
      {label}
    </Badge>
  );
}

interface CreateForm {
  code: string;
  name: string;
  nameAr: string;
  categoryId: string;
}

const emptyForm: CreateForm = { code: "", name: "", nameAr: "", categoryId: "" };

export default function PayrollTypesPage() {
  return (
    <AccessGuard anyOf={["Payroll.View"]}>
      <Inner />
    </AccessGuard>
  );
}

function Inner() {
  const router = useRouter();
  const { has } = usePermissions();
  const canConfigure = has("Payroll.Configure");

  const [types, setTypes] = useState<PayrollTypeListItem[]>([]);
  const [categories, setCategories] = useState<MasterDataItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [form, setForm] = useState<CreateForm>(emptyForm);
  const [saving, setSaving] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const [t, cats] = await Promise.all([
        payrollTypesApi.list(),
        getMasterDataItems("PayrollTypeCategory"),
      ]);
      setTypes(t ?? []);
      setCategories(cats ?? []);
    } catch (err) {
      notifyError(err, "تعذر تحميل أنواع المسير");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  function openCreate() {
    setForm(emptyForm);
    setDialogOpen(true);
  }

  async function save() {
    if (!form.code.trim()) { toast.error("الرمز مطلوب"); return; }
    if (!form.name.trim()) { toast.error("الاسم الإنجليزي مطلوب"); return; }
    setSaving(true);
    try {
      const id = await payrollTypesApi.create({
        code: form.code.trim().toUpperCase(),
        name: form.name.trim(),
        nameAr: form.nameAr.trim() || undefined,
        categoryId: form.categoryId || undefined,
      });
      toast.success("تمت إضافة نوع المسير");
      setDialogOpen(false);
      router.push(`/settings/payroll/types/${id}`);
    } catch (err) {
      notifyError(err, "تعذر حفظ نوع المسير");
    } finally {
      setSaving(false);
    }
  }

  const selectClass =
    "w-full h-9 bg-secondary border border-border px-3 text-sm text-foreground rounded-none";

  return (
    <div className="space-y-6">
      {/* Breadcrumb */}
      <div className="flex items-center gap-2 text-sm">
        <Link
          href="/settings/payroll"
          className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1"
        >
          <ArrowRight className="h-4 w-4" /> إعدادات الرواتب
        </Link>
        <span className="text-muted-foreground">/</span>
        <span>أنواع المسير</span>
      </div>

      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">أنواع المسير</h1>
          <p className="text-sm text-muted-foreground mt-1">
            عرّف أنواع المسيرات (شهري، مكافآت، نهاية خدمة…) وإصداراتها القابلة للتهيئة
          </p>
        </div>
        {canConfigure && (
          <Button onClick={openCreate} className="h-10 gap-2 font-bold uppercase tracking-wider text-sm">
            <Plus className="h-4 w-4" /> نوع مسير
          </Button>
        )}
      </div>

      {/* Table */}
      <div className="border border-border">
        <Table>
          <TableHeader>
            <TableRow className="border-border hover:bg-transparent">
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">
                النوع
              </TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">
                الفئة
              </TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">
                الحالة
              </TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">
                الإصدارات
              </TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              <TableRow className="hover:bg-transparent">
                <TableCell colSpan={4} className="py-12 text-center text-sm text-muted-foreground">
                  <Loader2 className="h-4 w-4 animate-spin inline" /> جاري التحميل...
                </TableCell>
              </TableRow>
            ) : types.length === 0 ? (
              <TableRow className="hover:bg-transparent">
                <TableCell colSpan={4} className="py-12 text-center text-sm text-muted-foreground">
                  لا توجد أنواع مسير — أضف نوعاً جديداً
                </TableCell>
              </TableRow>
            ) : (
              types.map((t) => {
                const cat = categories.find((c) => c.id === t.categoryId);
                return (
                  <TableRow
                    key={t.id}
                    className="border-border hover:bg-card/50 cursor-pointer"
                    onClick={() => router.push(`/settings/payroll/types/${t.id}`)}
                  >
                    <TableCell>
                      <div className="font-medium">{t.nameAr ?? t.name}</div>
                      <div className="font-mono text-[10px] text-muted-foreground">{t.code}</div>
                    </TableCell>
                    <TableCell className="text-sm text-muted-foreground">
                      {cat ? cat.nameAr : <span className="text-muted-foreground/40">—</span>}
                    </TableCell>
                    <TableCell>{statusBadge(t.status)}</TableCell>
                    <TableCell>
                      <span className="inline-flex items-center gap-1 text-sm text-muted-foreground">
                        <Layers className="h-3.5 w-3.5" />
                        {t.versionCount}
                      </span>
                    </TableCell>
                  </TableRow>
                );
              })
            )}
          </TableBody>
        </Table>
      </div>

      {/* New Type Dialog */}
      <Dialog open={dialogOpen} onOpenChange={(o) => { if (!o && !saving) setDialogOpen(false); }}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>نوع مسير جديد</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-2">
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الرمز</Label>
              <Input
                value={form.code}
                onChange={(e) => setForm({ ...form, code: e.target.value })}
                className="bg-secondary border-border font-mono"
                placeholder="MONTHLY"
              />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الاسم (إنجليزي)</Label>
              <Input
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
                className="bg-secondary border-border"
                placeholder="Monthly Payroll"
              />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الاسم (عربي)</Label>
              <Input
                value={form.nameAr}
                onChange={(e) => setForm({ ...form, nameAr: e.target.value })}
                className="bg-secondary border-border"
                placeholder="مسير الرواتب الشهري"
              />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الفئة</Label>
              <select
                value={form.categoryId}
                onChange={(e) => setForm({ ...form, categoryId: e.target.value })}
                className={selectClass}
              >
                <option value="">— بدون فئة —</option>
                {categories.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.nameAr}
                  </option>
                ))}
              </select>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)} disabled={saving}>
              إلغاء
            </Button>
            <Button onClick={save} disabled={saving} className="font-bold">
              {saving ? "جاري الحفظ..." : "إنشاء"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
