"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { ArrowRight, ChevronDown, Copy, FileText, LayoutTemplate, Loader2, Lock, Plus, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { usePermissions } from "@/lib/permissions";
import {
  addRequestTemplateMapping, deleteRequestTemplateMapping, DocumentTemplate, duplicateDocumentTemplate,
  getDocumentTemplates, getRequestTemplateMappings, getRequestTypeBindings, getTokenCatalog,
  RequestTemplateMapping, RequestTypeBinding, TokenGroup, TRIGGER_LABELS, TriggerEvent,
} from "@/lib/api/document-templates";
import { getPageTemplates, PageTemplate } from "@/lib/api/page-templates";
import { DocumentDesigner } from "@/components/documents/designer/DocumentDesigner";

export default function DocumentTemplatesPage() {
  const { has } = usePermissions();
  const canEdit = has("Platform.Documents.Edit") || has("Platform.Documents.Create");
  const [templates, setTemplates] = useState<DocumentTemplate[]>([]);
  const [pageTemplates, setPageTemplates] = useState<PageTemplate[]>([]);
  const [tokens, setTokens] = useState<TokenGroup[]>([]);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState<DocumentTemplate | "new" | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const [tpls, pages, toks] = await Promise.all([getDocumentTemplates(), getPageTemplates().catch(() => []), getTokenCatalog().catch(() => [])]);
      setTemplates(tpls); setPageTemplates(pages); setTokens(toks);
    } catch { toast.error("تعذر تحميل القوالب"); }
    finally { setLoading(false); }
  }, []);
  useEffect(() => { load(); }, [load]);

  if (editing) return (
    <DocumentDesigner
      template={editing === "new" ? null : editing}
      pageTemplates={pageTemplates}
      tokenGroups={tokens}
      canEdit={canEdit}
      onClose={() => setEditing(null)}
      onSaved={() => { setEditing(null); load(); }}
    />
  );

  const library = templates.filter((t) => t.isSystem);
  const custom = templates.filter((t) => !t.isSystem);

  const dup = async (t: DocumentTemplate) => {
    try { const copy = await duplicateDocumentTemplate(t.id); toast.success("تم إنشاء نسخة قابلة للتعديل"); await load(); setEditing(copy); }
    catch { toast.error("تعذر النسخ"); }
  };

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold">قوالب المستندات</h1>
          <p className="mt-1 text-sm text-muted-foreground">مصمّم مرئي بالسحب والإفلات — صمّم مستندات رسمية دون الحاجة لأي معرفة برمجية</p>
        </div>
        <div className="flex items-center gap-2">
          {canEdit && <button onClick={() => setEditing("new")} className="inline-flex h-10 items-center gap-2 bg-primary px-4 text-sm font-bold uppercase tracking-wider text-primary-foreground hover:bg-primary/80"><Plus className="h-4 w-4" /> قالب جديد</button>}
          <Link href="/settings/page-templates" className="inline-flex h-10 items-center gap-2 border border-border px-4 text-sm hover:bg-muted"><LayoutTemplate className="h-4 w-4" /> قوالب الصفحات</Link>
          <Link href="/settings" className="inline-flex h-10 items-center gap-2 border border-border px-4 text-sm hover:bg-muted"><ArrowRight className="h-4 w-4" /> الإعدادات</Link>
        </div>
      </div>

      {loading ? <div className="flex h-64 items-center justify-center text-muted-foreground"><Loader2 className="h-6 w-6 animate-spin" /></div> : (
        <>
          {custom.length > 0 && (
            <Section title="قوالبي">
              {custom.map((t) => <TemplateRow key={t.id} t={t} onOpen={() => setEditing(t)} onDuplicate={() => dup(t)} canEdit={canEdit} />)}
            </Section>
          )}
          <Section title="مكتبة القوالب الجاهزة" subtitle="قوالب نظام جاهزة — انسخها لتعديلها (لا يمكن حذف الأصل)">
            {library.map((t) => <TemplateRow key={t.id} t={t} onOpen={() => setEditing(t)} onDuplicate={() => dup(t)} canEdit={canEdit} />)}
            {library.length === 0 && <Empty>لا توجد قوالب — شغّل تهيئة الطلبات لإنشاء المكتبة</Empty>}
          </Section>

          {canEdit && <MappingSection publishedTemplates={templates.filter((t) => t.status === "Published")} />}
        </>
      )}
    </div>
  );
}

function Section({ title, subtitle, children }: { title: string; subtitle?: string; children: React.ReactNode }) {
  return (
    <div className="space-y-2">
      <div><h2 className="text-lg font-bold">{title}</h2>{subtitle && <p className="text-sm text-muted-foreground">{subtitle}</p>}</div>
      <div className="space-y-2">{children}</div>
    </div>
  );
}
function Empty({ children }: { children: React.ReactNode }) {
  return <div className="border border-dashed border-border py-12 text-center text-sm text-muted-foreground">{children}</div>;
}

function TemplateRow({ t, onOpen, onDuplicate, canEdit }: { t: DocumentTemplate; onOpen: () => void; onDuplicate: () => void; canEdit: boolean }) {
  return (
    <div className="flex items-center justify-between border border-border bg-card px-4 py-3 hover:bg-muted/40">
      <button onClick={onOpen} className="flex items-center gap-3 text-right">
        <FileText className="h-5 w-5 text-primary" />
        <div>
          <div className="flex items-center gap-2 font-medium">{t.nameAr}{t.isSystem && <Lock className="h-3.5 w-3.5 text-amber-400" />}</div>
          <div className="font-mono text-xs text-muted-foreground">{t.code}</div>
        </div>
      </button>
      <div className="flex items-center gap-2">
        <span className={`border px-2 py-1 text-xs ${t.status === "Published" ? "border-green-500/30 bg-green-500/10 text-green-400" : "border-border text-muted-foreground"}`}>{t.status === "Published" ? "منشور" : "مسودة"}</span>
        {canEdit && <button onClick={onDuplicate} title="نسخ" className="inline-flex h-8 items-center gap-1 border border-border px-2 text-xs hover:bg-muted"><Copy className="h-3.5 w-3.5" /> نسخ</button>}
      </div>
    </div>
  );
}

