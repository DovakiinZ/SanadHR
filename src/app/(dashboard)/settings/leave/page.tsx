"use client";

import { type ReactNode, useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { ArrowRight, Check, Loader2, Pencil, Plus, Save, X } from "lucide-react";
import { toast } from "sonner";
import { usePermissions } from "@/lib/permissions";
import { ApiError } from "@/lib/api-client";
import {
  createMasterDataItem, getMasterDataItems, MasterDataItem, parseMetadata, updateMasterDataItem,
} from "@/lib/api/master-data";
import { getEmployees } from "@/lib/api/employees";
import { getEmployeeLeaveBalances, LeaveTypeInfo, setLeaveBalance } from "@/lib/api/request-center";
import { Employee } from "@/types";

const OBJ = "LeaveType";

interface Rules {
  paid: boolean; paidPercentage: number; maxDays: number; annualBalance: number;
  requiresAttachment: boolean; affectsPayroll: boolean; affectsAttendance: boolean; countWeekends: boolean;
}
const DEFAULT_RULES: Rules = {
  paid: true, paidPercentage: 100, maxDays: 30, annualBalance: 30,
  requiresAttachment: false, affectsPayroll: false, affectsAttendance: true, countWeekends: false,
};

export default function LeaveSettingsPage() {
  const { has } = usePermissions();
  const canEdit = has("Platform.MasterData.Edit") || has("Settings.Edit");
  const [tab, setTab] = useState<"types" | "balances">("types");

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold">إعدادات الإجازات</h1>
          <p className="mt-1 text-sm text-muted-foreground">تحكّم في أنواع الإجازات وقواعدها وأرصدة الموظفين</p>
        </div>
        <Link href="/settings" className="inline-flex h-10 items-center gap-2 border border-border px-4 text-sm hover:bg-muted">
          <ArrowRight className="h-4 w-4" /> الإعدادات
        </Link>
      </div>

      <div className="flex items-center gap-1 border-b border-border">
        <button onClick={() => setTab("types")} className={`-mb-px border-b-2 px-4 py-2 text-sm ${tab === "types" ? "border-primary font-bold" : "border-transparent text-muted-foreground"}`}>أنواع الإجازات</button>
        <button onClick={() => setTab("balances")} className={`-mb-px border-b-2 px-4 py-2 text-sm ${tab === "balances" ? "border-primary font-bold" : "border-transparent text-muted-foreground"}`}>الأرصدة</button>
      </div>

      {tab === "types" ? <LeaveTypes canEdit={canEdit} /> : <Balances canEdit={canEdit} />}
    </div>
  );
}

// ── Leave Types CRUD ──
function LeaveTypes({ canEdit }: { canEdit: boolean }) {
  const [items, setItems] = useState<MasterDataItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState<MasterDataItem | "new" | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    try { setItems(await getMasterDataItems(OBJ, { includeInactive: true })); }
    catch { toast.error("تعذر تحميل أنواع الإجازات"); }
    finally { setLoading(false); }
  }, []);
  useEffect(() => { load(); }, [load]);

  if (loading) return <Spinner />;

  return (
    <div className="space-y-3">
      {canEdit && (
        <button onClick={() => setEditing("new")} className="inline-flex h-10 items-center gap-2 bg-primary px-4 text-sm font-bold uppercase tracking-wider text-primary-foreground hover:bg-primary/80">
          <Plus className="h-4 w-4" /> نوع إجازة جديد
        </button>
      )}
      <div className="space-y-2">
        {items.map((it) => {
          const r = parseMetadata<Rules>(it, DEFAULT_RULES);
          return (
            <div key={it.id} className={`flex items-center justify-between gap-3 border border-border bg-card px-4 py-3 ${it.isActive ? "" : "opacity-50"}`}>
              <div>
                <div className="flex items-center gap-2"><span className="font-medium">{it.nameAr}</span><span className="font-mono text-xs text-muted-foreground">{it.code}</span></div>
                <div className="mt-1 flex flex-wrap gap-1.5 text-xs text-muted-foreground">
                  <Tag>{r.paid ? `مدفوعة ${r.paidPercentage}%` : "بدون راتب"}</Tag>
                  <Tag>الرصيد {r.annualBalance} يوم</Tag>
                  <Tag>الحد الأقصى {r.maxDays}</Tag>
                  {r.requiresAttachment && <Tag>مرفق مطلوب</Tag>}
                  {r.affectsPayroll && <Tag>يؤثر على الراتب</Tag>}
                </div>
              </div>
              {canEdit && <button onClick={() => setEditing(it)} className="text-muted-foreground hover:text-foreground"><Pencil className="h-4 w-4" /></button>}
            </div>
          );
        })}
        {items.length === 0 && <p className="text-sm text-muted-foreground">لا توجد أنواع إجازات</p>}
      </div>

      {editing && <LeaveTypeDialog item={editing === "new" ? null : editing} onClose={() => setEditing(null)} onSaved={() => { setEditing(null); load(); }} />}
    </div>
  );
}

