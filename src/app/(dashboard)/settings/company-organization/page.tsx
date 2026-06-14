"use client";

import { type ElementType, type ReactNode, useEffect, useRef, useState } from "react";
import Link from "next/link";
import {
  ArrowRight, BriefcaseBusiness, Building, Building2, ChevronLeft, Coins, Globe, Landmark,
  Layers, Loader2, Network, PenLine, Save, Scale, Stamp, Upload,
} from "lucide-react";
import { toast } from "sonner";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { ApiError } from "@/lib/api-client";
import { usePermissions } from "@/lib/permissions";
import { fileUrl, uploadFile } from "@/lib/api/files";
import { CompanyProfile, getCompanyProfile, saveCompanyProfile } from "@/lib/api/company";

const ORG_MODULES = [
  { href: "/settings/organization/departments", title: "الأقسام", description: "الهيكل والتسلسل الإداري + المخطط التنظيمي", icon: Network },
  { href: "/settings/organization/job-titles", title: "المسميات الوظيفية", description: "المسميات ومسؤولياتها ومهاراتها", icon: BriefcaseBusiness },
  { href: "/settings/organization/branches", title: "الفروع", description: "الفروع ومواقعها وإحداثياتها", icon: Building },
  { href: "/settings/organization/nationalities", title: "الجنسيات", description: "الجنسيات في ملفات الموظفين", icon: Globe },
  { href: "/settings/organization/grades", title: "الدرجات الوظيفية", description: "درجات ومستويات الموظفين", icon: Layers },
  { href: "/settings/organization/cost-centers", title: "مراكز التكلفة", description: "مراكز التكلفة المحاسبية", icon: Coins },
];

