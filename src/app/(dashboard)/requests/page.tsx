"use client";

import { useEffect, useMemo, useState } from "react";
import { Loader2, Inbox, Send } from "lucide-react";
import { toast } from "sonner";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog";
import { ApiError } from "@/lib/api-client";
import { getLookup, lookupLabel, LookupItem } from "@/lib/api/lookups";
import { getFormDefinition, submitForm, FormDefinition, FormField, FormSubmissionValueInput } from "@/lib/api/forms";
import { startWorkflow } from "@/lib/api/workflows";
import { requestIcon } from "@/lib/request-icons";

function notifyError(err: unknown, fallback: string) {
  if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
    toast.error(err instanceof ApiError ? err.message : fallback);
  }
}

interface ReqMeta {
  categoryId?: string;
  formDefinitionId?: string;
  workflowCode?: string;
  slaHours?: number | null;
}

function readMeta(item: LookupItem): ReqMeta {
  return (item.metadata ?? {}) as ReqMeta;
}

// Map a backend FieldType string to an input renderer hint.
function inputType(fieldType: string): string {
  switch (fieldType) {
    case "Number": case "Decimal": case "Currency": case "Percentage": return "number";
    case "Date": return "date";
    case "DateTime": return "datetime-local";
    case "Email": return "email";
    case "Phone": return "tel";
    case "Url": return "url";
    default: return "text";
  }
}

function parseOptions(raw?: string | null): { value: string; label: string }[] {
  if (!raw) return [];
  try {
    const parsed = JSON.parse(raw);
    if (Array.isArray(parsed)) {
      return parsed.map((o) =>
        typeof o === "string" ? { value: o, label: o } : { value: String(o.value ?? o.code ?? ""), label: String(o.label ?? o.nameAr ?? o.value ?? "") }
      );
    }
  } catch { /* not JSON — ignore */ }
  return [];
}