function LeaveTypeDialog({ item, onClose, onSaved }: { item: MasterDataItem | null; onClose: () => void; onSaved: () => void }) {
  const [code, setCode] = useState(item?.code ?? "");
  const [nameAr, setNameAr] = useState(item?.nameAr ?? "");
  const [nameEn, setNameEn] = useState(item?.nameEn ?? "");
  const [isActive, setIsActive] = useState(item?.isActive ?? true);
  const [rules, setRules] = useState<Rules>(item ? parseMetadata<Rules>(item, DEFAULT_RULES) : DEFAULT_RULES);
  const [saving, setSaving] = useState(false);
  const setR = (patch: Partial<Rules>) => setRules((p) => ({ ...p, ...patch }));

  const save = async () => {
    if (!nameAr.trim() || !nameEn.trim() || (!item && !code.trim())) { toast.error("أكمل الحقول المطلوبة"); return; }
    setSaving(true);
    try {
      const payload = { nameAr: nameAr.trim(), nameEn: nameEn.trim(), isActive, metadata: rules as unknown as Record<string, unknown> };
      if (item) await updateMasterDataItem(item.id, payload);
      else await createMasterDataItem(OBJ, { code: code.trim().toUpperCase(), ...payload });
      toast.success("تم الحفظ");
      onSaved();
    } catch (e) { toast.error(e instanceof ApiError ? e.message : "تعذر الحفظ"); }
    finally { setSaving(false); }
  };

  const num = "h-9 w-full border border-border bg-secondary px-3 text-sm";
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/60" onClick={onClose} />
      <div className="relative z-10 max-h-[85vh] w-full max-w-lg overflow-auto border border-border bg-card">
        <div className="flex items-center justify-between border-b border-border px-5 py-4">
          <h3 className="font-bold">{item ? "تعديل نوع إجازة" : "نوع إجازة جديد"}</h3>
          <button onClick={onClose} className="text-muted-foreground hover:text-foreground"><X className="h-5 w-5" /></button>
        </div>
        <div className="space-y-3 p-5">
          <div className="grid grid-cols-2 gap-3">
            <Field label="الرمز"><input value={code} disabled={!!item} onChange={(e) => setCode(e.target.value)} className={`${num} ${item ? "opacity-60" : ""}`} dir="ltr" /></Field>
            <Field label="نشط"><label className="flex h-9 items-center gap-2 text-sm"><input type="checkbox" checked={isActive} onChange={(e) => setIsActive(e.target.checked)} /> مفعّل</label></Field>
            <Field label="الاسم (عربي)"><input value={nameAr} onChange={(e) => setNameAr(e.target.value)} className={num} /></Field>
            <Field label="الاسم (إنجليزي)"><input value={nameEn} onChange={(e) => setNameEn(e.target.value)} className={num} dir="ltr" /></Field>
          </div>

          <div className="border-t border-border pt-3">
            <p className="mb-2 text-xs font-bold uppercase tracking-wider text-muted-foreground">القواعد</p>
            <div className="grid grid-cols-2 gap-3">
              <Check2 label="مدفوعة" checked={rules.paid} onChange={(v) => setR({ paid: v })} />
              <Field label="نسبة الدفع %"><input type="number" value={rules.paidPercentage} onChange={(e) => setR({ paidPercentage: +e.target.value })} className={num} /></Field>
              <Field label="الرصيد السنوي (يوم)"><input type="number" value={rules.annualBalance} onChange={(e) => setR({ annualBalance: +e.target.value })} className={num} /></Field>
              <Field label="الحد الأقصى (يوم)"><input type="number" value={rules.maxDays} onChange={(e) => setR({ maxDays: +e.target.value })} className={num} /></Field>
              <Check2 label="يتطلب مرفقاً" checked={rules.requiresAttachment} onChange={(v) => setR({ requiresAttachment: v })} />
              <Check2 label="يؤثر على الراتب" checked={rules.affectsPayroll} onChange={(v) => setR({ affectsPayroll: v })} />
              <Check2 label="يؤثر على الحضور" checked={rules.affectsAttendance} onChange={(v) => setR({ affectsAttendance: v })} />
              <Check2 label="احتساب عطلة نهاية الأسبوع" checked={rules.countWeekends} onChange={(v) => setR({ countWeekends: v })} />
            </div>
          </div>
        </div>
        <div className="flex justify-end gap-2 border-t border-border px-5 py-4">
          <button onClick={onClose} className="h-10 px-4 text-sm text-muted-foreground hover:text-foreground">إلغاء</button>
          <button onClick={save} disabled={saving} className="inline-flex h-10 items-center gap-2 bg-primary px-5 text-sm font-bold uppercase tracking-wider text-primary-foreground hover:bg-primary/80 disabled:opacity-50">
            {saving ? <Loader2 className="h-4 w-4 animate-spin" /> : <Save className="h-4 w-4" />} حفظ
          </button>
        </div>
      </div>
    </div>
  );
}

