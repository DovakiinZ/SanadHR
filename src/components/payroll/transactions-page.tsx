"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { ArrowRight, Plus, Pencil, Trash2, Loader2, Send, Check, X, Ban, Paperclip } from "lucide-react";
import { toast } from "sonner";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Combobox } from "@/components/ui/combobox";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { ApiError } from "@/lib/api-client";
import { AccessGuard } from "@/components/access/access-guard";
import { usePermissions } from "@/lib/permissions";
import { TransactionStatusBadge } from "@/components/payroll/transaction-status-badge";
import { getEmployees } from "@/lib/api/employees";
import { getMasterDataItems, type MasterDataItem } from "@/lib/api/master-data";
import { uploadFile } from "@/lib/api/files";
import { type Employee } from "@/types";
import {
  listTransactions, createTransaction, updateTransaction, submitTransaction, approveTransaction,
  rejectTransaction, cancelTransaction, setTransactionAttachment, deleteTransaction,
  type PayrollTransaction, type TransactionKind,
} from "@/lib/api/payroll-transactions";

const MONTHS_AR = ["", "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو", "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"];

function notifyError(err: unknown, fallback: string) {
  if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
    toast.error(err instanceof ApiError ? err.message : fallback);
  }
}

function todayIso() { return new Date().toISOString().slice(0, 10); }

interface Copy { title: string; subtitle: string; one: string; back: string; objectType: string; }

const COPY: Record<TransactionKind, Copy> = {
  1: { title: "الإضافات", subtitle: "إضافات الرواتب — مكافآت، عمولات، بدلات لمرة واحدة", one: "إضافة", back: "الرواتب", objectType: "AdditionType" },
  2: { title: "الاستقطاعات", subtitle: "استقطاعات الرواتب — جزاءات، خصومات، تسويات", one: "استقطاع", back: "الرواتب", objectType: "DeductionType" },
};

interface Form {
  employeeId: string | null;
  typeId: string | null;
  amount: string;
  effectiveDate: string;
  transactionDate: string;
  notes: string;
  isRecurring: boolean;
}

const emptyForm: Form = {
  employeeId: null, typeId: null, amount: "", effectiveDate: todayIso(), transactionDate: "", notes: "", isRecurring: false,
};

export function TransactionsPage({ kind }: { kind: TransactionKind }) {
  return <AccessGuard anyOf={["Payroll.View"]}><Inner kind={kind} /></AccessGuard>;
}