export default function RequestsPage() {
  const [types, setTypes] = useState<LookupItem[]>([]);
  const [categories, setCategories] = useState<LookupItem[]>([]);
  const [loading, setLoading] = useState(true);

  const [active, setActive] = useState<LookupItem | null>(null);
  const [formDef, setFormDef] = useState<FormDefinition | null>(null);
  const [loadingForm, setLoadingForm] = useState(false);
  const [values, setValues] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    (async () => {
      setLoading(true);
      try {
        const [ts, cats] = await Promise.all([getLookup("request-types"), getLookup("request-categories")]);
        setTypes(ts); setCategories(cats);
      } catch (err) { notifyError(err, "تعذر تحميل أنواع الطلبات"); }
      finally { setLoading(false); }
    })();
  }, []);

  const catName = useMemo(() => {
    const m = new Map(categories.map((c) => [c.id, lookupLabel(c)]));
    return (id?: string) => (id ? m.get(id) ?? null : null);
  }, [categories]);

  async function openType(item: LookupItem) {
    setActive(item);
    setFormDef(null);
    setValues({});
    const meta = readMeta(item);
    if (!meta.formDefinitionId) return;
    setLoadingForm(true);
    try {
      const def = await getFormDefinition(meta.formDefinitionId);
      setFormDef(def);
      const init: Record<string, string> = {};
      def.fields.forEach((f) => { init[f.id] = f.defaultValue ?? ""; });
      setValues(init);
    } catch (err) { notifyError(err, "تعذر تحميل النموذج"); }
    finally { setLoadingForm(false); }
  }

  function closeDialog() { if (!submitting) { setActive(null); setFormDef(null); setValues({}); } }

  async function submit() {
    if (!active) return;
    const meta = readMeta(active);

    if (formDef) {
      const missing = formDef.fields.filter((f) => f.isRequired && !values[f.id]?.trim());
      if (missing.length) { toast.error(`حقول مطلوبة: ${missing.map((f) => f.nameAr).join("، ")}`); return; }
    }

    setSubmitting(true);
    try {
      let submissionId: string | null = null;
      if (formDef) {
        const payload: FormSubmissionValueInput[] = formDef.fields.map((f) => ({
          formFieldId: f.id, fieldCode: f.code, value: values[f.id] ?? "",
        }));
        const submission = await submitForm(formDef.id, payload);
        submissionId = submission.id;
      }

      // Kick off the approval workflow when one is linked (best-effort; needs permission).
      if (meta.workflowCode && submissionId) {
        try { await startWorkflow(meta.workflowCode, "FormSubmission", submissionId); }
        catch { /* workflow start is non-blocking for the submitter */ }
      }

      toast.success("تم إرسال الطلب بنجاح");
      closeDialog();
    } catch (err) { notifyError(err, "تعذر إرسال الطلب"); }
    finally { setSubmitting(false); }
  }

  function renderField(f: FormField) {
    const val = values[f.id] ?? "";
    const set = (v: string) => setValues((s) => ({ ...s, [f.id]: v }));
    const opts = parseOptions(f.options);
    const selectClass = "w-full h-9 bg-secondary border border-border px-3 text-sm text-foreground";

    if (f.fieldType === "TextArea") {
      return <textarea value={val} onChange={(e) => set(e.target.value)} placeholder={f.placeholder ?? ""} className="w-full bg-secondary border border-border px-3 py-2 text-sm text-foreground min-h-[80px]" />;
    }
    if (f.fieldType === "Boolean") {
      return <label className="flex items-center gap-2 text-sm cursor-pointer"><input type="checkbox" checked={val === "true"} onChange={(e) => set(e.target.checked ? "true" : "false")} /> نعم</label>;
    }
    if (f.fieldType === "Dropdown") {
      return (
        <select value={val} onChange={(e) => set(e.target.value)} className={selectClass}>
          <option value="">— اختر —</option>
          {opts.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
        </select>
      );
    }
    return <Input type={inputType(f.fieldType)} value={val} onChange={(e) => set(e.target.value)} placeholder={f.placeholder ?? ""} className="bg-secondary border-border" />;
  }

  const activeMeta = active ? readMeta(active) : null;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">الطلبات</h1>
        <p className="text-sm text-muted-foreground mt-1">اختر نوع الطلب لتقديمه</p>
      </div>

      {loading ? (
        <div className="py-16 text-center text-sm text-muted-foreground"><Loader2 className="h-5 w-5 animate-spin inline" /> جاري التحميل...</div>
      ) : types.length === 0 ? (
        <div className="border border-border bg-card p-12 flex flex-col items-center justify-center text-center">
          <Inbox className="h-12 w-12 text-muted-foreground mb-4" />
          <h2 className="text-lg font-semibold mb-2">لا توجد أنواع طلبات</h2>
          <p className="text-sm text-muted-foreground">يقوم المسؤول بتعريف أنواع الطلبات من الإعدادات → إعدادات الطلبات</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {types.map((t) => {
            const Icon = requestIcon(t.icon);
            const color = t.color ?? "#3b82f6";
            const cat = catName(readMeta(t).categoryId);
            return (
              <button key={t.id} onClick={() => openType(t)} className="group text-right border border-border bg-card p-5 hover:border-primary/50 hover:bg-card/70 transition-colors">
                <div className="flex items-start justify-between">
                  <span className="flex h-10 w-10 items-center justify-center shrink-0" style={{ backgroundColor: `${color}1a`, color }}><Icon className="h-5 w-5" /></span>
                  {cat && <Badge variant="outline" className="text-[10px] text-muted-foreground border-border">{cat}</Badge>}
                </div>
                <h2 className="text-base font-bold mt-4">{lookupLabel(t)}</h2>
                {t.description && <p className="text-xs text-muted-foreground mt-1 leading-relaxed line-clamp-2">{t.description}</p>}
              </button>
            );
          })}
        </div>
      )}

      <Dialog open={!!active} onOpenChange={(o) => { if (!o) closeDialog(); }}>
        <DialogContent className="sm:max-w-xl">
          <DialogHeader>
            <DialogTitle>{active ? lookupLabel(active) : ""}</DialogTitle>
            {active?.description && <DialogDescription>{active.description}</DialogDescription>}
          </DialogHeader>

          <div className="space-y-4 py-2 max-h-[65vh] overflow-y-auto pl-1">
            {loadingForm ? (
              <div className="py-8 text-center text-sm text-muted-foreground"><Loader2 className="h-4 w-4 animate-spin inline" /> جاري تحميل النموذج...</div>
            ) : !activeMeta?.formDefinitionId ? (
              <p className="text-sm text-muted-foreground py-4">لا يوجد نموذج مرتبط بهذا الطلب بعد. تواصل مع المسؤول لإكمال إعداده.</p>
            ) : formDef ? (
              formDef.fields.length === 0 ? (
                <p className="text-sm text-muted-foreground py-4">النموذج لا يحتوي على حقول.</p>
              ) : (
                formDef.fields.map((f) => (
                  <div key={f.id} className="space-y-2">
                    <Label className="text-xs font-bold uppercase tracking-wider">
                      {f.nameAr}{f.isRequired && <span className="text-destructive"> *</span>}
                    </Label>
                    {renderField(f)}
                  </div>
                ))
              )
            ) : null}
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={closeDialog} disabled={submitting}>إلغاء</Button>
            <Button onClick={submit} disabled={submitting || !activeMeta?.formDefinitionId} className="font-bold gap-2">
              {submitting ? <Loader2 className="h-4 w-4 animate-spin" /> : <Send className="h-4 w-4" />}
              {submitting ? "جاري الإرسال..." : "إرسال الطلب"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
