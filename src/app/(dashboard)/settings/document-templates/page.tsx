"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import Link from "next/link";
import { ArrowRight, Check, FileText, Loader2, Plus, Save, Send, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { ApiError } from "@/lib/api-client";
import { usePermissions } from "@/lib/permissions";
import {
  createDocumentTemplate, deleteDocumentTemplate, DocumentTemplate, getDocumentTemplate,
  getDocumentTemplates, getRequestTypeBindings, getTokenCatalog, previewTemplateHtml,
  publishDocumentTemplate, RequestTypeBinding, setRequestTypePrintTemplate,
  TokenGroup, updateDocumentTemplate,
} from "@/lib/api/document-templates";

export default function DocumentTemplatesPage() {
  const { has } = usePermissions();
  const canEdit = has("Platform.Documents.Edit") || has("Platform.Documents.Create");
  const [templates, setTemplates] = useState<DocumentTemplate[]>([]);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState<DocumentTemplate | "new" | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    try { setTemplates(await getDocumentTemplates()); }
    catch { toast.error("تعذر تحميل القوالب"); }
    finally { setLoading(false); }
  }, []);
  useEffect(() => { load(); }, [load]);

  if (editing) return <Editor template={editing === "new" ? null : editing} canEdit={canEdit} onClose={() => setEditing(null)} onSaved={() => { setEditing(null); load(); }} />;

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold">قوالب المستندات</h1>
          <p className="mt-1 text-sm text-muted-foreground">صمّم مستندات رسمية بالنصوص والرموز — بدون مبرمجين</p>
        </div>
        <div className="flex items-center gap-2">
          {canEdit && <button onClick={() => setEditing("new")} className="inline-flex h-10 items-center gap-2 bg-primary px-4 text-sm font-bold uppercase tracking-wider text-primary-foreground hover:bg-primary/80"><Plus className="h-4 w-4" /> قالب جديد</button>}
          <Link href="/settings" className="inline-flex h-10 items-center gap-2 border border-border px-4 text-sm hover:bg-muted"><ArrowRight className="h-4 w-4" /> الإعدادات</Link>
        </div>
      </div>

      {loading ? <div className="flex h-64 items-center justify-center text-muted-foreground"><Loader2 className="h-6 w-6 animate-spin" /></div> : (
        <div className="space-y-2">
          {templates.map((t) => (
            <button key={t.id} onClick={() => setEditing(t)} className="flex w-full items-center justify-between border border-border bg-card px-4 py-3 text-right hover:bg-muted/40">
              <div className="flex items-center gap-3">
                <FileText className="h-5 w-5 text-primary" />
                <div><div className="font-medium">{t.nameAr}</div><div className="font-mono text-xs text-muted-foreground">{t.code}</div></div>
              </div>
              <span className={`border px-2 py-1 text-xs ${t.status === "Published" ? "border-green-500/30 bg-green-500/10 text-green-400" : "border-border text-muted-foreground"}`}>{t.status === "Published" ? "منشور" : "مسودة"}</span>
            </button>
          ))}
          {templates.length === 0 && <div className="border border-dashed border-border py-16 text-center text-muted-foreground">لا توجد قوالب</div>}
        </div>
      )}

      {!loading && <Bindings templates={templates} canEdit={canEdit} />}
    </div>
  );
}