function Inner({ kind }: { kind: TransactionKind }) {
  const copy = COPY[kind];
  const { has } = usePermissions();
  const canCreate = has("Payroll.Create");
  const canEdit = has("Payroll.Edit");
  const canApprove = has("Payroll.Approve");
  const canDelete = has("Payroll.Delete");

  const [rows, setRows] = useState<PayrollTransaction[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [types, setTypes] = useState<MasterDataItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<PayrollTransaction | null>(null);
  const [form, setForm] = useState<Form>(emptyForm);
  const [saving, setSaving] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<PayrollTransaction | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    try { setRows(await listTransactions({ kind })); }
    catch (err) { notifyError(err, "تعذر تحميل البيانات"); }
    finally { setLoading(false); }
  }, [kind]);

  useEffect(() => { load(); }, [load]);
  useEffect(() => {
    (async () => {
      try {
        const [emps, tps] = await Promise.all([getEmployees(), getMasterDataItems(copy.objectType)]);
        setEmployees(emps); setTypes(tps);
      } catch (err) { notifyError(err, "تعذر تحميل القوائم"); }
    })();
  }, [copy.objectType]);

  const empOptions = useMemo(() => employees.map((e) => ({ value: e.id, label: e.name })), [employees]);
  const typeOptions = useMemo(() => types.map((t) => ({ value: t.id, label: t.nameAr || t.nameEn })), [types]);

  function openCreate() { setEditing(null); setForm(emptyForm); setDialogOpen(true); }
  function openEdit(t: PayrollTransaction) {
    setEditing(t);
    setForm({
      employeeId: t.employeeId, typeId: t.typeId, amount: String(t.amount),
      effectiveDate: t.effectiveDate.slice(0, 10),
      transactionDate: t.transactionDate ? t.transactionDate.slice(0, 10) : "",
      notes: t.notes ?? "", isRecurring: t.isRecurring,
    });
    setDialogOpen(true);
  }

  async function save(submit: boolean) {
    if (!form.employeeId) { toast.error("اختر موظفاً"); return; }
    if (!form.typeId) { toast.error("اختر النوع"); return; }
    const amount = Number(form.amount);
    if (Number.isNaN(amount) || amount < 0) { toast.error("المبلغ غير صالح"); return; }
    if (!form.effectiveDate) { toast.error("تاريخ السريان مطلوب"); return; }
    setSaving(true);
    try {
      if (editing) {
        await updateTransaction(editing.id, {
          typeId: form.typeId, amount, effectiveDate: form.effectiveDate,
          transactionDate: form.transactionDate || null, isRecurring: form.isRecurring,
          recurrenceEndDate: null, notes: form.notes.trim() || null, attachmentFileId: editing.attachmentFileId,
        });
        if (submit) await submitTransaction(editing.id);
        toast.success("تم الحفظ");
      } else {
        await createTransaction({
          kind, employeeId: form.employeeId, typeId: form.typeId, amount,
          effectiveDate: form.effectiveDate, transactionDate: form.transactionDate || null,
          isRecurring: form.isRecurring, recurrenceEndDate: null, notes: form.notes.trim() || null,
          attachmentFileId: null, submitImmediately: submit,
        });
        toast.success("تمت الإضافة");
      }
      setDialogOpen(false); await load();
    } catch (err) { notifyError(err, "تعذر الحفظ"); } finally { setSaving(false); }
  }

  async function act(id: string, fn: () => Promise<unknown>, ok: string) {
    setBusyId(id);
    try { await fn(); toast.success(ok); await load(); }
    catch (err) { notifyError(err, "تعذر تنفيذ الإجراء"); } finally { setBusyId(null); }
  }

  async function doReject(t: PayrollTransaction) {
    const reason = window.prompt("سبب الرفض؟");
    if (reason == null) return;
    await act(t.id, () => rejectTransaction(t.id, reason), "تم الرفض");
  }

  async function attach(t: PayrollTransaction, file: File) {
    setBusyId(t.id);
    try {
      const up = await uploadFile(file, "payroll");
      await setTransactionAttachment(t.id, up.id);
      toast.success("تم إرفاق الملف"); await load();
    } catch (err) { notifyError(err, "تعذر الإرفاق"); } finally { setBusyId(null); }
  }

  async function confirmDelete() {
    if (!deleteTarget) return;
    await act(deleteTarget.id, () => deleteTransaction(deleteTarget.id), "تم الحذف");
    setDeleteTarget(null);
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2 text-sm">
        <Link href="/payroll" className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1">
          <ArrowRight className="h-4 w-4" /> {copy.back}
        </Link>
        <span className="text-muted-foreground">/</span>
        <span>{copy.title}</span>
      </div>

      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{copy.title}</h1>
          <p className="text-sm text-muted-foreground mt-1">{copy.subtitle}</p>
        </div>
        {canCreate && (
          <Button onClick={openCreate} className="h-10 gap-2 font-bold text-sm"><Plus className="h-4 w-4" /> {copy.one}</Button>
        )}
      </div>

      <div className="border border-border">
        <Table>
          <TableHeader>
            <TableRow className="border-border hover:bg-transparent">
              {["الموظف", "النوع", "المبلغ", "تاريخ السريان", "سيؤثر على", "الحالة", ""].map((h, i) => (
                <TableHead key={i} className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">{h}</TableHead>
              ))}
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              <TableRow className="hover:bg-transparent"><TableCell colSpan={7} className="py-12 text-center text-sm text-muted-foreground"><Loader2 className="h-4 w-4 animate-spin inline" /> جاري التحميل...</TableCell></TableRow>
            ) : rows.length === 0 ? (
              <TableRow className="hover:bg-transparent"><TableCell colSpan={7} className="py-12 text-center text-sm text-muted-foreground">لا توجد سجلات</TableCell></TableRow>
            ) : rows.map((t) => (
              <TableRow key={t.id} className="border-border hover:bg-card/50">
                <TableCell><div className="font-medium">{t.employeeName}</div><div className="font-mono text-[10px] text-muted-foreground">{t.employeeNumber}</div></TableCell>
                <TableCell className="text-sm">{t.typeName}</TableCell>
                <TableCell className="text-sm tabular-nums">{t.amount.toLocaleString()}</TableCell>
                <TableCell className="text-sm text-muted-foreground" dir="ltr">{t.effectiveDate.slice(0, 10)}</TableCell>
                <TableCell className="text-sm text-muted-foreground">
                  {t.targetPeriodYear ? `${MONTHS_AR[t.targetPeriodMonth ?? 0]} ${t.targetPeriodYear}` : "—"}
                </TableCell>
                <TableCell><TransactionStatusBadge status={t.status} /></TableCell>
                <TableCell>
                  <div className="flex items-center gap-1 justify-end">
                    {busyId === t.id && <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />}
                    {canEdit && t.status === 0 && (
                      <>
                        <button onClick={() => openEdit(t)} className="h-8 w-8 inline-flex items-center justify-center text-muted-foreground hover:text-foreground" title="تعديل"><Pencil className="h-4 w-4" /></button>
                        <button onClick={() => act(t.id, () => submitTransaction(t.id), "تم الإرسال للاعتماد")} className="h-8 w-8 inline-flex items-center justify-center text-amber-500 hover:text-amber-400" title="إرسال للاعتماد"><Send className="h-4 w-4" /></button>
                      </>
                    )}
                    {canApprove && t.status === 1 && (
                      <>
                        <button onClick={() => act(t.id, () => approveTransaction(t.id), "تم الاعتماد")} className="h-8 w-8 inline-flex items-center justify-center text-green-500 hover:text-green-400" title="اعتماد"><Check className="h-4 w-4" /></button>
                        <button onClick={() => doReject(t)} className="h-8 w-8 inline-flex items-center justify-center text-red-500 hover:text-red-400" title="رفض"><X className="h-4 w-4" /></button>
                      </>
                    )}
                    {canEdit && (t.status === 0 || t.status === 2) && (
                      <button onClick={() => act(t.id, () => cancelTransaction(t.id), "تم الإلغاء")} className="h-8 w-8 inline-flex items-center justify-center text-muted-foreground hover:text-foreground" title="إلغاء"><Ban className="h-4 w-4" /></button>
                    )}
                    {canEdit && t.status !== 6 && t.status !== 7 && (
                      <label className="h-8 w-8 inline-flex items-center justify-center text-muted-foreground hover:text-foreground cursor-pointer" title="إرفاق ملف">
                        <Paperclip className="h-4 w-4" />
                        <input type="file" className="hidden" onChange={(e) => { const f = e.target.files?.[0]; if (f) attach(t, f); e.currentTarget.value = ""; }} />
                      </label>
                    )}
                    {canDelete && t.status === 0 && (
                      <button onClick={() => setDeleteTarget(t)} className="h-8 w-8 inline-flex items-center justify-center text-destructive hover:text-destructive/80" title="حذف"><Trash2 className="h-4 w-4" /></button>
                    )}
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <Dialog open={dialogOpen} onOpenChange={(o) => { if (!o && !saving) setDialogOpen(false); }}>
        <DialogContent className="sm:max-w-lg">
          <DialogHeader><DialogTitle>{editing ? `تعديل ${copy.one}` : `${copy.one} جديد`}</DialogTitle></DialogHeader>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 py-2">
            <div className="space-y-2 sm:col-span-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الموظف</Label>
              <Combobox value={form.employeeId} onChange={(v) => setForm({ ...form, employeeId: v })} options={empOptions} placeholder="اختر موظفاً…" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">النوع</Label>
              <Combobox value={form.typeId} onChange={(v) => setForm({ ...form, typeId: v })} options={typeOptions} placeholder="اختر النوع…" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">المبلغ</Label>
              <Input type="number" step="any" min={0} value={form.amount} onChange={(e) => setForm({ ...form, amount: e.target.value })} className="bg-secondary border-border" dir="ltr" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">تاريخ السريان</Label>
              <Input type="date" value={form.effectiveDate} onChange={(e) => setForm({ ...form, effectiveDate: e.target.value })} className="bg-secondary border-border" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">تاريخ المعاملة (اختياري)</Label>
              <Input type="date" value={form.transactionDate} onChange={(e) => setForm({ ...form, transactionDate: e.target.value })} className="bg-secondary border-border" />
            </div>
            <div className="space-y-2 sm:col-span-2">
              <Label className="text-xs font-bold uppercase tracking-wider">ملاحظات</Label>
              <Input value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} className="bg-secondary border-border" />
            </div>
            <label className="flex items-center gap-2 text-sm cursor-pointer sm:col-span-2 border border-border px-3 py-2">
              <input type="checkbox" checked={form.isRecurring} onChange={(e) => setForm({ ...form, isRecurring: e.target.checked })} /> متكرر شهرياً (يُفعّل في مرحلة لاحقة)
            </label>
          </div>
          <DialogFooter className="gap-2">
            <Button variant="outline" onClick={() => setDialogOpen(false)} disabled={saving}>إلغاء</Button>
            <Button variant="outline" onClick={() => save(false)} disabled={saving} className="font-bold">{saving ? "..." : "حفظ كمسودة"}</Button>
            <Button onClick={() => save(true)} disabled={saving} className="font-bold">{saving ? "جاري الحفظ..." : "حفظ وإرسال للاعتماد"}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={!!deleteTarget} onOpenChange={(o) => { if (!o) setDeleteTarget(null); }}>
        <DialogContent showCloseButton={false}>
          <DialogHeader>
            <DialogTitle>حذف سجل</DialogTitle>
            <DialogDescription>هل أنت متأكد من حذف سجل <span className="font-bold text-foreground">{deleteTarget?.employeeName}</span>؟</DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteTarget(null)}>إلغاء</Button>
            <Button onClick={confirmDelete} className="bg-destructive text-white hover:bg-destructive/90">حذف</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
