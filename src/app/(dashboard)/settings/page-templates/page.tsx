"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { ArrowRight, LayoutTemplate, Loader2, Lock, Plus, Save, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { ApiError } from "@/lib/api-client";
import { usePermissions } from "@/lib/permissions";
import {
  createPageTemplate, deletePageTemplate, FooterConfig, getPageTemplate, getPageTemplates, HeaderConfig,
  Margins, PageTemplate, parseJson, updatePageTemplate, Watermark,
} from "@/lib/api/page-templates";

export default function PageTemplatesPage() {
  const { has } = usePermissions();
  const canEdit = has("Platform.Documents.Edit") || has("Platform.Documents.Create");
  const [items, setItems] = useState<PageTemplate[]>([]);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState<PageTemplate | "new" | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    try { setItems(await getPageTemplates()); }
    catch { toast.error("تعذر التحميل"); }
    finally { setLoading(false); }
  }, []);
  useEffect(() => { load(); }, [load]);

  if (editing) return <Editor item={editing === "new" ? null : editing} canEdit={canEdit} onClose={() => setEditing(null)} onSaved={() => { setEditing(null); load(); }} />;

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold">قوالب الصفحات</h1>
          <p className="mt-1 text-sm text-muted-foreground">الترويسة والتذييل والهوامش والعلامة المائية — تُورّثها قوالب المستندات</p>
        </div>
        <div className="flex items-center gap-2">
          {canEdit && <button onClick={() => setEditing("new")} className="inline-flex h-10 items-center gap-2 bg-primary px-4 text-sm font-bold uppercase tracking-wider text-primary-foreground hover:bg-primary/80"><Plus className="h-4 w-4" /> قالب صفحة</button>}
          <Link href="/settings/document-templates" className="inline-flex h-10 items-center gap-2 border border-border px-4 text-sm hover:bg-muted"><ArrowRight className="h-4 w-4" /> قوالب المستندات</Link>
        </div>
      </div>

      {loading ? <div className="flex h-64 items-center justify-center text-muted-foreground"><Loader2 className="h-6 w-6 animate-spin" /></div> : (
        <div className="grid grid-cols-1 gap-2 sm:grid-cols-2">
          {items.map((p) => (
            <button key={p.id} onClick={() => setEditing(p)} className="flex items-center justify-between border border-border bg-card px-4 py-3 text-right hover:bg-muted/40">
              <div className="flex items-center gap-3">
                <LayoutTemplate className="h-5 w-5 text-primary" />
                <div><div className="flex items-center gap-2 font-medium">{p.nameAr}{p.isSystem && <Lock className="h-3.5 w-3.5 text-amber-400" />}</div><div className="font-mono text-xs text-muted-foreground">{p.code}</div></div>
              </div>
            </button>
          ))}
          {items.length === 0 && <div className="col-span-full border border-dashed border-border py-12 text-center text-sm text-muted-foreground">لا توجد قوالب صفحات — شغّل تهيئة الطلبات لإنشاء الإعدادات الجاهزة</div>}
        </div>
      )}
    </div>
  );
}

