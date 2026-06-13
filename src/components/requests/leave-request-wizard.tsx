"use client";

import { type ElementType, type ReactNode, useCallback, useEffect, useMemo, useState } from "react";
import { AlertCircle, Calendar, Check, Loader2, Upload, UserCheck, Wallet } from "lucide-react";
import { toast } from "sonner";
import { uploadFile } from "@/lib/api/files";
import { ApiError } from "@/lib/api-client";
import {
  getLeaveTypes, LeavePreview, LeaveTypeInfo, previewLeave, RequestTypeDetail, RequestValue, submitRequest,
} from "@/lib/api/request-center";

interface Props {
  type: RequestTypeDetail;
  onSubmitted: () => void;
  onCancel: () => void;
}

export function LeaveRequestWizard({ type, onSubmitted, onCancel }: Props) {
  const [leaveTypes, setLeaveTypes] = useState<LeaveTypeInfo[]>([]);
  const [leaveTypeId, setLeaveTypeId] = useState("");
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");
  const [notes, setNotes] = useState("");
  const [attachmentUrl, setAttachmentUrl] = useState<string | null>(null);
  const [uploading, setUploading] = useState(false);
  const [preview, setPreview] = useState<LeavePreview | null>(null);
  const [previewing, setPreviewing] = useState(false);
  const [saving, setSaving] = useState(false);

  const fieldId = (code: string) => type.fields.find((f) => f.code === code)?.id ?? null;
  const selected = useMemo(() => leaveTypes.find((t) => t.id === leaveTypeId) ?? null, [leaveTypes, leaveTypeId]);
  const requiresAttachment = preview?.requiresAttachment ?? selected?.rules.requiresAttachment ?? false;

  useEffect(() => {
    getLeaveTypes().then(setLeaveTypes).catch(() => toast.error("تعذر تحميل أنواع الإجازات"));
  }, []);

  // Live preview: days, balance, next approver, validation.
  const runPreview = useCallback(async () => {
    if (!leaveTypeId || !startDate || !endDate) { setPreview(null); return; }
    setPreviewing(true);
    try {
      setPreview(await previewLeave(leaveTypeId, startDate, endDate, !!attachmentUrl));
    } catch { setPreview(null); }
    finally { setPreviewing(false); }
  }, [leaveTypeId, startDate, endDate, attachmentUrl]);

  useEffect(() => { runPreview(); }, [runPreview]);

  const onFile = async (file?: File) => {
    if (!file) return;
    setUploading(true);
    try { const r = await uploadFile(file, "requests"); setAttachmentUrl(r.url); toast.success("تم رفع المرفق"); }
    catch { toast.error("تعذر رفع الملف"); }
    finally { setUploading(false); }
  };

  const canSubmit = !!leaveTypeId && !!startDate && !!endDate && (preview?.isValid ?? false) && !(requiresAttachment && !attachmentUrl);

  const submit = async () => {
    if (!canSubmit) return;
    setSaving(true);
    try {
      const values: RequestValue[] = [
        { fieldCode: "leaveType", value: leaveTypeId, formFieldId: fieldId("leaveType") },
        { fieldCode: "startDate", value: startDate, formFieldId: fieldId("startDate") },
        { fieldCode: "endDate", value: endDate, formFieldId: fieldId("endDate") },
        { fieldCode: "attachment", fileUrl: attachmentUrl, formFieldId: fieldId("attachment") },
        { fieldCode: "notes", value: notes, formFieldId: fieldId("notes") },
      ];
      await submitRequest(type.id, values);
      toast.success("تم تقديم طلب الإجازة");
      onSubmitted();
    } catch (e) {
      toast.error(e instanceof ApiError ? e.message : "تعذر تقديم الطلب");
    } finally {
      setSaving(false);
    }
  };

  const inputCls = "h-10 w-full border border-border bg-secondary px-3 text-sm";

  return (
    <div className="space-y-4">
      {/* Leave type */}
      <div className="space-y-1">
        <label className="text-xs font-bold uppercase tracking-wider text-muted-foreground">نوع الإجازة <span className="text-destructive">*</span></label>
        <select value={leaveTypeId} onChange={(e) => setLeaveTypeId(e.target.value)} className={inputCls}>
          <option value="">— اختر نوع الإجازة —</option>
          {leaveTypes.map((t) => <option key={t.id} value={t.id}>{t.nameAr}{t.rules.paid ? "" : " (بدون راتب)"}</option>)}
        </select>
      </div>

      {/* Selected type rules summary */}
      {selected && (
        <div className="flex flex-wrap gap-2 text-xs">
          <Chip>{selected.rules.paid ? `مدفوعة ${selected.rules.paidPercentage}%` : "بدون راتب"}</Chip>
          <Chip>الرصيد المتبقي: {selected.remainingDays} يوم</Chip>
          <Chip>الحد الأقصى: {selected.rules.maxDays} يوم</Chip>
          {selected.rules.requiresAttachment && <Chip tone="warn">يتطلب مرفقاً</Chip>}
          {selected.rules.affectsPayroll && <Chip tone="warn">يؤثر على الراتب</Chip>}
        </div>
      )}

      {/* Dates */}
      <div className="grid grid-cols-2 gap-3">
        <div className="space-y-1">
          <label className="text-xs font-bold uppercase tracking-wider text-muted-foreground">من <span className="text-destructive">*</span></label>
          <input type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} className={inputCls} />
        </div>
        <div className="space-y-1">
          <label className="text-xs font-bold uppercase tracking-wider text-muted-foreground">إلى <span className="text-destructive">*</span></label>
          <input type="date" value={endDate} min={startDate || undefined} onChange={(e) => setEndDate(e.target.value)} className={inputCls} />
        </div>
      </div>

      {/* Attachment (conditional) */}
      {requiresAttachment && (
        <div className="space-y-1">
          <label className="text-xs font-bold uppercase tracking-wider text-muted-foreground">مرفق <span className="text-destructive">*</span></label>
          <label className="inline-flex h-10 cursor-pointer items-center gap-2 border border-border bg-secondary px-3 text-sm hover:bg-muted">
            {uploading ? <Loader2 className="h-4 w-4 animate-spin" /> : <Upload className="h-4 w-4" />}
            {attachmentUrl ? "تم الرفع" : "اختر ملفاً"}
            <input type="file" className="hidden" onChange={(e) => onFile(e.target.files?.[0])} />
            {attachmentUrl && <Check className="h-4 w-4 text-green-400" />}
          </label>
        </div>
      )}

      {/* Notes */}
      <div className="space-y-1">
        <label className="text-xs font-bold uppercase tracking-wider text-muted-foreground">ملاحظات</label>
        <textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={2} className="w-full border border-border bg-secondary px-3 py-2 text-sm" />
      </div>

      {/* Review / impact panel */}
      <div className="border border-border bg-secondary/40 p-3">
        <p className="mb-2 flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-muted-foreground">
          <Calendar className="h-3.5 w-3.5" /> المراجعة {previewing && <Loader2 className="h-3 w-3 animate-spin" />}
        </p>
        {preview && leaveTypeId && startDate && endDate ? (
          <div className="space-y-1.5 text-sm">
            <Row label="عدد الأيام" value={`${preview.days} يوم`} />
            <Row label="الرصيد قبل" value={`${preview.balanceBefore} يوم`} />
            <Row label="الرصيد بعد" value={`${preview.balanceAfter} يوم`} highlight={preview.balanceAfter < 0} />
            <Row label="الموافق التالي" value={preview.nextApproverAr ?? "—"} icon={UserCheck} />
            <div className="mt-2 border-t border-border pt-2 text-xs text-muted-foreground">
              <p className="flex items-center gap-1"><Wallet className="h-3 w-3" /> الأثر عند الموافقة:</p>
              <ul className="mt-1 list-inside list-disc space-y-0.5">
                {preview.affectsAttendance && <li>سيتم تسجيل الأيام كإجازة في الحضور</li>}
                {preview.paid && <li>سيتم خصم {preview.days} يوم من الرصيد</li>}
                {preview.affectsPayroll && <li>أثر على الراتب ({preview.paidPercentage}% مدفوع)</li>}
              </ul>
            </div>
            {preview.errors.length > 0 && (
              <div className="mt-2 space-y-1">
                {preview.errors.map((er, i) => (
                  <p key={i} className="flex items-center gap-1 text-xs text-destructive"><AlertCircle className="h-3 w-3" /> {er}</p>
                ))}
              </div>
            )}
          </div>
        ) : (
          <p className="text-sm text-muted-foreground">اختر نوع الإجازة والتواريخ لعرض المراجعة</p>
        )}
      </div>

      {/* Actions */}
      <div className="flex items-center justify-end gap-2 border-t border-border pt-4">
        <button onClick={onCancel} className="h-10 px-4 text-sm text-muted-foreground hover:text-foreground">إلغاء</button>
        <button onClick={submit} disabled={!canSubmit || saving}
          className="inline-flex h-10 items-center gap-2 bg-primary px-5 text-sm font-bold uppercase tracking-wider text-primary-foreground hover:bg-primary/80 disabled:opacity-40">
          {saving ? <Loader2 className="h-4 w-4 animate-spin" /> : <Check className="h-4 w-4" />} تقديم الطلب
        </button>
      </div>
    </div>
  );
}

function Chip({ children, tone }: { children: ReactNode; tone?: "warn" }) {
  return <span className={`border px-2 py-0.5 ${tone === "warn" ? "border-amber-500/30 bg-amber-500/10 text-amber-400" : "border-border bg-card text-muted-foreground"}`}>{children}</span>;
}

function Row({ label, value, highlight, icon: Icon }: { label: string; value: string; highlight?: boolean; icon?: ElementType }) {
  return (
    <div className="flex items-center justify-between">
      <span className="flex items-center gap-1 text-muted-foreground">{Icon && <Icon className="h-3.5 w-3.5" />} {label}</span>
      <span className={highlight ? "font-bold text-destructive" : "font-medium"}>{value}</span>
    </div>
  );
}
