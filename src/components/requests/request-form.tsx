"use client";

import { useState } from "react";
import { Loader2, Upload, Check } from "lucide-react";
import { toast } from "sonner";
import { uploadFile } from "@/lib/api/files";
import { ApiError } from "@/lib/api-client";
import { RequestField, RequestTypeDetail, RequestValue, submitRequest } from "@/lib/api/request-center";

interface RequestFormProps {
  type: RequestTypeDetail;
  onSubmitted: () => void;
  onCancel: () => void;
}

export function RequestForm({ type, onSubmitted, onCancel }: RequestFormProps) {
  const [values, setValues] = useState<Record<string, string>>({});
  const [files, setFiles] = useState<Record<string, string>>({}); // fieldCode → fileUrl
  const [uploading, setUploading] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const set = (code: string, v: string) => setValues((p) => ({ ...p, [code]: v }));

  const onFile = async (field: RequestField, file?: File) => {
    if (!file) return;
    setUploading(field.code);
    try {
      const res = await uploadFile(file, "requests");
      setFiles((p) => ({ ...p, [field.code]: res.url }));
      toast.success("تم رفع الملف");
    } catch {
      toast.error("تعذر رفع الملف");
    } finally {
      setUploading(null);
    }
  };

  const submit = async () => {
    // client-side required check (server also enforces)
    const missing = type.fields.filter((f) => f.isRequired && !(values[f.code]?.trim() || files[f.code]));
    if (missing.length) {
      toast.error(`حقول مطلوبة: ${missing.map((f) => f.nameAr).join("، ")}`);
      return;
    }
    setSaving(true);
    try {
      const payload: RequestValue[] = type.fields.map((f) => ({
        fieldCode: f.code,
        value: values[f.code] ?? null,
        fileUrl: files[f.code] ?? null,
        formFieldId: f.id,
      }));
      await submitRequest(type.id, payload);
      toast.success("تم تقديم الطلب بنجاح");
      onSubmitted();
    } catch (e) {
      toast.error(e instanceof ApiError ? e.message : "تعذر تقديم الطلب");
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="space-y-4">
      <div className="space-y-3">
        {type.fields.map((f) => (
          <div key={f.id} className="space-y-1">
            <label className="text-xs font-bold uppercase tracking-wider text-muted-foreground">
              {f.nameAr}{f.isRequired && <span className="text-destructive"> *</span>}
            </label>
            {renderField(f, values[f.code] ?? "", set, files[f.code], onFile, uploading === f.code)}
          </div>
        ))}
        {type.fields.length === 0 && <p className="text-sm text-muted-foreground">لا توجد حقول في هذا النموذج.</p>}
      </div>

      <div className="flex items-center justify-end gap-2 border-t border-border pt-4">
        <button onClick={onCancel} className="h-10 px-4 text-sm text-muted-foreground hover:text-foreground">إلغاء</button>
        <button onClick={submit} disabled={saving}
          className="inline-flex h-10 items-center gap-2 bg-primary px-5 text-sm font-bold uppercase tracking-wider text-primary-foreground hover:bg-primary/80 disabled:opacity-50">
          {saving ? <Loader2 className="h-4 w-4 animate-spin" /> : <Check className="h-4 w-4" />} تقديم الطلب
        </button>
      </div>
    </div>
  );
}

function renderField(
  f: RequestField,
  value: string,
  set: (code: string, v: string) => void,
  fileUrl: string | undefined,
  onFile: (f: RequestField, file?: File) => void,
  uploading: boolean,
) {
  const cls = "h-10 w-full border border-border bg-secondary px-3 text-sm";
  switch (f.fieldType) {
    case "Date":
      return <input type="date" value={value} onChange={(e) => set(f.code, e.target.value)} className={cls} />;
    case "DateTime":
      return <input type="datetime-local" value={value} onChange={(e) => set(f.code, e.target.value)} className={cls} />;
    case "Number": case "Decimal": case "Currency": case "Percentage":
      return <input type="number" value={value} onChange={(e) => set(f.code, e.target.value)} placeholder={f.placeholder ?? ""} className={cls} />;
    case "Boolean":
      return (
        <label className="flex items-center gap-2 text-sm">
          <input type="checkbox" checked={value === "true"} onChange={(e) => set(f.code, e.target.checked ? "true" : "false")} /> نعم
        </label>
      );
    case "TextArea":
      return <textarea value={value} onChange={(e) => set(f.code, e.target.value)} placeholder={f.placeholder ?? ""} rows={3} className="w-full border border-border bg-secondary px-3 py-2 text-sm" />;
    case "Dropdown": case "MultiSelect":
      return (
        <select value={value} onChange={(e) => set(f.code, e.target.value)} className={cls}>
          <option value="">— اختر —</option>
          {parseOptions(f.options).map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
        </select>
      );
    case "File": case "Image":
      return (
        <div className="flex items-center gap-2">
          <label className="inline-flex h-10 cursor-pointer items-center gap-2 border border-border bg-secondary px-3 text-sm hover:bg-muted">
            {uploading ? <Loader2 className="h-4 w-4 animate-spin" /> : <Upload className="h-4 w-4" />}
            {fileUrl ? "تم الرفع" : "اختر ملفاً"}
            <input type="file" className="hidden" onChange={(e) => onFile(f, e.target.files?.[0])} />
          </label>
          {fileUrl && <Check className="h-4 w-4 text-green-400" />}
        </div>
      );
    default:
      return <input type="text" value={value} onChange={(e) => set(f.code, e.target.value)} placeholder={f.placeholder ?? ""} className={cls} />;
  }
}

function parseOptions(raw?: string | null): { value: string; label: string }[] {
  if (!raw) return [];
  try {
    const parsed = JSON.parse(raw);
    if (Array.isArray(parsed)) {
      return parsed.map((o) =>
        typeof o === "string" ? { value: o, label: o } : { value: String(o.value ?? o.code ?? o), label: String(o.label ?? o.nameAr ?? o.name ?? o.value ?? o) });
    }
  } catch { /* ignore */ }
  return [];
}
