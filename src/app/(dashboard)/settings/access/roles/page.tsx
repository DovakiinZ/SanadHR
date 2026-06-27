"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { ArrowRight, Plus, Pencil, Trash2, Loader2, Lock } from "lucide-react";
import { toast } from "sonner";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog";
import { ApiError } from "@/lib/api-client";
import { AccessGuard } from "@/components/access/access-guard";
import { usePermissions } from "@/lib/permissions";
import { PermissionMatrix } from "@/components/access/permission-matrix";
import {
  listRoles, createRole, updateRole, deleteRole, getPermissionCatalog,
  type RoleDto, type PermissionCatalogModule,
} from "@/lib/api/access";

function notifyError(err: unknown, fallback: string) {
  if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
    toast.error(err instanceof ApiError ? err.message : fallback);
  }
}

export default function RolesPage() {
  return <AccessGuard anyOf={["Settings.ManageRoles", "Identity.ViewRoles"]}><RolesInner /></AccessGuard>;
}

function RolesInner() {
  const { has } = usePermissions();
  const canManage = has("Settings.ManageRoles");

  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [catalog, setCatalog] = useState<PermissionCatalogModule[]>([]);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState<RoleDto | "new" | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<RoleDto | null>(null);
  const [deleting, setDeleting] = useState(false);

  const load = useCallback(async () => {
    try { setRoles(await listRoles()); } catch (err) { notifyError(err, "تعذر تحميل الأدوار"); }
  }, []);

  useEffect(() => {
    (async () => {
      setLoading(true);
      try { const [r, c] = await Promise.all([listRoles(), getPermissionCatalog()]); setRoles(r); setCatalog(c); }
      catch (err) { notifyError(err, "تعذر تحميل البيانات"); } finally { setLoading(false); }
    })();
  }, []);

  async function confirmDelete() {
    if (!deleteTarget) return;
    setDeleting(true);
    try { await deleteRole(deleteTarget.id); toast.success("تم حذف الدور"); setDeleteTarget(null); await load(); }
    catch (err) { notifyError(err, "تعذر حذف الدور"); } finally { setDeleting(false); }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2 text-sm">
        <Link href="/settings/access" className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1">
          <ArrowRight className="h-4 w-4" /> المستخدمون والصلاحيات
        </Link>
        <span className="text-muted-foreground">/</span><span>الأدوار</span>
      </div>

      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">الأدوار</h1>
          <p className="text-sm text-muted-foreground mt-1">مجموعات صلاحيات تُسند للمستخدمين</p>
        </div>
        {canManage && <Button onClick={() => setEditing("new")} className="h-10 gap-2 font-bold text-sm"><Plus className="h-4 w-4" /> دور جديد</Button>}
      </div>

      <div className="border border-border">
        <Table>
          <TableHeader>
            <TableRow className="border-border hover:bg-transparent">
              {["الدور", "المستخدمون", "الصلاحيات", ""].map((h, i) => (
                <TableHead key={i} className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">{h}</TableHead>
              ))}
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              <TableRow className="hover:bg-transparent"><TableCell colSpan={4} className="py-12 text-center text-sm text-muted-foreground"><Loader2 className="h-4 w-4 animate-spin inline" /> جاري التحميل…</TableCell></TableRow>
            ) : roles.length === 0 ? (
              <TableRow className="hover:bg-transparent"><TableCell colSpan={4} className="py-12 text-center text-sm text-muted-foreground">لا توجد أدوار</TableCell></TableRow>
            ) : roles.map((r) => (
              <TableRow key={r.id} className="border-border hover:bg-card/50">
                <TableCell className="font-medium">
                  <div className="flex items-center gap-2">{r.nameAr || r.name}
                    {r.isSystemRole && <Badge variant="outline" className="text-[10px] gap-1"><Lock className="h-3 w-3" /> نظام</Badge>}</div>
                  {r.description && <div className="text-xs text-muted-foreground">{r.description}</div>}
                </TableCell>
                <TableCell className="text-sm text-muted-foreground">{r.userCount}</TableCell>
                <TableCell className="text-sm text-muted-foreground">{r.permissionCodes.length}</TableCell>
                <TableCell>
                  <div className="flex items-center justify-end gap-1">
                    <button onClick={() => setEditing(r)} className="h-8 w-8 inline-flex items-center justify-center text-muted-foreground hover:text-foreground" title={canManage ? "تعديل" : "عرض"}><Pencil className="h-4 w-4" /></button>
                    {canManage && !r.isSystemRole && <button onClick={() => setDeleteTarget(r)} className="h-8 w-8 inline-flex items-center justify-center text-destructive hover:text-destructive/80"><Trash2 className="h-4 w-4" /></button>}
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      {editing && (
        <RoleEditor role={editing === "new" ? null : editing} catalog={catalog} canManage={canManage}
          onClose={() => setEditing(null)} onSaved={async () => { setEditing(null); await load(); }} />
      )}

      <Dialog open={!!deleteTarget} onOpenChange={(o) => { if (!o && !deleting) setDeleteTarget(null); }}>
        <DialogContent showCloseButton={false}>
          <DialogHeader>
            <DialogTitle>حذف دور</DialogTitle>
            <DialogDescription>هل أنت متأكد من حذف <span className="font-bold text-foreground">{deleteTarget?.nameAr || deleteTarget?.name}</span>؟</DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteTarget(null)} disabled={deleting}>إلغاء</Button>
            <Button onClick={confirmDelete} disabled={deleting} className="bg-destructive text-white hover:bg-destructive/90">{deleting ? "جاري الحذف…" : "حذف"}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

function RoleEditor({ role, catalog, canManage, onClose, onSaved }: {
  role: RoleDto | null; catalog: PermissionCatalogModule[]; canManage: boolean; onClose: () => void; onSaved: () => void;
}) {
  const [name, setName] = useState(role?.name ?? "");
  const [nameAr, setNameAr] = useState(role?.nameAr ?? "");
  const [description, setDescription] = useState(role?.description ?? "");
  const [selected, setSelected] = useState<Set<string>>(new Set(role?.permissionCodes ?? []));
  const [saving, setSaving] = useState(false);

  async function save() {
    if (!name.trim() && !nameAr.trim()) { toast.error("اسم الدور مطلوب"); return; }
    setSaving(true);
    try {
      const body = { name: name.trim() || nameAr.trim(), nameAr: nameAr.trim() || undefined, description: description.trim() || undefined, permissionCodes: [...selected] };
      if (role) await updateRole(role.id, body); else await createRole(body);
      toast.success(role ? "تم تحديث الدور" : "تم إنشاء الدور");
      onSaved();
    } catch (err) { notifyError(err, "تعذر حفظ الدور"); } finally { setSaving(false); }
  }

  return (
    <Dialog open onOpenChange={(o) => { if (!o && !saving) onClose(); }}>
      <DialogContent className="sm:max-w-4xl">
        <DialogHeader><DialogTitle>{role ? `تعديل دور — ${role.nameAr || role.name}` : "دور جديد"}</DialogTitle></DialogHeader>
        <div className="space-y-4 py-2 max-h-[72vh] overflow-y-auto">
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div className="space-y-2"><Label className="text-xs font-bold uppercase tracking-wider">الاسم (عربي)</Label>
              <Input value={nameAr} onChange={(e) => setNameAr(e.target.value)} disabled={!canManage} className="bg-secondary border-border" /></div>
            <div className="space-y-2"><Label className="text-xs font-bold uppercase tracking-wider">الاسم (إنجليزي)</Label>
              <Input value={name} onChange={(e) => setName(e.target.value)} disabled={!canManage} dir="ltr" className="bg-secondary border-border" /></div>
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