// ── Balances admin ──
function Balances({ canEdit }: { canEdit: boolean }) {
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [empId, setEmpId] = useState("");
  const [rows, setRows] = useState<LeaveTypeInfo[]>([]);
  const [edits, setEdits] = useState<Record<string, { entitled: number; carried: number }>>({});
  const [loading, setLoading] = useState(false);

  useEffect(() => { getEmployees().then(setEmployees).catch(() => {}); }, []);

  const load = useCallback(async (id: string) => {
    if (!id) { setRows([]); return; }
    setLoading(true);
    try {
      const data = await getEmployeeLeaveBalances(id);
      setRows(data);
      setEdits(Object.fromEntries(data.map((r) => [r.id, { entitled: r.entitledDays, carried: 0 }])));
    } catch { toast.error("تعذر تحميل الأرصدة"); }
    finally { setLoading(false); }
  }, []);

  const save = async (typeId: string) => {
    const e = edits[typeId];
    if (!e) return;
    try { await setLeaveBalance(empId, typeId, e.entitled, e.carried); toast.success("تم تحديث الرصيد"); load(empId); }
    catch { toast.error("تعذر الحفظ"); }
  };

  return (
    <div className="space-y-3">
      <select value={empId} onChange={(e) => { setEmpId(e.target.value); load(e.target.value); }} className="h-10 border border-border bg-secondary px-3 text-sm">
        <option value="">— اختر موظفاً —</option>
        {employees.map((e) => <option key={e.id} value={e.id}>{e.name}</option>)}
      </select>

      {loading ? <Spinner /> : empId && rows.length > 0 ? (
        <div className="space-y-2">
          {rows.map((r) => (
            <div key={r.id} className="flex flex-wrap items-center gap-3 border border-border bg-card px-4 py-3">
              <span className="w-32 font-medium">{r.nameAr}</span>
              <span className="text-xs text-muted-foreground">المستخدم: {r.usedDays} · المتبقي: {r.remainingDays}</span>
              <div className="ms-auto flex items-center gap-2">
                <label className="text-xs text-muted-foreground">الرصيد</label>
                <input type="number" value={edits[r.id]?.entitled ?? 0} onChange={(e) => setEdits((p) => ({ ...p, [r.id]: { ...p[r.id], entitled: +e.target.value } }))} className="h-9 w-20 border border-border bg-secondary px-2 text-sm" disabled={!canEdit} />
                <label className="text-xs text-muted-foreground">مُرحّل</label>
                <input type="number" value={edits[r.id]?.carried ?? 0} onChange={(e) => setEdits((p) => ({ ...p, [r.id]: { ...p[r.id], carried: +e.target.value } }))} className="h-9 w-20 border border-border bg-secondary px-2 text-sm" disabled={!canEdit} />
                {canEdit && <button onClick={() => save(r.id)} className="inline-flex h-9 items-center gap-1 border border-border px-3 text-sm hover:bg-muted"><Check className="h-3.5 w-3.5" /> حفظ</button>}
              </div>
            </div>
          ))}
        </div>
      ) : empId ? <p className="text-sm text-muted-foreground">لا توجد بيانات</p> : <p className="text-sm text-muted-foreground">اختر موظفاً لإدارة أرصدته</p>}
    </div>
  );
}

// ── small UI ──
function Spinner() { return <div className="flex h-40 items-center justify-center text-muted-foreground"><Loader2 className="h-5 w-5 animate-spin" /></div>; }
function Tag({ children }: { children: ReactNode }) { return <span className="border border-border bg-secondary px-1.5 py-0.5">{children}</span>; }
function Field({ label, children }: { label: string; children: ReactNode }) { return <div className="space-y-1"><label className="text-xs font-bold uppercase tracking-wider text-muted-foreground">{label}</label>{children}</div>; }
function Check2({ label, checked, onChange }: { label: string; checked: boolean; onChange: (v: boolean) => void }) {
  return <label className="flex h-9 items-center gap-2 text-sm"><input type="checkbox" checked={checked} onChange={(e) => onChange(e.target.checked)} /> {label}</label>;
}