function Editor({ item, canEdit, onClose, onSaved }: { item: PageTemplate | null; canEdit: boolean; onClose: () => void; onSaved: () => void }) {
  const locked = !canEdit || (item?.isSystem ?? false);
  const [code, setCode] = useState(item?.code ?? "");
  const [nameAr, setNameAr] = useState(item?.nameAr ?? "");
  const [nameEn, setNameEn] = useState(item?.nameEn ?? "");
  const [header, setHeader] = useState<HeaderConfig>({ showLogo: true, logoPlacement: "Left", showIdentity: true, showCrVat: true, showContact: true });
  const [footer, setFooter] = useState<FooterConfig>({ showQr: true, qrPlacement: "Left", showStamp: true, showSignatures: false, showGeneratedDate: true });
  const [margins, setMargins] = useState<Margins>({ top: 40, right: 40, bottom: 40, left: 40 });
  const [watermark, setWatermark] = useState<Watermark>({ text: "" });
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!item) return;
    getPageTemplate(item.id).then((p) => {
      setHeader({ showLogo: true, logoPlacement: "Left", showIdentity: true, showCrVat: true, showContact: true, ...parseJson<HeaderConfig>(p.headerConfig) });
      setFooter({ showQr: true, qrPlacement: "Left", showStamp: true, showSignatures: false, showGeneratedDate: true, ...parseJson<FooterConfig>(p.footerConfig) });
      setMargins({ top: 40, right: 40, bottom: 40, left: 40, ...parseJson<Margins>(p.margins) });
      setWatermark({ text: "", ...parseJson<Watermark>(p.watermark) });
    }).catch(() => {});
  }, [item]);

  const save = async () => {
    if (!nameAr.trim() || !nameEn.trim() || (!item && !code.trim())) { toast.error("أكمل الحقول المطلوبة"); return; }
    setSaving(true);
    try {
      const payload = {
        nameAr, nameEn,
        headerConfig: JSON.stringify(header), footerConfig: JSON.stringify(footer),
        margins: JSON.stringify(margins), watermark: watermark.text ? JSON.stringify(watermark) : null,
      };
      if (item) await updatePageTemplate(item.id, payload);
      else await createPageTemplate({ code: code.trim().toUpperCase(), ...payload });
      toast.success("تم الحفظ"); onSaved();
    } catch (e) { toast.error(e instanceof ApiError ? e.message : "تعذر الحفظ"); }
    finally { setSaving(false); }
  };
  const del = async () => {
    if (!item || !confirm("حذف قالب الصفحة؟")) return;
    try { await deletePageTemplate(item.id); toast.success("تم الحذف"); onSaved(); }
    catch (e) { toast.error(e instanceof ApiError ? e.message : "تعذر الحذف"); }
  };

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <button onClick={onClose} className="inline-flex h-10 items-center gap-2 border border-border px-4 text-sm hover:bg-muted"><ArrowRight className="h-4 w-4" /> قوالب الصفحات</button>
        <div className="flex items-center gap-2">
          {item?.isSystem && <span className="inline-flex items-center gap-1 border border-amber-500/30 bg-amber-500/10 px-2 py-1 text-xs text-amber-400"><Lock className="h-3 w-3" /> نظام</span>}
          {item && !locked && <button onClick={del} className="inline-flex h-10 items-center gap-2 border border-destructive/40 px-3 text-sm text-destructive hover:bg-destructive/10"><Trash2 className="h-4 w-4" /> حذف</button>}
          {!locked && <button onClick={save} disabled={saving} className="inline-flex h-10 items-center gap-2 bg-primary px-5 text-sm font-bold uppercase tracking-wider text-primary-foreground hover:bg-primary/80 disabled:opacity-50">{saving ? <Loader2 className="h-4 w-4 animate-spin" /> : <Save className="h-4 w-4" />} حفظ</button>}
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <div className="space-y-4">
          <Card title="الهوية">
            {!item && <Field label="الرمز"><input value={code} onChange={(e) => setCode(e.target.value)} dir="ltr" disabled={locked} className={inp} /></Field>}
            <Field label="الاسم (عربي)"><input value={nameAr} onChange={(e) => setNameAr(e.target.value)} disabled={locked} className={inp} /></Field>
            <Field label="الاسم (إنجليزي)"><input value={nameEn} onChange={(e) => setNameEn(e.target.value)} dir="ltr" disabled={locked} className={inp} /></Field>
          </Card>

          <Card title="الترويسة">
            <Check label="إظهار الشعار" v={!!header.showLogo} onChange={(v) => setHeader({ ...header, showLogo: v })} disabled={locked} />
            <Field label="موضع الشعار"><select value={header.logoPlacement} onChange={(e) => setHeader({ ...header, logoPlacement: e.target.value as HeaderConfig["logoPlacement"] })} disabled={locked} className={inp}><option value="Left">يسار</option><option value="Center">وسط</option><option value="Right">يمين</option></select></Field>
            <Check label="إظهار هوية الشركة" v={!!header.showIdentity} onChange={(v) => setHeader({ ...header, showIdentity: v })} disabled={locked} />
            <Check label="إظهار السجل/الضريبي" v={!!header.showCrVat} onChange={(v) => setHeader({ ...header, showCrVat: v })} disabled={locked} />
            <Check label="إظهار التواصل" v={!!header.showContact} onChange={(v) => setHeader({ ...header, showContact: v })} disabled={locked} />
          </Card>

          <Card title="التذييل">
            <Check label="إظهار رمز QR" v={!!footer.showQr} onChange={(v) => setFooter({ ...footer, showQr: v })} disabled={locked} />
            <Check label="إظهار الختم" v={!!footer.showStamp} onChange={(v) => setFooter({ ...footer, showStamp: v })} disabled={locked} />
            <Check label="إظهار التواقيع" v={!!footer.showSignatures} onChange={(v) => setFooter({ ...footer, showSignatures: v })} disabled={locked} />
            <Check label="إظهار تاريخ الإصدار" v={!!footer.showGeneratedDate} onChange={(v) => setFooter({ ...footer, showGeneratedDate: v })} disabled={locked} />
          </Card>

          <Card title="الهوامش (نقاط) والعلامة المائية">
            <Field label="أعلى"><input type="number" value={margins.top ?? 0} onChange={(e) => setMargins({ ...margins, top: Number(e.target.value) })} dir="ltr" disabled={locked} className={inp} /></Field>
            <Field label="أسفل"><input type="number" value={margins.bottom ?? 0} onChange={(e) => setMargins({ ...margins, bottom: Number(e.target.value) })} dir="ltr" disabled={locked} className={inp} /></Field>
            <Field label="يمين"><input type="number" value={margins.right ?? 0} onChange={(e) => setMargins({ ...margins, right: Number(e.target.value) })} dir="ltr" disabled={locked} className={inp} /></Field>
            <Field label="يسار"><input type="number" value={margins.left ?? 0} onChange={(e) => setMargins({ ...margins, left: Number(e.target.value) })} dir="ltr" disabled={locked} className={inp} /></Field>
            <Field label="نص العلامة المائية"><input value={watermark.text ?? ""} onChange={(e) => setWatermark({ text: e.target.value })} disabled={locked} className={inp} /></Field>
          </Card>
        </div>

        {/* A4 chrome preview */}
        <div>
          <p className="mb-1 text-xs font-bold uppercase tracking-wider text-muted-foreground">معاينة الإطار (A4)</p>
          <div className="mx-auto flex min-h-[520px] max-w-[460px] flex-col bg-white text-black shadow-lg" dir="rtl">
            <div className={`flex items-start gap-3 border-b border-zinc-200 p-4 ${header.logoPlacement === "Right" ? "flex-row-reverse" : header.logoPlacement === "Center" ? "flex-col items-center text-center" : ""}`}>
              {header.showLogo && <div className="flex h-10 w-16 items-center justify-center border border-dashed border-zinc-300 text-[9px] text-zinc-400">شعار</div>}
              <div className="text-xs">
                {header.showIdentity && <div className="font-bold">اسم الشركة</div>}
                {header.showCrVat && <div className="text-[9px] text-zinc-500">س.ت • ض.ق.م</div>}
                {header.showContact && <div className="text-[9px] text-zinc-500">هاتف • بريد • موقع</div>}
              </div>
            </div>
            <div className="relative flex-1 p-6 text-center text-sm text-zinc-400">
              {watermark.text && <div className="pointer-events-none absolute inset-0 flex items-center justify-center text-3xl font-bold text-zinc-100">{watermark.text}</div>}
              محتوى المستند
            </div>
            <div className="flex items-center justify-between border-t border-zinc-200 p-4 text-[9px] text-zinc-500">
              {footer.showQr ? <div className="flex h-10 w-10 items-center justify-center border border-zinc-300">QR</div> : <span />}
              {footer.showGeneratedDate && <span>تاريخ الإصدار</span>}
              {footer.showStamp ? <div className="flex h-10 w-10 items-center justify-center rounded-full border border-dashed border-zinc-300">ختم</div> : <span />}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

const inp = "h-9 w-full border border-border bg-secondary px-3 text-sm disabled:opacity-60";
function Card({ title, children }: { title: string; children: React.ReactNode }) {
  return <div className="border border-border bg-card p-4"><h2 className="mb-3 text-sm font-bold uppercase tracking-wider text-muted-foreground">{title}</h2><div className="grid grid-cols-1 gap-3 sm:grid-cols-2">{children}</div></div>;
}
function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return <div className="space-y-1"><label className="text-xs font-bold uppercase tracking-wider text-muted-foreground">{label}</label>{children}</div>;
}
function Check({ label, v, onChange, disabled }: { label: string; v: boolean; onChange: (v: boolean) => void; disabled?: boolean }) {
  return <label className="flex items-center gap-2 text-sm"><input type="checkbox" checked={v} onChange={(e) => onChange(e.target.checked)} disabled={disabled} /> {label}</label>;
}