export default function CompanyOrganizationPage() {
  const { has } = usePermissions();
  const canEdit = has("Platform.CompanyConfig.Edit") || has("Settings.Edit") || has("Platform.Documents.Edit");
  const [p, setP] = useState<Partial<CompanyProfile>>({});
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => { getCompanyProfile().then((d) => setP(d ?? {})).catch(() => {}).finally(() => setLoading(false)); }, []);
  const set = (k: keyof CompanyProfile, v: string | number) => setP((s) => ({ ...s, [k]: v }));

  const save = async () => {
    if (!p.nameAr?.trim() || !p.nameEn?.trim()) { toast.error("اسم الشركة (عربي/إنجليزي) مطلوب"); return; }
    setSaving(true);
    try { const saved = await saveCompanyProfile(p); setP(saved); toast.success("تم الحفظ"); }
    catch (e) { toast.error(e instanceof ApiError ? e.message : "تعذر الحفظ"); }
    finally { setSaving(false); }
  };

  if (loading) return <div className="flex h-64 items-center justify-center text-muted-foreground"><Loader2 className="h-6 w-6 animate-spin" /></div>;

  const SaveBar = canEdit ? (
    <div className="flex justify-end">
      <button onClick={save} disabled={saving} className="inline-flex h-10 items-center gap-2 bg-primary px-6 text-sm font-bold uppercase tracking-wider text-primary-foreground hover:bg-primary/80 disabled:opacity-50">
        {saving ? <Loader2 className="h-4 w-4 animate-spin" /> : <Save className="h-4 w-4" />} حفظ
      </button>
    </div>
  ) : null;

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold">الشركة والمؤسسة</h1>
          <p className="mt-1 text-sm text-muted-foreground">هوية الشركة، العلامة التجارية للمستندات، المعلومات النظامية، والهيكل التنظيمي — في مكان واحد</p>
        </div>
        <Link href="/settings" className="inline-flex h-10 items-center gap-2 border border-border px-4 text-sm hover:bg-muted"><ArrowRight className="h-4 w-4" /> الإعدادات</Link>
      </div>

      <Tabs defaultValue="company" className="w-full">
        <TabsList className="flex h-auto flex-wrap justify-start gap-1 bg-transparent p-0">
          {[["company", "الشركة", Landmark], ["branding", "هوية المستندات", Stamp], ["legal", "المعلومات النظامية", Scale], ["organization", "الهيكل التنظيمي", Building2]].map(([v, l, Icon]) => (
            <TabsTrigger key={v as string} value={v as string} className="gap-2 border border-border text-xs data-[state=active]:bg-primary data-[state=active]:text-primary-foreground">
              {(() => { const I = Icon as ElementType; return <I className="h-3.5 w-3.5" />; })()} {l as string}
            </TabsTrigger>
          ))}
        </TabsList>

        {/* Company */}
        <TabsContent value="company" className="mt-4 space-y-4">
          <Card title="هوية الشركة">
            <Field label="الاسم (عربي) *"><Input value={p.nameAr} onChange={(v) => set("nameAr", v)} disabled={!canEdit} /></Field>
            <Field label="الاسم (إنجليزي) *"><Input value={p.nameEn} onChange={(v) => set("nameEn", v)} dir="ltr" disabled={!canEdit} /></Field>
            <Field label="السجل التجاري"><Input value={p.commercialRegistration} onChange={(v) => set("commercialRegistration", v)} dir="ltr" disabled={!canEdit} /></Field>
            <Field label="الرقم الضريبي (VAT)"><Input value={p.vatNumber} onChange={(v) => set("vatNumber", v)} dir="ltr" disabled={!canEdit} /></Field>
          </Card>
          <Card title="بيانات التواصل">
            <Field label="الموقع الإلكتروني"><Input value={p.website} onChange={(v) => set("website", v)} dir="ltr" disabled={!canEdit} /></Field>
            <Field label="البريد الإلكتروني"><Input value={p.email} onChange={(v) => set("email", v)} dir="ltr" disabled={!canEdit} /></Field>
            <Field label="الهاتف"><Input value={p.phone} onChange={(v) => set("phone", v)} dir="ltr" disabled={!canEdit} /></Field>
          </Card>
          <Card title="العنوان">
            <Field label="العنوان"><Input value={p.address} onChange={(v) => set("address", v)} disabled={!canEdit} /></Field>
            <Field label="المدينة"><Input value={p.city} onChange={(v) => set("city", v)} disabled={!canEdit} /></Field>
            <Field label="الدولة"><Input value={p.country} onChange={(v) => set("country", v)} disabled={!canEdit} /></Field>
            <Field label="الرمز البريدي"><Input value={p.postalCode} onChange={(v) => set("postalCode", v)} dir="ltr" disabled={!canEdit} /></Field>
          </Card>
          {SaveBar}
        </TabsContent>

        {/* Branding */}
        <TabsContent value="branding" className="mt-4 space-y-4">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <ImageField label="شعار الشركة" icon={Building2} url={p.logoUrl} disabled={!canEdit} onUploaded={(u) => set("logoUrl", u)} />
            <ImageField label="الختم الرسمي" icon={Stamp} url={p.stampUrl} disabled={!canEdit} onUploaded={(u) => set("stampUrl", u)} />
            <ImageField label="توقيع الموارد البشرية" icon={PenLine} url={p.hrSignatureUrl} disabled={!canEdit} onUploaded={(u) => set("hrSignatureUrl", u)} />
            <ImageField label="توقيع الرئيس التنفيذي" icon={PenLine} url={p.ceoSignatureUrl} disabled={!canEdit} onUploaded={(u) => set("ceoSignatureUrl", u)} />
          </div>
          <p className="text-sm text-muted-foreground">تُحقن هذه العناصر تلقائياً في كل مستند رسمي يُولّده النظام.</p>
          {SaveBar}
        </TabsContent>

        {/* Legal */}
        <TabsContent value="legal" className="mt-4 space-y-4">
          <Card title="المعلومات النظامية">
            <Field label="رقم منشأة وزارة العمل"><Input value={p.molNumber} onChange={(v) => set("molNumber", v)} dir="ltr" disabled={!canEdit} /></Field>
            <Field label="رقم منشأة التأمينات (GOSI)"><Input value={p.gosiNumber} onChange={(v) => set("gosiNumber", v)} dir="ltr" disabled={!canEdit} /></Field>
            <Field label="نسبة التأمينات على الموظف (%)"><Input type="number" value={p.gosiRate != null ? String(p.gosiRate) : ""} onChange={(v) => set("gosiRate", Number(v))} dir="ltr" disabled={!canEdit} /></Field>
          </Card>
          <p className="text-sm text-muted-foreground">تُستخدم نسبة GOSI في احتساب صافي الراتب لجميع الموظفين.</p>
          {SaveBar}
        </TabsContent>

        {/* Organization */}
        <TabsContent value="organization" className="mt-4">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {ORG_MODULES.map((m) => {
              const Icon = m.icon;
              return (
                <Link key={m.href} href={m.href} className="group border border-border bg-card p-5 hover:border-primary/50 hover:bg-card/70 transition-colors">
                  <div className="flex items-start justify-between">
                    <div className="flex h-10 w-10 items-center justify-center bg-primary/10 text-primary"><Icon className="h-5 w-5" /></div>
                    <ChevronLeft className="h-4 w-4 text-muted-foreground group-hover:text-primary transition-colors" />
                  </div>
                  <h2 className="mt-4 text-base font-bold">{m.title}</h2>
                  <p className="mt-1 text-xs leading-relaxed text-muted-foreground">{m.description}</p>
                </Link>
              );
            })}
          </div>
        </TabsContent>
      </Tabs>
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
function Input({ value, onChange, dir, disabled, type }: { value?: string | null; onChange: (v: string) => void; dir?: string; disabled?: boolean; type?: string }) {
  return <input type={type} value={value ?? ""} onChange={(e) => onChange(e.target.value)} dir={dir} disabled={disabled} className="h-10 w-full border border-border bg-secondary px-3 text-sm disabled:opacity-60" />;
}
