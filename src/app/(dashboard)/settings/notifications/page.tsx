"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { ArrowRight, Plus, Pencil, Trash2, Loader2, Bell, Play } from "lucide-react";
import { toast } from "sonner";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { ApiError } from "@/lib/api-client";
import {
  NotificationRule, NotificationRuleInput,
  getNotificationRules, createNotificationRule, updateNotificationRule, deleteNotificationRule,
  runDocumentExpiryScan, getRoles, RoleLite,
} from "@/lib/api/notification-rules";
import { DOCUMENT_TYPES, documentTypeLabel } from "@/lib/api/employee-documents";
import { getEmployees } from "@/lib/api/employees";
import { Employee } from "@/types";

function notifyError(err: unknown, fallback: string) {
  if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
    toast.error(err instanceof ApiError ? err.message : fallback);
  }
}

const emptyForm: NotificationRuleInput = {
  name: "", event: "DocumentExpiry", daysBefore: 30, documentType: null,
  notifyEmployee: false, notifyDirectManager: true, notifyDepartmentManager: false,
  extraEmployeeId: null, roleId: null,
  channelBell: true, channelEmail: false, channelSms: false, isActive: true,
};

function recipientsSummary(r: NotificationRule, roles: RoleLite[], emps: Employee[]): string {
  const parts: string[] = [];
  if (r.notifyEmployee) parts.push("الموظف");
  if (r.notifyDirectManager) parts.push("المدير المباشر");
  if (r.notifyDepartmentManager) parts.push("مدير القسم");
  if (r.extraEmployeeId) parts.push(emps.find((e) => e.id === r.extraEmployeeId)?.name ?? "موظف محدد");
  if (r.roleId) parts.push(roles.find((x) => x.id === r.roleId)?.nameAr ?? "دور");
  return parts.length ? parts.join("، ") : "—";
}

