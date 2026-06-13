"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import {
  DndContext, closestCenter, PointerSensor, useSensor, useSensors, type DragEndEvent,
} from "@dnd-kit/core";
import {
  SortableContext, verticalListSortingStrategy, useSortable, arrayMove,
} from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import {
  ArrowRight, Braces, Copy, GripVertical, Heading, Image as ImageIcon, Loader2, Lock, Minus,
  MoveVertical, PenLine, Plus, QrCode, Save, Search, Send, Settings2, Stamp as StampIcon,
  Table as TableIcon, Trash2, Type, Upload,
} from "lucide-react";
import { toast } from "sonner";
import { ApiError } from "@/lib/api-client";
import { fileUrl, uploadFile } from "@/lib/api/files";
import {
  createDocumentTemplate, deleteDocumentTemplate, DocumentTemplate, duplicateDocumentTemplate,
  getDocumentTemplate, publishDocumentTemplate, TokenGroup, updateDocumentTemplate,
} from "@/lib/api/document-templates";
import { PageTemplate } from "@/lib/api/page-templates";
import { BLOCK_DEFS, BlockType, DocBlock, newBlock, parseLayout, serializeLayout } from "./types";

const ICONS: Record<string, typeof Type> = {
  Heading, Type, Braces, Table: TableIcon, Image: ImageIcon, QrCode, PenLine, Stamp: StampIcon, Minus, MoveVertical,
};

