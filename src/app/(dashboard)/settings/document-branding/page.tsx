"use client";

import { type ElementType, type ReactNode, useEffect, useRef, useState } from "react";
import Link from "next/link";
import { ArrowRight, Building2, FileText, Loader2, PenLine, Save, Stamp, Upload } from "lucide-react";
import { toast } from "sonner";
import { ApiError } from "@/lib/api-client";
import { usePermissions } from "@/lib/permissions";
import { fileUrl, uploadFile } from "@/lib/api/files";
import { CompanyProfile, getCompanyProfile, saveCompanyProfile } from "@/lib/api/company";

const GLOBAL_TOKENS = [
  "{{Company.Name}}", "{{Company.NameEn}}", "{{Company.CR}}", "{{Company.VAT}}",
  "{{Company.Address}}", "{{Company.Phone}}", "{{Company.Email}}", "{{Company.Website}}",
];

export default function DocumentBrandingPage() {
  const { has } = usePermissions();
  const canEdit = has("Platform.CompanyConfig.Edit") || has("Settings.Edit") || has("Platform.Documents.Edit");
  const [p, setP] = useState<Partial<CompanyProfile>>({});
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => { getCompanyProfile().then((d) => setP(d ?? {})).catch(() => {}).finally(() => setLoading(false)); }, []);
  const set = (k: keyof CompanyProfile, v: string) => setP((s) => ({ ...s, [k]: v }));

  const save = async () => {
    if (!p.nameAr?.trim() || !p.nameEn?.trim()) { toast.error("اسم الشركة (عربي/إنجليزي) مطلوب"); return; }
    setSaving(true);
    try { const saved = await saveCompanyProfile(p); setP(saved); toast.success("تم حفظ هوية المستندات"); }
    catch (e) { toast.error(e instanceof ApiError ? e.message : "تعذر الحفظ"); }
    finally { setSaving(false); }
  };

  if (loading) return <div className="flex h-64 items-center justify-center text-muted-foreground"><Loader2 className="h-6 w-6 animate-spin" /></div>;

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold">هوية المستندات</h1>
          <p className="mt-1 text-sm text-muted-foreground">الشعار والختم والتواقيع وبيانات الشركة — تُحقن تلقائياً في كل مستند مُولّد</p>
        </div>
        <Link href="/settings/document-templates" className="inline-flex h-10 items-center gap-2 border border-border px-4 text-sm hover:bg-muted"><ArrowRight className="h-4 w-4" /> قوالب المستندات</Link>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <ImageField label="شعار الشركة" icon={Building2} url={p.logoUrl} disabled={!canEdit} onUploaded={(u) => set("logoUrl", u)} />
        <ImageField label="الختم الرسمي" icon={Stamp} url={p.stampUrl} disabled={!canEdit} onUploaded={(u) => set("stampUrl", u)} />
        <ImageField label="توقيع الموارد البشرية" icon={PenLine} url={p.hrSignatureUrl} disabled={!canEdit} onUploaded={(u) => set("hrSignatureUrl", u)} />
        <ImageField label="توقيع الرئيس التنفيذي" icon={PenLine} url={p.ceoSignatureUrl} disabled={!canEdit} onUploaded={(u) => set("ceoSignatureUrl", u)} />
      </div>

      <Card title="هوية الشركة (رموز عامة)">
        <Field label="الاسم (عربي) *"><Input value={p.nameAr} onChange={(v) => set("nameAr", v)} disabled={!canEdit} /></Field>
        <Field label="الاسم (إنجليزي) *"><Input value={p.nameEn} onChange={(v) => set("nameEn", v)} dir="ltr" disabled={!canEdit} /></Field>
        <Field label="السجل التجاري"><Input value={p.commercialRegistration} onChange={(v) => set("commercialRegistration", v)} dir="ltr" disabled={!canEdit} /></Field>
        <Field label="الرقم الضريبي (VAT)"><Input value={p.vatNumber} onChange={(v) => set("vatNumber", v)} dir="ltr" disabled={!canEdit} /></Field>
        <Field label="العنوان"><Input value={p.address} onChange={(v) => set("address", v)} disabled={!canEdit} /></Field>
        <Field label="الهاتف"><Input value={p.phone} onChange={(v) => set("phone", v)} dir="ltr" disabled={!canEdit} /></Field>
        <Field label="البريد الإلكتروني"><Input value={p.email} onChange={(v) => set("email", v)} dir="ltr" disabled={!canEdit} /></Field>
        <Field label="الموقع الإلكتروني"><Input value={p.website} onChange={(v) => set("website", v)} dir="ltr" disabled={!canEdit} /></Field>
      </Card>

      <div className="border border-border bg-card p-5">
        <h2 className="mb-3 flex items-center gap-2 text-sm font-bold uppercase tracking-wider text-muted-foreground"><FileText className="h-4 w-4" /> الرموز العامة المتاحة في القوالب</h2>
        <div className="flex flex-wrap gap-2">
          {GLOBAL_TOKENS.map((t) => <span key={t} className="border border-border bg-secondary px-2 py-1 font-mono text-xs text-muted-foreground">{t}</span>)}
        </div>
      </div>

      {canEdit && (
        <div className="flex justify-end">
          <button onClick={save} disabled={saving} className="inline-flex h-10 items-center gap-2 bg-primary px-6 text-sm font-bold uppercase tracking-wider text-primary-foreground hover:bg-primary/80 disabled:opacity-50">{saving ? <Loader2 className="h-4 w-4 animate-spin" /> : <Save className="h-4 w-4" />} حفظ</button>
        </div>
      )}
    </div>
  );
}