export default function NotificationsSettingsPage() {
  const [rules, setRules] = useState<NotificationRule[]>([]);
  const [roles, setRoles] = useState<RoleLite[]>([]);
  const [emps, setEmps] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<NotificationRule | null>(null);
  const [form, setForm] = useState<NotificationRuleInput>(emptyForm);
  const [saving, setSaving] = useState(false);
  const [running, setRunning] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<NotificationRule | null>(null);
  const [deleting, setDeleting] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try { setRules(await getNotificationRules()); }
    catch (err) { notifyError(err, "تعذر تحميل القواعد"); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => {
    load();
    getRoles().then(setRoles).catch(() => setRoles([]));
    getEmployees({ pageSize: 500 }).then(setEmps).catch(() => setEmps([]));
  }, [load]);

  function openCreate() { setEditing(null); setForm(emptyForm); setDialogOpen(true); }
  function openEdit(r: NotificationRule) {
    setEditing(r);
    setForm({
      name: r.name, event: r.event, daysBefore: r.daysBefore, documentType: r.documentType ?? null,
      notifyEmployee: r.notifyEmployee, notifyDirectManager: r.notifyDirectManager, notifyDepartmentManager: r.notifyDepartmentManager,
      extraEmployeeId: r.extraEmployeeId ?? null, roleId: r.roleId ?? null,
      channelBell: r.channelBell, channelEmail: r.channelEmail, channelSms: r.channelSms, isActive: r.isActive,
    });
    setDialogOpen(true);
  }

  async function save() {
    if (!form.name.trim()) { toast.error("اسم القاعدة مطلوب"); return; }
    if (!form.notifyEmployee && !form.notifyDirectManager && !form.notifyDepartmentManager && !form.extraEmployeeId && !form.roleId) {
      toast.error("اختر مستلماً واحداً على الأقل"); return;
    }
    setSaving(true);
    try {
      const payload = { ...form, name: form.name.trim() };
      if (editing) { await updateNotificationRule(editing.id, payload); toast.success("تم تحديث القاعدة"); }
      else { await createNotificationRule(payload); toast.success("تمت إضافة القاعدة"); }
      setDialogOpen(false); await load();
    } catch (err) { notifyError(err, "تعذر حفظ القاعدة"); } finally { setSaving(false); }
  }

  async function confirmDelete() {
    if (!deleteTarget) return;
    setDeleting(true);
    try { await deleteNotificationRule(deleteTarget.id); toast.success("تم الحذف"); setDeleteTarget(null); await load(); }
    catch (err) { notifyError(err, "تعذر الحذف"); } finally { setDeleting(false); }
  }

  async function runNow() {
    setRunning(true);
    try { const n = await runDocumentExpiryScan(); toast.success(`تم إنشاء ${n} تنبيه`); }
    catch (err) { notifyError(err, "تعذر تشغيل الفحص"); } finally { setRunning(false); }
  }

  const selectClass = "w-full h-9 bg-secondary border border-border px-3 text-sm text-foreground";

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2 text-sm">
        <Link href="/settings" className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1">
          <ArrowRight className="h-4 w-4" /> الإعدادات
        </Link>
        <span className="text-muted-foreground">/</span>
        <span>إعدادات التنبيهات</span>
      </div>

      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">قواعد التنبيهات</h1>
          <p className="text-sm text-muted-foreground mt-1">تنبيه المستلمين قبل انتهاء مستندات الموظفين بالمدة المحددة</p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" onClick={runNow} disabled={running} className="h-10 gap-2 text-sm">
            {running ? <Loader2 className="h-4 w-4 animate-spin" /> : <Play className="h-4 w-4" />} تشغيل الفحص الآن
          </Button>
          <Button onClick={openCreate} className="h-10 gap-2 font-bold uppercase tracking-wider text-sm"><Plus className="h-4 w-4" /> قاعدة</Button>
        </div>
      </div>

      <div className="border border-border">
        <Table>
          <TableHeader>
            <TableRow className="border-border hover:bg-transparent">
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">القاعدة</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">قبل</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">النوع</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">المستلمون</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">القنوات</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">الحالة</TableHead>
              <TableHead className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground w-24"></TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              <TableRow className="hover:bg-transparent"><TableCell colSpan={7} className="py-12 text-center text-sm text-muted-foreground"><Loader2 className="h-4 w-4 animate-spin inline" /> جاري التحميل...</TableCell></TableRow>
            ) : rules.length === 0 ? (
              <TableRow className="hover:bg-transparent"><TableCell colSpan={7} className="py-12 text-center text-sm text-muted-foreground">لا توجد قواعد — أضف قاعدة لتفعيل تنبيهات انتهاء المستندات</TableCell></TableRow>
            ) : rules.map((r) => (
              <TableRow key={r.id} className="border-border hover:bg-card/50">
                <TableCell><div className="font-medium">{r.name}</div><div className="text-[10px] text-muted-foreground">انتهاء مستند</div></TableCell>
                <TableCell className="text-sm font-mono">{r.daysBefore} يوم</TableCell>
                <TableCell className="text-sm text-muted-foreground">{r.documentType ? documentTypeLabel(r.documentType) : "الكل"}</TableCell>
                <TableCell className="text-xs text-muted-foreground max-w-[220px]">{recipientsSummary(r, roles, emps)}</TableCell>
                <TableCell>
                  <div className="flex flex-wrap gap-1">
                    {r.channelBell && <Badge variant="outline" className="text-[10px] border-border text-muted-foreground">جرس</Badge>}
                    {r.channelEmail && <Badge variant="outline" className="text-[10px] border-border text-muted-foreground">بريد</Badge>}
                    {r.channelSms && <Badge variant="outline" className="text-[10px] border-border text-muted-foreground">SMS</Badge>}
                  </div>
                </TableCell>
                <TableCell>
                  {r.isActive
                    ? <Badge variant="outline" className="text-xs bg-green-500/10 text-green-600 border-green-500/20">نشط</Badge>
                    : <Badge variant="outline" className="text-xs bg-zinc-500/10 text-zinc-400 border-zinc-500/20">غير نشط</Badge>}
                </TableCell>
                <TableCell>
                  <div className="flex items-center gap-1 justify-end">
                    <button onClick={() => openEdit(r)} className="h-8 w-8 inline-flex items-center justify-center text-muted-foreground hover:text-foreground" title="تعديل"><Pencil className="h-4 w-4" /></button>
                    <button onClick={() => setDeleteTarget(r)} className="h-8 w-8 inline-flex items-center justify-center text-destructive hover:text-destructive/80" title="حذف"><Trash2 className="h-4 w-4" /></button>
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <Dialog open={dialogOpen} onOpenChange={(o) => { if (!o && !saving) setDialogOpen(false); }}>
        <DialogContent className="sm:max-w-2xl">
          <DialogHeader><DialogTitle>{editing ? "تعديل قاعدة" : "قاعدة جديدة"}</DialogTitle></DialogHeader>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 py-2 max-h-[68vh] overflow-y-auto pl-1">
            <div className="space-y-2 sm:col-span-2">
              <Label className="text-xs font-bold uppercase tracking-wider">اسم القاعدة</Label>
              <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} className="bg-secondary border-border" placeholder="مثال: تنبيه انتهاء الإقامة" />
            </div>

            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">التنبيه قبل (يوم)</Label>
              <div className="flex gap-2">
                {[10, 20, 30].map((d) => (
                  <button key={d} type="button" onClick={() => setForm({ ...form, daysBefore: d })}
                    className={`h-9 px-3 border text-sm ${form.daysBefore === d ? "border-primary bg-primary/10 text-primary" : "border-border text-muted-foreground"}`}>{d}</button>
                ))}
                <Input type="number" min={0} value={form.daysBefore} onChange={(e) => setForm({ ...form, daysBefore: Number(e.target.value) || 0 })} className="bg-secondary border-border w-20" />
              </div>
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">نوع المستند</Label>
              <select value={form.documentType ?? ""} onChange={(e) => setForm({ ...form, documentType: e.target.value || null })} className={selectClass}>
                <option value="">كل الأنواع</option>
                {DOCUMENT_TYPES.map((t) => <option key={t.code} value={t.code}>{t.labelAr}</option>)}
              </select>
            </div>

            <div className="sm:col-span-2 border-t border-border pt-2 text-xs font-bold uppercase tracking-wider text-primary">المستلمون</div>
            <div className="sm:col-span-2 grid grid-cols-1 sm:grid-cols-3 gap-2">
              <label className="flex items-center gap-2 text-sm cursor-pointer border border-border px-3 py-2"><input type="checkbox" checked={form.notifyEmployee} onChange={(e) => setForm({ ...form, notifyEmployee: e.target.checked })} /> الموظف</label>
              <label className="flex items-center gap-2 text-sm cursor-pointer border border-border px-3 py-2"><input type="checkbox" checked={form.notifyDirectManager} onChange={(e) => setForm({ ...form, notifyDirectManager: e.target.checked })} /> المدير المباشر</label>
              <label className="flex items-center gap-2 text-sm cursor-pointer border border-border px-3 py-2"><input type="checkbox" checked={form.notifyDepartmentManager} onChange={(e) => setForm({ ...form, notifyDepartmentManager: e.target.checked })} /> مدير القسم</label>
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">موظف محدد (اختياري)</Label>
              <select value={form.extraEmployeeId ?? ""} onChange={(e) => setForm({ ...form, extraEmployeeId: e.target.value || null })} className={selectClass}>
                <option value="">— لا أحد —</option>
                {emps.map((e) => <option key={e.id} value={e.id}>{e.name}</option>)}
              </select>
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">دور / مجموعة (اختياري)</Label>
              <select value={form.roleId ?? ""} onChange={(e) => setForm({ ...form, roleId: e.target.value || null })} className={selectClass}>
                <option value="">— لا أحد —</option>
                {roles.map((r) => <option key={r.id} value={r.id}>{r.nameAr || r.name}</option>)}
              </select>
            </div>

            <div className="sm:col-span-2 border-t border-border pt-2 text-xs font-bold uppercase tracking-wider text-primary">القنوات</div>
            <div className="sm:col-span-2 grid grid-cols-1 sm:grid-cols-3 gap-2">
              <label className="flex items-center gap-2 text-sm cursor-pointer border border-border px-3 py-2"><input type="checkbox" checked={form.channelBell} onChange={(e) => setForm({ ...form, channelBell: e.target.checked })} /> جرس (فوري)</label>
              <label className="flex items-center gap-2 text-sm cursor-pointer border border-border px-3 py-2"><input type="checkbox" checked={form.channelEmail} onChange={(e) => setForm({ ...form, channelEmail: e.target.checked })} /> بريد (قائمة انتظار)</label>
              <label className="flex items-center gap-2 text-sm cursor-pointer border border-border px-3 py-2 opacity-60"><input type="checkbox" checked={form.channelSms} onChange={(e) => setForm({ ...form, channelSms: e.target.checked })} /> SMS (لاحقاً)</label>
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
            <DialogTitle>حذف قاعدة</DialogTitle>
            <DialogDescription>هل أنت متأكد من حذف <span className="font-bold text-foreground">{deleteTarget?.name}</span>؟</DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteTarget(null)} disabled={deleting}>إلغاء</Button>
            <Button onClick={confirmDelete} disabled={deleting} className="bg-destructive text-white hover:bg-destructive/90">{deleting ? "جاري الحذف..." : "حذف"}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <div className="text-xs text-muted-foreground flex items-center gap-2">
        <Bell className="h-3.5 w-3.5" /> يعمل الفحص تلقائياً في الخلفية كل 12 ساعة، أو شغّله الآن يدوياً.
      </div>
    </div>
  );
}
