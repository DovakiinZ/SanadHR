"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { ArrowRight, Plus, Pencil, Trash2, Loader2, Copy, Lock, Sparkles } from "lucide-react";
import { toast } from "sonner";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog";
import { ApiError } from "@/lib/api-client";
import { AccessGuard } from "@/components/access/access-guard";
import { usePermissions } from "@/lib/permissions";
import { PermissionMatrix } from "@/components/access/permission-matrix";
import {
  listTemplates, createTemplate, updateTemplate, deleteTemplate, duplicateTemplate, seedDefaultTemplates, getPermissionCatalog,
  type TemplateDto, type PermissionCatalogModule,
} from "@/lib/api/access";

function notifyError(err: unknown, fallback: string) {
  if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
    toast.error(err instanceof ApiError ? err.message : fallback);
  }
}

export default function TemplatesPage() {
  return <AccessGuard anyOf={["Settings.ManageTemplates", "Identity.ViewRoles"]}><TemplatesInner /></AccessGuard>;
}

function TemplatesInner() {
  const { has } = usePermissions();
  const canManage = has("Settings.ManageTemplates");

  const [templates, setTemplates] = useState<TemplateDto[]>([]);
  const [catalog, setCatalog] = useState<PermissionCatalogModule[]>([]);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState<TemplateDto | "new" | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<TemplateDto | null>(null);
  const [busy, setBusy] = useState(false);

  const load = useCallback(async () => {
    try { setTemplates(await listTemplates()); } catch (err) { notifyError(err, "تعذر تحميل القوالب"); }
  }, []);

  useEffect(() => {
    (async () => {
      setLoading(true);
      try { const [t, c] = await Promise.all([listTemplates(), getPermissionCatalog()]); setTemplates(t); setCatalog(c); }
      catch (err) { notifyError(err, "تعذر تحميل البيانات"); } finally { setLoading(false); }
    })();
  }, []);

  async function doSeed() {
    setBusy(true);
    try { await seedDefaultTemplates(); toast.success("تم إنشاء القوالب الجاهزة"); await load(); }
    catch (err) { notifyError(err, "تعذر إنشاء القوالب"); } finally { setBusy(false); }
  }

  async function doDuplicate(t: TemplateDto) {
    setBusy(true);
    try { await duplicateTemplate(t.id); toast.success("تم نسخ القالب"); await load(); }
    catch (err) { notifyError(err, "تعذر نسخ القالب"); } finally { setBusy(false); }
  }

  async function confirmDelete() {
    if (!deleteTarget) return;
    setBusy(true);
    try { await deleteTemplate(deleteTarget.id); toast.success("تم حذف القالب"); setDeleteTarget(null); await load(); }
    catch (err) { notifyError(err, "تعذر حذف القالب"); } finally { setBusy(false); }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2 text-sm">
        <Link href="/settings/access" className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1">
          <ArrowRight className="h-4 w-4" /> المستخدمون والصلاحيات
        </Link>
        <span className="text-muted-foreground">/</span><span>قوالب الصلاحيات</span>
      </div>

      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">قوالب الصلاحيات</h1>
          <p className="text-sm text-muted-foreground mt-1">مجموعات صلاحيات جاهزة تُسند للمستخدمين — على نمط HubSpot</p>
        </div>
        {canManage && (
          <div className="flex items-center gap-2">
            <Button onClick={doSeed} disabled={busy} variant="outline" className="h-10 gap-2 text-sm"><Sparkles className="h-4 w-4" /> القوالب الجاهزة</Button>
            <Button onClick={() => setEditing("new")} className="h-10 gap-2 font-bold text-sm"><Plus className="h-4 w-4" /> قالب جديد</Button>
          </div>
        )}
      </div>

      {loading ? (
        <div className="py-16 text-center text-muted-foreground"><Loader2 className="h-5 w-5 animate-spin inline" /> جاري التحميل…</div>
      ) : templates.length === 0 ? (
        <div className="border border-dashed border-border p-12 text-center text-sm text-muted-foreground">
          لا توجد قوالب بعد. {canManage && "اضغط «القوالب الجاهزة» لإنشاء المجموعة الافتراضية."}
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {templates.map((t) => (
            <div key={t.id} className="border border-border bg-card p-5">
              <div className="flex items-start justify-between">
                <h2 className="text-base font-bold flex items-center gap-2">{t.nameAr || t.nameEn}
                  {t.isSystem && <Badge variant="outline" className="text-[10px] gap-1"><Lock className="h-3 w-3" /> جاهز</Badge>}</h2>
              </div>
              {t.description && <p className="text-xs text-muted-foreground mt-1 leading-relaxed">{t.description}</p>}
              <p className="text-xs text-muted-foreground mt-3">{t.permissionCodes.length} صلاحية</p>
              <div className="flex items-center gap-1 mt-3 pt-3 border-t border-border">
                <button onClick={() => setEditing(t)} className="h-8 px-2 inline-flex items-center gap-1 text-xs text-muted-foreground hover:text-foreground"><Pencil className="h-3.5 w-3.5" /> {canManage ? "تعديل" : "عرض"}</button>
                {canManage && <button onClick={() => doDuplicate(t)} disabled={busy} className="h-8 px-2 inline-flex items-center gap-1 text-xs text-muted-foreground hover:text-foreground"><Copy className="h-3.5 w-3.5" /> نسخ</button>}
                {canManage && !t.isSystem && <button onClick={() => setDeleteTarget(t)} className="h-8 px-2 inline-flex items-center gap-1 text-xs text-destructive hover:text-destructive/80"><Trash2 className="h-3.5 w-3.5" /> حذف</button>}
              </div>
            </div>
          ))}
        </div>
      )}

      {editing && (
        <TemplateEditor template={editing === "new" ? null : editing} catalog={catalog} canManage={canManage}
          onClose={() => setEditing(null)} onSaved={async () => { setEditing(null); await load(); }} />
      )}

      <Dialog open={!!deleteTarget} onOpenChange={(o) => { if (!o && !busy) setDeleteTarget(null); }}>
        <DialogContent showCloseButton={false}>
          <DialogHeader>
            <DialogTitle>حذف قالب</DialogTitle>
            <DialogDescription>هل أنت متأكد من حذف <span className="font-bold text-foreground">{deleteTarget?.nameAr || deleteTarget?.nameEn}</span>؟</DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteTarget(null)} disabled={busy}>إلغاء</Button>
            <Button onClick={confirmDelete} disabled={busy} className="bg-destructive text-white hover:bg-destructive/90">حذف</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

function TemplateEditor({ template, catalog, canManage, onClose, onSaved }: {
  template: TemplateDto | null; catalog: PermissionCatalogModule[]; canManage: boolean; onClose: () => void; onSaved: () => void;
}) {
  const [nameAr, setNameAr] = useState(template?.nameAr ?? "");
  const [nameEn, setNameEn] = useState(template?.nameEn ?? "");
  const [description, setDescription] = useState(template?.description ?? "");
  const [selected, setSelected] = useState<Set<string>>(new Set(template?.permissionCodes ?? []));
  const [saving, setSaving] = useState(false);

  async function save() {
    if (!nameAr.trim() && !nameEn.trim()) { toast.error("اسم القالب مطلوب"); return; }
    setSaving(true);
    try {
      const body = { nameEn: nameEn.trim() || nameAr.trim(), nameAr: nameAr.trim() || nameEn.trim(), description: description.trim() || undefined, permissionCodes: [...selected] };
      if (template) await updateTemplate(template.id, body); else await createTemplate(body);
      toast.success(template ? "تم تحديث القالب" : "تم إنشاء القالب");
      onSaved();
    } catch (err) { notifyError(err, "تعذر حفظ القالب"); } finally { setSaving(false); }
  }

  return (
    <Dialog open onOpenChange={(o) => { if (!o && !saving) onClose(); }}>
      <DialogContent className="sm:max-w-4xl">
        <DialogHeader><DialogTitle>{template ? `تعديل قالب — ${template.nameAr || template.nameEn}` : "قالب جديد"}</DialogTitle></DialogHeader>
        <div className="space-y-4 py-2 max-h-[72vh] overflow-y-auto">
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div className="space-y-2"><Label className="text-xs font-bold uppercase tracking-wider">الاسم (عربي)</Label>
              <Input value={nameAr} onChange={(e) => setNameAr(e.target.value)} disabled={!canManage} className="bg-secondary border-border" /></div>
            <div className="space-y-2"><Label className="text-xs font-bold uppercase tracking-wider">الاسم (إنجليزي)</Label>
              <Input value={nameEn} onChange={(e) => setNameEn(e.target.value)} disabled={!canManage} dir="ltr" className="bg-secondary border-border" /></div>
            <div className="space-y-2"><Label className="text-xs font-bold uppercase tracking-wider">الوصف</Label>
              <Input value={description} onChange={(e) => setDescription(e.target.value)} disabled={!canManage} className="bg-secondary border-border" /></div>
          </div>
          <div>
            <div className="flex items-center justify-between mb-2">
              <Label className="text-xs font-bold uppercase tracking-wider">مصفوفة الصلاحيات</Label>
              <span className="text-xs text-muted-foreground">{selected.size} صلاحية</span>
            </div>
            <PermissionMatrix catalog={catalog} value={selected} onChange={setSelected} disabled={!canManage} />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={onClose} disabled={saving}>إغلاق</Button>
          {canManage && <Button onClick={save} disabled={saving} className="font-bold">{saving ? "جاري الحفظ…" : "حفظ"}</Button>}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