function Bindings({ templates, canEdit }: { templates: DocumentTemplate[]; canEdit: boolean }) {
  const [rows, setRows] = useState<RequestTypeBinding[]>([]);
  const [loading, setLoading] = useState(true);
  const [savingId, setSavingId] = useState<string | null>(null);
  const published = templates.filter((t) => t.status === "Published");

  const load = useCallback(async () => {
    setLoading(true);
    try { setRows(await getRequestTypeBindings()); }
    catch { /* admin-only; ignore if forbidden */ }
    finally { setLoading(false); }
  }, []);
  useEffect(() => { load(); }, [load]);

  const change = async (id: string, templateId: string) => {
    setSavingId(id);
    try {
      await setRequestTypePrintTemplate(id, templateId || null);
      setRows((rs) => rs.map((r) => (r.id === id ? { ...r, printTemplateId: templateId || null } : r)));
      toast.success("تم ربط القالب");
    } catch { toast.error("تعذر الربط"); }
    finally { setSavingId(null); }
  };

  if (loading) return null;
  if (rows.length === 0) return null;

  return (
    <div className="space-y-2 border-t border-border pt-5">
      <div>
        <h2 className="text-lg font-bold">ربط القوالب بأنواع الطلبات</h2>
        <p className="mt-1 text-sm text-muted-foreground">اختر القالب الرسمي الذي يُطبع لكل نوع طلب عند الموافقة — تظهر القوالب المنشورة فقط</p>
      </div>
      <div className="divide-y divide-border border border-border bg-card">
        {rows.map((r) => (
          <div key={r.id} className="flex items-center justify-between gap-3 px-4 py-3">
            <div className="flex items-center gap-3">
              <FileText className="h-4 w-4 text-muted-foreground" />
              <div><div className="font-medium">{r.nameAr}</div><div className="font-mono text-xs text-muted-foreground">{r.code}</div></div>
            </div>
            <div className="flex items-center gap-2">
              {savingId === r.id && <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />}
              <select
                value={r.printTemplateId ?? ""}
                disabled={!canEdit || savingId === r.id}
                onChange={(e) => change(r.id, e.target.value)}
                className="h-9 min-w-56 border border-border bg-secondary px-3 text-sm disabled:opacity-50"
              >
                <option value="">— بدون قالب —</option>
                {published.map((t) => <option key={t.id} value={t.id}>{t.nameAr}</option>)}
              </select>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

function Editor({ template, canEdit, onClose, onSaved }: { template: DocumentTemplate | null; canEdit: boolean; onClose: () => void; onSaved: () => void }) {
  const [code, setCode] = useState(template?.code ?? "");
  const [nameAr, setNameAr] = useState(template?.nameAr ?? "");
  const [nameEn, setNameEn] = useState(template?.nameEn ?? "");
  const [body, setBody] = useState(template?.bodyTemplate ?? "<h2>{{Company.Name}}</h2>\n<p>التاريخ: {{System.Today}}</p>\n<hr/>\n<p>نشهد بأن الموظف <b>{{Employee.FullName}}</b> (رقم {{Employee.EmployeeNumber}}) ...</p>");
  const [tokens, setTokens] = useState<TokenGroup[]>([]);
  const [preview, setPreview] = useState("");
  const [saving, setSaving] = useState(false);
  const [full, setFull] = useState<DocumentTemplate | null>(template);
  const bodyRef = useRef<HTMLTextAreaElement>(null);

  useEffect(() => { getTokenCatalog().then(setTokens).catch(() => {}); }, []);
  useEffect(() => { if (template) getDocumentTemplate(template.id).then((d) => { setFull(d); setBody(d.bodyTemplate); }).catch(() => {}); }, [template]);

  // Debounced live preview
  useEffect(() => {
    const t = setTimeout(() => { previewTemplateHtml(body).then(setPreview).catch(() => setPreview(body)); }, 350);
    return () => clearTimeout(t);
  }, [body]);

  const insert = (token: string) => {
    const el = bodyRef.current;
    const pos = el?.selectionStart ?? body.length;
    setBody(body.slice(0, pos) + token + body.slice(pos));
    setTimeout(() => { el?.focus(); el?.setSelectionRange(pos + token.length, pos + token.length); }, 0);
  };

  const save = async () => {
    if (!nameAr.trim() || !nameEn.trim() || (!template && !code.trim())) { toast.error("أكمل الحقول المطلوبة"); return; }
    setSaving(true);
    try {
      const saved = template ? await updateDocumentTemplate(template.id, { nameAr, nameEn, bodyTemplate: body })
        : await createDocumentTemplate({ code: code.trim().toUpperCase(), nameAr, nameEn, bodyTemplate: body });
      setFull(saved);
      toast.success("تم الحفظ");
      if (!template) onSaved();
    } catch (e) { toast.error(e instanceof ApiError ? e.message : "تعذر الحفظ"); }
    finally { setSaving(false); }
  };

  const publish = async () => {
    if (!full) return;
    try { await publishDocumentTemplate(full.id); toast.success("تم النشر"); onSaved(); }
    catch { toast.error("تعذر النشر"); }
  };

  const del = async () => {
    if (!full || !confirm("حذف القالب؟")) return;
    try { await deleteDocumentTemplate(full.id); toast.success("تم الحذف"); onSaved(); }
    catch { toast.error("تعذر الحذف"); }
  };

  const input = "h-9 w-full border border-border bg-secondary px-3 text-sm";
  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <button onClick={onClose} className="inline-flex h-10 items-center gap-2 border border-border px-4 text-sm hover:bg-muted"><ArrowRight className="h-4 w-4" /> القوالب</button>
        {canEdit && (
          <div className="flex items-center gap-2">
            {full && <button onClick={del} className="inline-flex h-10 items-center gap-2 border border-destructive/40 px-3 text-sm text-destructive hover:bg-destructive/10"><Trash2 className="h-4 w-4" /> حذف</button>}
            {full && <button onClick={publish} className="inline-flex h-10 items-center gap-2 border border-green-500/40 px-3 text-sm text-green-400 hover:bg-green-500/10"><Send className="h-4 w-4" /> نشر</button>}
            <button onClick={save} disabled={saving} className="inline-flex h-10 items-center gap-2 bg-primary px-5 text-sm font-bold uppercase tracking-wider text-primary-foreground hover:bg-primary/80 disabled:opacity-50">{saving ? <Loader2 className="h-4 w-4 animate-spin" /> : <Save className="h-4 w-4" />} حفظ</button>
          </div>
        )}
      </div>

      <div className="grid grid-cols-2 gap-3">
        {!template && <div className="space-y-1"><label className="text-xs font-bold uppercase tracking-wider text-muted-foreground">الرمز</label><input value={code} onChange={(e) => setCode(e.target.value)} className={input} dir="ltr" /></div>}
        <div className="space-y-1"><label className="text-xs font-bold uppercase tracking-wider text-muted-foreground">الاسم (عربي)</label><input value={nameAr} onChange={(e) => setNameAr(e.target.value)} className={input} /></div>
        <div className="space-y-1"><label className="text-xs font-bold uppercase tracking-wider text-muted-foreground">الاسم (إنجليزي)</label><input value={nameEn} onChange={(e) => setNameEn(e.target.value)} className={input} dir="ltr" /></div>
      </div>

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        {/* Editor + token explorer */}
        <div className="space-y-3">
          <div>
            <p className="mb-1 text-xs font-bold uppercase tracking-wider text-muted-foreground">محتوى القالب (HTML + رموز)</p>
            <textarea ref={bodyRef} value={body} onChange={(e) => setBody(e.target.value)} rows={16} className="w-full border border-border bg-secondary px-3 py-2 font-mono text-sm" dir="ltr" />
          </div>
          <div className="border border-border bg-card p-3">
            <p className="mb-2 text-xs font-bold uppercase tracking-wider text-muted-foreground">مستكشف الرموز — انقر للإدراج</p>
            <div className="space-y-2">
              {tokens.map((g) => (
                <div key={g.group}>
                  <p className="text-xs text-muted-foreground">{g.group}</p>
                  <div className="mt-1 flex flex-wrap gap-1">
                    {g.tokens.map((t) => (
                      <button key={t.token} onClick={() => insert(t.token)} title={t.token} className="border border-border bg-secondary px-2 py-1 text-xs hover:border-primary/60">{t.label}</button>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Live preview */}
        <div className="space-y-1">
          <p className="text-xs font-bold uppercase tracking-wider text-muted-foreground">معاينة مباشرة (بيانات تجريبية)</p>
          <div className="min-h-[28rem] border border-border bg-white p-6 text-black" dir="rtl" dangerouslySetInnerHTML={{ __html: preview || body }} />
        </div>
      </div>
    </div>
  );
}