function ImageField({ label, icon: Icon, url, onUploaded, disabled }: { label: string; icon: ElementType; url?: string | null; onUploaded: (u: string) => void; disabled?: boolean }) {
  const [uploading, setUploading] = useState(false);
  const ref = useRef<HTMLInputElement>(null);
  const onFile = async (f?: File) => {
    if (!f) return;
    setUploading(true);
    try { const r = await uploadFile(f, "branding"); onUploaded(r.url); toast.success("تم الرفع"); }
    catch { toast.error("تعذر الرفع"); }
    finally { setUploading(false); }
  };
  return (
    <div className="flex flex-col items-center gap-3 border border-border bg-card p-4 text-center">
      <div className="flex h-24 w-full items-center justify-center border border-border bg-secondary">
        {url ? <img src={fileUrl(url)} alt={label} className="h-full w-full object-contain" /> : <Icon className="h-7 w-7 text-muted-foreground" />}
      </div>
      <p className="text-sm font-medium">{label}</p>
      {!disabled && (
        <>
          <button onClick={() => ref.current?.click()} disabled={uploading} className="inline-flex h-9 items-center gap-2 border border-border px-3 text-sm hover:bg-muted disabled:opacity-50">
            {uploading ? <Loader2 className="h-4 w-4 animate-spin" /> : <Upload className="h-4 w-4" />} {url ? "تغيير" : "رفع"}
          </button>
          <input ref={ref} type="file" accept="image/*" className="hidden" onChange={(e) => onFile(e.target.files?.[0])} />
        </>
      )}
    </div>
  );
}

function Card({ title, children }: { title: string; children: ReactNode }) {
  return <div className="border border-border bg-card p-5"><h2 className="mb-3 text-sm font-bold uppercase tracking-wider text-muted-foreground">{title}</h2><div className="grid grid-cols-1 gap-3 sm:grid-cols-2">{children}</div></div>;
}
function Field({ label, children }: { label: string; children: ReactNode }) {
  return <div className="space-y-1"><label className="text-xs font-bold uppercase tracking-wider text-muted-foreground">{label}</label>{children}</div>;
}
function Input({ value, onChange, dir, disabled }: { value?: string | null; onChange: (v: string) => void; dir?: string; disabled?: boolean }) {
  return <input value={value ?? ""} onChange={(e) => onChange(e.target.value)} dir={dir} disabled={disabled} className="h-10 w-full border border-border bg-secondary px-3 text-sm disabled:opacity-60" />;
}