export function DocumentDesigner({
  template, pageTemplates, tokenGroups, canEdit, onClose, onSaved,
}: {
  template: DocumentTemplate | null;
  pageTemplates: PageTemplate[];
  tokenGroups: TokenGroup[];
  canEdit: boolean;
  onClose: () => void;
  onSaved: () => void;
}) {
  const locked = !canEdit || (template?.isSystem ?? false);
  const [full, setFull] = useState<DocumentTemplate | null>(template);
  const [code, setCode] = useState(template?.code ?? "");
  const [nameAr, setNameAr] = useState(template?.nameAr ?? "");
  const [nameEn, setNameEn] = useState(template?.nameEn ?? "");
  const [pageTemplateId, setPageTemplateId] = useState<string | null>(template?.pageTemplateId ?? null);
  const [blocks, setBlocks] = useState<DocBlock[]>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [tab, setTab] = useState<"tokens" | "props" | "page">("page");
  const [tokenSearch, setTokenSearch] = useState("");
  const [saving, setSaving] = useState(false);

  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 5 } }));
  const selected = blocks.find((b) => b.id === selectedId) ?? null;

  // Load full template (with layoutJson) for editing.
  useEffect(() => {
    if (!template) {
      setBlocks([newBlock("title")]);
      setPageTemplateId(pageTemplates[0]?.id ?? null);
      return;
    }
    getDocumentTemplate(template.id).then((d) => {
      setFull(d);
      setNameAr(d.nameAr); setNameEn(d.nameEn);
      setPageTemplateId(d.pageTemplateId ?? null);
      setBlocks(parseLayout(d.layoutJson));
    }).catch(() => {});
  }, [template, pageTemplates]);

  const addBlock = (type: BlockType) => {
    if (locked) return;
    const nb = newBlock(type);
    setBlocks((b) => [...b, nb]);
    setSelectedId(nb.id);
    setTab("props");
  };
  const update = (id: string, patch: Partial<DocBlock>) =>
    setBlocks((bs) => bs.map((b) => (b.id === id ? { ...b, ...patch } : b)));
  const remove = (id: string) => {
    setBlocks((bs) => bs.filter((b) => b.id !== id));
    if (selectedId === id) setSelectedId(null);
  };

  const onDragEnd = (e: DragEndEvent) => {
    const { active, over } = e;
    if (!over || active.id === over.id) return;
    setBlocks((bs) => {
      const oldIdx = bs.findIndex((b) => b.id === active.id);
      const newIdx = bs.findIndex((b) => b.id === over.id);
      return arrayMove(bs, oldIdx, newIdx);
    });
  };

  const insertToken = (token: string) => {
    if (locked) return;
    if (selected && (selected.type === "text" || selected.type === "title")) {
      update(selected.id, { text: `${selected.text ?? ""}${token}` });
    } else if (selected && selected.type === "token") {
      update(selected.id, { token });
    } else {
      const nb = newBlock("token");
      nb.token = token;
      setBlocks((b) => [...b, nb]);
      setSelectedId(nb.id);
    }
  };

  const save = useCallback(async () => {
    if (!nameAr.trim() || !nameEn.trim() || (!template && !code.trim())) { toast.error("أكمل الحقول المطلوبة (الرمز/الاسم)"); return; }
    setSaving(true);
    try {
      const layoutJson = blocks.length ? serializeLayout(blocks) : null;
      const payload = { nameAr, nameEn, layoutJson, bodyTemplate: full?.bodyTemplate ?? null, pageTemplateId };
      const saved = template
        ? await updateDocumentTemplate(template.id, payload)
        : await createDocumentTemplate({ code: code.trim().toUpperCase(), ...payload });
      setFull(saved);
      toast.success("تم الحفظ");
      if (!template) onSaved();
    } catch (e) { toast.error(e instanceof ApiError ? e.message : "تعذر الحفظ"); }
    finally { setSaving(false); }
  }, [blocks, code, full, nameAr, nameEn, onSaved, pageTemplateId, template]);

  const publish = async () => {
    if (!full) return;
    try { await publishDocumentTemplate(full.id); toast.success("تم النشر"); onSaved(); }
    catch { toast.error("تعذر النشر"); }
  };
  const del = async () => {
    if (!full || !confirm("حذف القالب؟")) return;
    try { await deleteDocumentTemplate(full.id); toast.success("تم الحذف"); onSaved(); }
    catch (e) { toast.error(e instanceof ApiError ? e.message : "تعذر الحذف"); }
  };
  const duplicate = async () => {
    if (!full) return;
    try { await duplicateDocumentTemplate(full.id); toast.success("تم إنشاء نسخة قابلة للتعديل"); onSaved(); }
    catch { toast.error("تعذر النسخ"); }
  };

  const pageTpl = pageTemplates.find((p) => p.id === pageTemplateId) ?? null;
  const filteredGroups = tokenGroups
    .map((g) => ({ ...g, tokens: g.tokens.filter((t) => !tokenSearch || t.label.includes(tokenSearch) || t.token.toLowerCase().includes(tokenSearch.toLowerCase())) }))
    .filter((g) => g.tokens.length > 0);

  return (
    <div className="space-y-4">
      {/* Toolbar */}
      <div className="flex flex-wrap items-center justify-between gap-3">
        <button onClick={onClose} className="inline-flex h-10 items-center gap-2 border border-border px-4 text-sm hover:bg-muted"><ArrowRight className="h-4 w-4" /> القوالب</button>
        <div className="flex items-center gap-2">
          {template?.isSystem && (
            <span className="inline-flex items-center gap-1 border border-amber-500/30 bg-amber-500/10 px-2 py-1 text-xs text-amber-400"><Lock className="h-3 w-3" /> قالب نظام</span>
          )}
          {full && <button onClick={duplicate} className="inline-flex h-10 items-center gap-2 border border-border px-3 text-sm hover:bg-muted"><Copy className="h-4 w-4" /> نسخ</button>}
          {full && !locked && <button onClick={del} className="inline-flex h-10 items-center gap-2 border border-destructive/40 px-3 text-sm text-destructive hover:bg-destructive/10"><Trash2 className="h-4 w-4" /> حذف</button>}
          {full && !locked && <button onClick={publish} className="inline-flex h-10 items-center gap-2 border border-green-500/40 px-3 text-sm text-green-400 hover:bg-green-500/10"><Send className="h-4 w-4" /> نشر</button>}
          {!locked && <button onClick={save} disabled={saving} className="inline-flex h-10 items-center gap-2 bg-primary px-5 text-sm font-bold uppercase tracking-wider text-primary-foreground hover:bg-primary/80 disabled:opacity-50">{saving ? <Loader2 className="h-4 w-4 animate-spin" /> : <Save className="h-4 w-4" />} حفظ</button>}
        </div>
      </div>

      <div className="grid grid-cols-1 gap-3 lg:grid-cols-[170px_1fr_320px]">
        {/* Left rail — block palette */}
        <div className="space-y-2">
          <p className="text-xs font-bold uppercase tracking-wider text-muted-foreground">العناصر</p>
          <div className="grid grid-cols-2 gap-2 lg:grid-cols-1">
            {BLOCK_DEFS.map((d) => {
              const Icon = ICONS[d.icon] ?? Type;
              return (
                <button key={d.type} onClick={() => addBlock(d.type)} disabled={locked}
                  className="flex items-center gap-2 border border-border bg-card px-3 py-2 text-right text-sm hover:border-primary/60 disabled:opacity-50">
                  <Icon className="h-4 w-4 text-primary" /> {d.label}
                </button>
              );
            })}
          </div>
        </div>

        {/* Center — A4 canvas */}
        <div className="overflow-auto">
          <div className="mx-auto w-full max-w-[794px] bg-white text-black shadow-lg" dir="rtl">
            <ChromeBand page={pageTpl} which="header" />
            <div className="min-h-[700px] px-12 py-8">
              <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={onDragEnd}>
                <SortableContext items={blocks.map((b) => b.id)} strategy={verticalListSortingStrategy}>
                  <div className="space-y-2">
                    {blocks.map((b) => (
                      <SortableBlock key={b.id} block={b} selected={b.id === selectedId} locked={locked}
                        onSelect={() => { setSelectedId(b.id); setTab("props"); }} onRemove={() => remove(b.id)} />
                    ))}
                  </div>
                </SortableContext>
              </DndContext>
              {blocks.length === 0 && <div className="py-20 text-center text-sm text-zinc-400">أضف عناصر من القائمة اليمنى لبناء المستند</div>}
            </div>
            <ChromeBand page={pageTpl} which="footer" />
          </div>
        </div>

        {/* Right rail — tabs */}
        <div className="space-y-3">
          <div className="flex border border-border bg-card text-sm">
            {([["page", "الصفحة"], ["props", "الخصائص"], ["tokens", "الرموز"]] as const).map(([k, l]) => (
              <button key={k} onClick={() => setTab(k)} className={`flex-1 px-2 py-2 ${tab === k ? "bg-primary text-primary-foreground font-bold" : "hover:bg-muted"}`}>{l}</button>
            ))}
          </div>

          {tab === "page" && (
            <div className="space-y-3 border border-border bg-card p-3">
              {!template && (
                <Field label="الرمز (Code)"><input value={code} onChange={(e) => setCode(e.target.value)} dir="ltr" disabled={locked} className={inputCls} /></Field>
              )}
              <Field label="الاسم (عربي)"><input value={nameAr} onChange={(e) => setNameAr(e.target.value)} disabled={locked} className={inputCls} /></Field>
              <Field label="الاسم (إنجليزي)"><input value={nameEn} onChange={(e) => setNameEn(e.target.value)} dir="ltr" disabled={locked} className={inputCls} /></Field>
              <Field label="قالب الصفحة (الترويسة/التذييل)">
                <select value={pageTemplateId ?? ""} onChange={(e) => setPageTemplateId(e.target.value || null)} disabled={locked} className={inputCls}>
                  <option value="">— افتراضي —</option>
                  {pageTemplates.map((p) => <option key={p.id} value={p.id}>{p.nameAr}</option>)}
                </select>
              </Field>
              {full && <p className="text-xs text-muted-foreground">الحالة: {full.status === "Published" ? "منشور" : "مسودة"} • الإصدار {full.version}</p>}
            </div>
          )}

          {tab === "props" && (
            <PropertiesPanel block={selected} locked={locked} onChange={(patch) => selected && update(selected.id, patch)} />
          )}

          {tab === "tokens" && (
            <div className="space-y-2 border border-border bg-card p-3">
              <div className="relative">
                <Search className="absolute right-2 top-2.5 h-4 w-4 text-muted-foreground" />
                <input value={tokenSearch} onChange={(e) => setTokenSearch(e.target.value)} placeholder="ابحث عن رمز…" className="h-9 w-full border border-border bg-secondary pr-8 pl-3 text-sm" />
              </div>
              <p className="text-xs text-muted-foreground">انقر لإدراج الرمز في العنصر المحدد (أو إنشاء حقل رمز)</p>
              <div className="max-h-[28rem] space-y-3 overflow-auto">
                {filteredGroups.map((g) => (
                  <div key={g.group}>
                    <p className="text-xs font-bold text-muted-foreground">{g.group}</p>
                    <div className="mt-1 flex flex-wrap gap-1">
                      {g.tokens.map((t) => (
                        <button key={t.token} onClick={() => insertToken(t.token)} disabled={locked} title={t.token}
                          className="border border-border bg-secondary px-2 py-1 text-xs hover:border-primary/60 disabled:opacity-50">{t.label}</button>
                      ))}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

const inputCls = "h-9 w-full border border-border bg-secondary px-3 text-sm disabled:opacity-60";
function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return <div className="space-y-1"><label className="text-xs font-bold uppercase tracking-wider text-muted-foreground">{label}</label>{children}</div>;
}

// ── Sortable canvas block ──
function SortableBlock({ block, selected, locked, onSelect, onRemove }: {
  block: DocBlock; selected: boolean; locked: boolean; onSelect: () => void; onRemove: () => void;
}) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({ id: block.id, disabled: locked });
  const style = { transform: CSS.Transform.toString(transform), transition, opacity: isDragging ? 0.5 : 1 };
  return (
    <div ref={setNodeRef} style={style} onClick={onSelect}
      className={`group relative rounded-sm border ${selected ? "border-blue-500 ring-1 ring-blue-500" : "border-transparent hover:border-zinc-300"} p-1`}>
      {!locked && (
        <div className="absolute -right-7 top-1 hidden flex-col gap-1 group-hover:flex">
          <button {...attributes} {...listeners} className="cursor-grab text-zinc-400 hover:text-zinc-600" title="سحب"><GripVertical className="h-4 w-4" /></button>
          <button onClick={(e) => { e.stopPropagation(); onRemove(); }} className="text-red-400 hover:text-red-600" title="حذف"><Trash2 className="h-4 w-4" /></button>
        </div>
      )}
      <BlockPreview block={block} />
    </div>
  );
}

// ── Visual preview of a block (WYSIWYG, approximate PDF rendering) ──
function BlockPreview({ block: b }: { block: DocBlock }) {
  const align = b.align === "center" ? "text-center" : b.align === "left" ? "text-left" : "text-right";
  const size = b.size === "lg" ? "text-2xl" : b.size === "xl" ? "text-3xl" : b.size === "sm" ? "text-xs" : "text-sm";
  switch (b.type) {
    case "title": return <div className={`${align} text-xl font-bold`}>{b.text || "عنوان"}</div>;
    case "text": return <div className={`${align} ${size} ${b.bold ? "font-bold" : ""} whitespace-pre-wrap leading-relaxed`}>{b.text || "نص"}</div>;
    case "token": return <div className={`${align} ${size}`}><span className="rounded bg-blue-50 px-1 font-mono text-blue-700">{b.token || "{{token}}"}</span></div>;
    case "table": return (
      <table className="w-full border-collapse text-sm"><tbody>
        {(b.rows ?? []).map((r, i) => (
          <tr key={i}><td className="border border-zinc-200 bg-zinc-50 px-2 py-1 font-semibold">{r.label}</td><td className="border border-zinc-200 px-2 py-1">{r.value}</td></tr>
        ))}
      </tbody></table>
    );
    case "image": return (
      <div className={align}>{b.fileId
        ? <img src={fileUrl(`/api/files/${b.fileId}`)} alt="" style={{ width: b.width ?? 160 }} className="inline-block object-contain" />
        : <span className="inline-flex h-20 w-32 items-center justify-center border border-dashed border-zinc-300 text-xs text-zinc-400">صورة</span>}</div>
    );
    case "qr": return <div className={align}><span className="inline-flex h-20 w-20 items-center justify-center border border-zinc-300 text-zinc-400"><QrCode className="h-8 w-8" /></span></div>;
    case "signature": return (
      <div className="pt-4 text-center"><div className="mx-auto mb-1 h-10 w-40 border-b border-zinc-400" /><div className="text-sm font-semibold">{b.label || (b.role === "ceo" ? "الرئيس التنفيذي" : "إدارة الموارد البشرية")}</div></div>
    );
    case "stamp": return <div className={align}><span className="inline-flex h-20 w-20 items-center justify-center rounded-full border-2 border-dashed border-zinc-300 text-xs text-zinc-400">ختم</span></div>;
    case "divider": return <hr className="border-zinc-300" />;
    case "spacer": return <div style={{ height: b.height ?? 16 }} />;
    default: return null;
  }
}

// ── Properties panel for the selected block ──
function PropertiesPanel({ block, locked, onChange }: { block: DocBlock | null; locked: boolean; onChange: (patch: Partial<DocBlock>) => void }) {
  const [uploading, setUploading] = useState(false);
  const fileRef = useRef<HTMLInputElement>(null);
  if (!block) return <div className="border border-dashed border-border p-6 text-center text-sm text-muted-foreground">اختر عنصراً لتعديل خصائصه</div>;

  const onFile = async (f?: File) => {
    if (!f) return;
    setUploading(true);
    try { const r = await uploadFile(f, "documents"); onChange({ fileId: r.id }); toast.success("تم الرفع"); }
    catch { toast.error("تعذر الرفع"); }
    finally { setUploading(false); }
  };

  const AlignRow = () => (
    <Field label="المحاذاة">
      <div className="flex gap-1">
        {(["right", "center", "left"] as const).map((a) => (
          <button key={a} onClick={() => onChange({ align: a })} disabled={locked} className={`flex-1 border px-2 py-1 text-xs ${block.align === a ? "border-primary bg-primary/10" : "border-border"}`}>{a === "right" ? "يمين" : a === "center" ? "وسط" : "يسار"}</button>
        ))}
      </div>
    </Field>
  );

  return (
    <div className="space-y-3 border border-border bg-card p-3">
      <p className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-muted-foreground"><Settings2 className="h-3.5 w-3.5" /> {block.type}</p>

      {(block.type === "title" || block.type === "text") && (
        <Field label="النص"><textarea value={block.text ?? ""} onChange={(e) => onChange({ text: e.target.value })} disabled={locked} rows={4} className="w-full border border-border bg-secondary px-3 py-2 text-sm" /></Field>
      )}
      {block.type === "token" && (
        <Field label="الرمز"><input value={block.token ?? ""} onChange={(e) => onChange({ token: e.target.value })} dir="ltr" disabled={locked} className={inputCls} /></Field>
      )}

      {(block.type === "title" || block.type === "text" || block.type === "token" || block.type === "image" || block.type === "qr" || block.type === "stamp") && <AlignRow />}

      {(block.type === "title" || block.type === "text") && (
        <>
          <Field label="الحجم">
            <select value={block.size ?? "md"} onChange={(e) => onChange({ size: e.target.value as DocBlock["size"] })} disabled={locked} className={inputCls}>
              <option value="sm">صغير</option><option value="md">متوسط</option><option value="lg">كبير</option><option value="xl">كبير جداً</option>
            </select>
          </Field>
          <label className="flex items-center gap-2 text-sm"><input type="checkbox" checked={!!block.bold} onChange={(e) => onChange({ bold: e.target.checked })} disabled={locked} /> غامق</label>
        </>
      )}

      {block.type === "table" && (
        <div className="space-y-2">
          <p className="text-xs font-bold text-muted-foreground">الصفوف</p>
          {(block.rows ?? []).map((r, i) => (
            <div key={i} className="flex items-center gap-1">
              <input value={r.label} onChange={(e) => onChange({ rows: (block.rows ?? []).map((x, j) => j === i ? { ...x, label: e.target.value } : x) })} disabled={locked} placeholder="الحقل" className="h-8 w-1/2 border border-border bg-secondary px-2 text-xs" />
              <input value={r.value} onChange={(e) => onChange({ rows: (block.rows ?? []).map((x, j) => j === i ? { ...x, value: e.target.value } : x) })} disabled={locked} dir="ltr" placeholder="{{token}}" className="h-8 w-1/2 border border-border bg-secondary px-2 text-xs" />
              <button onClick={() => onChange({ rows: (block.rows ?? []).filter((_, j) => j !== i) })} disabled={locked} className="text-red-400"><Trash2 className="h-3.5 w-3.5" /></button>
            </div>
          ))}
          <button onClick={() => onChange({ rows: [...(block.rows ?? []), { label: "", value: "" }] })} disabled={locked} className="inline-flex h-8 items-center gap-1 border border-border px-2 text-xs hover:bg-muted"><Plus className="h-3.5 w-3.5" /> صف</button>
        </div>
      )}

      {block.type === "image" && (
        <Field label="الصورة">
          <button onClick={() => fileRef.current?.click()} disabled={locked || uploading} className="inline-flex h-9 items-center gap-2 border border-border px-3 text-sm hover:bg-muted disabled:opacity-50">{uploading ? <Loader2 className="h-4 w-4 animate-spin" /> : <Upload className="h-4 w-4" />} {block.fileId ? "تغيير الصورة" : "رفع صورة"}</button>
          <input ref={fileRef} type="file" accept="image/*" className="hidden" onChange={(e) => onFile(e.target.files?.[0])} />
        </Field>
      )}

      {block.type === "signature" && (
        <>
          <Field label="الدور">
            <select value={block.role ?? "hr"} onChange={(e) => onChange({ role: e.target.value as "hr" | "ceo" })} disabled={locked} className={inputCls}>
              <option value="hr">الموارد البشرية</option><option value="ceo">الرئيس التنفيذي</option>
            </select>
          </Field>
          <Field label="التسمية"><input value={block.label ?? ""} onChange={(e) => onChange({ label: e.target.value })} disabled={locked} className={inputCls} /></Field>
        </>
      )}

      {(block.type === "image" || block.type === "qr" || block.type === "stamp" || block.type === "signature") && (
        <Field label="العرض (نقاط)"><input type="number" value={block.width ?? 0} onChange={(e) => onChange({ width: Number(e.target.value) })} disabled={locked} dir="ltr" className={inputCls} /></Field>
      )}
      {block.type === "spacer" && (
        <Field label="الارتفاع (نقاط)"><input type="number" value={block.height ?? 16} onChange={(e) => onChange({ height: Number(e.target.value) })} disabled={locked} dir="ltr" className={inputCls} /></Field>
      )}
    </div>
  );
}

// ── Header / footer chrome hint (preview of the chosen page template) ──
function ChromeBand({ page, which }: { page: PageTemplate | null; which: "header" | "footer" }) {
  const label = which === "header" ? "ترويسة" : "تذييل";
  return (
    <div className={`flex items-center justify-between border-zinc-200 px-12 py-3 text-[10px] text-zinc-400 ${which === "header" ? "border-b" : "border-t"}`}>
      <span>{label} {page ? `— ${page.nameAr}` : "— افتراضي"}</span>
      <span>{which === "header" ? "الشعار + هوية الشركة" : "رمز QR • الختم • التاريخ"}</span>
    </div>
  );
}