// ── Request → template mapping manager ──
function MappingSection({ publishedTemplates }: { publishedTemplates: DocumentTemplate[] }) {
  const [types, setTypes] = useState<RequestTypeBinding[]>([]);
  const [openId, setOpenId] = useState<string | null>(null);

  useEffect(() => { getRequestTypeBindings().then(setTypes).catch(() => {}); }, []);
  if (types.length === 0) return null;

  return (
    <div className="space-y-2 border-t border-border pt-5">
      <div>
        <h2 className="text-lg font-bold">ربط القوالب بأنواع الطلبات</h2>
        <p className="text-sm text-muted-foreground">عيّن قالباً (أو أكثر) لكل نوع طلب، وحدد متى يُنشأ المستند: عند التقديم، الموافقة الأولى، الموافقة النهائية، الرفض، أو الاكتمال</p>
      </div>
      <div className="divide-y divide-border border border-border bg-card">
        {types.map((rt) => (
          <div key={rt.id}>
            <button onClick={() => setOpenId(openId === rt.id ? null : rt.id)} className="flex w-full items-center justify-between px-4 py-3 text-right hover:bg-muted/40">
              <div className="flex items-center gap-3"><FileText className="h-4 w-4 text-muted-foreground" /><div><div className="font-medium">{rt.nameAr}</div><div className="font-mono text-xs text-muted-foreground">{rt.code}</div></div></div>
              <ChevronDown className={`h-4 w-4 transition-transform ${openId === rt.id ? "rotate-180" : ""}`} />
            </button>
            {openId === rt.id && <MappingEditor typeId={rt.id} publishedTemplates={publishedTemplates} />}
          </div>
        ))}
      </div>
    </div>
  );
}

function MappingEditor({ typeId, publishedTemplates }: { typeId: string; publishedTemplates: DocumentTemplate[] }) {
  const [rows, setRows] = useState<RequestTemplateMapping[]>([]);
  const [loading, setLoading] = useState(true);
  const [tplId, setTplId] = useState("");
  const [trigger, setTrigger] = useState<TriggerEvent>("FinalApproval");
  const [adding, setAdding] = useState(false);

  const load = useCallback(() => {
    setLoading(true);
    getRequestTemplateMappings(typeId).then(setRows).catch(() => {}).finally(() => setLoading(false));
  }, [typeId]);
  useEffect(() => { load(); }, [load]);

  const add = async () => {
    if (!tplId) { toast.error("اختر قالباً"); return; }
    setAdding(true);
    try { await addRequestTemplateMapping(typeId, tplId, trigger); setTplId(""); toast.success("تم الربط"); load(); }
    catch { toast.error("تعذر الربط (قد يكون مرتبطاً مسبقاً)"); }
    finally { setAdding(false); }
  };
  const remove = async (m: RequestTemplateMapping) => {
    try { await deleteRequestTemplateMapping(m.id); toast.success("تم الحذف"); load(); }
    catch { toast.error("تعذر الحذف"); }
  };

  return (
    <div className="space-y-3 bg-secondary/30 px-4 py-3">
      {loading ? <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" /> : (
        <div className="space-y-1">
          {rows.length === 0 && <p className="text-xs text-muted-foreground">لا توجد قوالب مرتبطة</p>}
          {rows.map((m) => (
            <div key={m.id} className="flex items-center justify-between border border-border bg-card px-3 py-2 text-sm">
              <div className="flex items-center gap-2">
                <span className="font-medium">{m.templateNameAr ?? "—"}</span>
                <span className="border border-border px-1.5 py-0.5 text-xs text-muted-foreground">{TRIGGER_LABELS[m.triggerEvent] ?? m.triggerEvent}</span>
                {m.isSystem && <span className="inline-flex items-center gap-1 text-xs text-amber-400"><Lock className="h-3 w-3" /> افتراضي</span>}
              </div>
              {!m.isSystem && <button onClick={() => remove(m)} className="text-red-400 hover:text-red-600"><Trash2 className="h-4 w-4" /></button>}
            </div>
          ))}
        </div>
      )}
      <div className="flex flex-wrap items-center gap-2">
        <select value={tplId} onChange={(e) => setTplId(e.target.value)} className="h-9 min-w-48 border border-border bg-secondary px-3 text-sm">
          <option value="">— اختر قالباً منشوراً —</option>
          {publishedTemplates.map((t) => <option key={t.id} value={t.id}>{t.nameAr}</option>)}
        </select>
        <select value={trigger} onChange={(e) => setTrigger(e.target.value as TriggerEvent)} className="h-9 border border-border bg-secondary px-3 text-sm">
          {(Object.keys(TRIGGER_LABELS) as TriggerEvent[]).map((k) => <option key={k} value={k}>{TRIGGER_LABELS[k]}</option>)}
        </select>
        <button onClick={add} disabled={adding} className="inline-flex h-9 items-center gap-1 bg-primary px-3 text-sm font-bold text-primary-foreground hover:bg-primary/80 disabled:opacity-50">{adding ? <Loader2 className="h-4 w-4 animate-spin" /> : <Plus className="h-4 w-4" />} ربط</button>
      </div>
    </div>
  );
}
