"use client";

import { useCallback, useEffect, useState } from "react";
import { FileText, Plus, Pencil, Trash2, Loader2, Download, Eye, Upload, AlertTriangle } from "lucide-react";
import { toast } from "sonner";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { ApiError } from "@/lib/api-client";
import { fileUrl, uploadFile } from "@/lib/api/files";
import {
  EmployeeDocument, DOCUMENT_TYPES, documentTypeLabel,
  getEmployeeDocuments, createEmployeeDocument, updateEmployeeDocument, deleteEmployeeDocument,
} from "@/lib/api/employee-documents";

function notifyError(err: unknown, fallback: string) {
  if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
    toast.error(err instanceof ApiError ? err.message : fallback);
  }
}

interface DocForm {
  type: string;
  title: string;
  documentNumber: string;
  issueDate: string;
  expiryDate: string;
  notes: string;
  fileUrl: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
}

const emptyForm: DocForm = {
  type: "Iqama", title: "", documentNumber: "", issueDate: "", expiryDate: "",
  notes: "", fileUrl: "", fileName: "", contentType: "", sizeBytes: 0,
};

// Returns an expiry descriptor: expired / soon (<=30d) / valid / none.
function expiryState(expiry?: string | null): { label: string; cls: string } | null {
  if (!expiry) return null;
  const d = new Date(expiry);
  if (isNaN(d.getTime())) return null;
  const days = Math.floor((d.getTime() - Date.now()) / 86_400_000);
  const date = expiry.slice(0, 10);
  if (days < 0) return { label: `منتهٍ — ${date}`, cls: "bg-destructive/10 text-destructive border-destructive/20" };
  if (days <= 30) return { label: `ينتهي خلال ${days} يوم`, cls: "bg-amber-500/10 text-amber-600 border-amber-500/20" };
  return { label: `صالح حتى ${date}`, cls: "bg-green-500/10 text-green-600 border-green-500/20" };
}

export function EmployeeDocuments({ employeeId, canWrite }: { employeeId: string; canWrite: boolean }) {
  const [docs, setDocs] = useState<EmployeeDocument[]>([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<EmployeeDocument | null>(null);
  const [form, setForm] = useState<DocForm>(emptyForm);
  const [saving, setSaving] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<EmployeeDocument | null>(null);
  const [deleting, setDeleting] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try { setDocs(await getEmployeeDocuments(employeeId)); }
    catch (err) { notifyError(err, "تعذر تحميل المستندات"); }
    finally { setLoading(false); }
  }, [employeeId]);

  useEffect(() => { load(); }, [load]);

  function openCreate() { setEditing(null); setForm(emptyForm); setDialogOpen(true); }
  function openEdit(d: EmployeeDocument) {
    setEditing(d);
    setForm({
      type: d.type, title: d.title, documentNumber: d.documentNumber ?? "",
      issueDate: d.issueDate?.slice(0, 10) ?? "", expiryDate: d.expiryDate?.slice(0, 10) ?? "",
      notes: d.notes ?? "", fileUrl: d.fileUrl, fileName: d.fileName ?? "",
      contentType: d.contentType ?? "", sizeBytes: d.sizeBytes,
    });
    setDialogOpen(true);
  }

  async function onFile(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;
    setUploading(true);
    try {
      const res = await uploadFile(file, "document");
      setForm((f) => ({ ...f, fileUrl: res.url, fileName: res.fileName, contentType: res.contentType, sizeBytes: res.sizeBytes }));
      toast.success("تم رفع الملف");
    } catch (err) { notifyError(err, "تعذر رفع الملف"); }
    finally { setUploading(false); e.target.value = ""; }
  }

  async function save() {
    if (!form.title.trim()) { toast.error("اسم المستند مطلوب"); return; }
    if (!form.fileUrl) { toast.error("يجب رفع ملف المستند"); return; }
    setSaving(true);
    try {
      const payload = {
        type: form.type, title: form.title.trim(),
        documentNumber: form.documentNumber.trim() || null,
        issueDate: form.issueDate || null, expiryDate: form.expiryDate || null,
        notes: form.notes.trim() || null,
        fileUrl: form.fileUrl, fileName: form.fileName || null,
        contentType: form.contentType || null, sizeBytes: form.sizeBytes,
      };
      if (editing) { await updateEmployeeDocument(employeeId, editing.id, payload); toast.success("تم تحديث المستند"); }
      else { await createEmployeeDocument(employeeId, payload); toast.success("تمت إضافة المستند"); }
      setDialogOpen(false); await load();
    } catch (err) { notifyError(err, "تعذر حفظ المستند"); } finally { setSaving(false); }
  }

  async function confirmDelete() {
    if (!deleteTarget) return;
    setDeleting(true);
    try { await deleteEmployeeDocument(employeeId, deleteTarget.id); toast.success("تم حذف المستند"); setDeleteTarget(null); await load(); }
    catch (err) { notifyError(err, "تعذر حذف المستند"); } finally { setDeleting(false); }
  }

  const selectClass = "w-full h-9 bg-secondary border border-border px-3 text-sm text-foreground";

  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-bold">مستندات الموظف</h3>
        {canWrite && (
          <Button onClick={openCreate} className="h-8 gap-2 text-xs"><Plus className="h-4 w-4" /> مستند</Button>
        )}
      </div>

      {loading ? (
        <div className="py-10 text-center text-sm text-muted-foreground"><Loader2 className="h-4 w-4 animate-spin inline" /> جاري التحميل...</div>
      ) : docs.length === 0 ? (
        <div className="py-10 text-center text-sm text-muted-foreground border border-dashed border-border">لا توجد مستندات</div>
      ) : (
        <div className="space-y-2">
          {docs.map((d) => {
            const exp = expiryState(d.expiryDate);
            const href = fileUrl(d.fileUrl);
            return (
              <div key={d.id} className="flex items-center justify-between gap-3 border border-border bg-card px-4 py-3">
                <div className="min-w-0 flex items-center gap-3">
                  <FileText className="h-5 w-5 shrink-0 text-primary" />
                  <div className="min-w-0">
                    <div className="flex items-center gap-2">
                      <span className="font-medium truncate">{d.title}</span>
                      <Badge variant="outline" className="text-[10px] border-border text-muted-foreground shrink-0">{documentTypeLabel(d.type)}</Badge>
                    </div>
                    <div className="flex flex-wrap items-center gap-2 mt-0.5 text-xs text-muted-foreground">
                      {d.documentNumber && <span className="font-mono">{d.documentNumber}</span>}
                      {exp && <span className={`inline-flex items-center gap-1 border px-1.5 py-0.5 ${exp.cls}`}>{exp.cls.includes("destructive") || exp.cls.includes("amber") ? <AlertTriangle className="h-3 w-3" /> : null}{exp.label}</span>}
                    </div>
                  </div>
                </div>
                <div className="flex items-center gap-1 shrink-0">
                  {href && <a href={href} target="_blank" rel="noreferrer" className="h-8 w-8 inline-flex items-center justify-center text-muted-foreground hover:text-foreground" title="عرض"><Eye className="h-4 w-4" /></a>}
                  {href && <a href={href} download={d.fileName ?? undefined} className="h-8 w-8 inline-flex items-center justify-center text-muted-foreground hover:text-foreground" title="تحميل"><Download className="h-4 w-4" /></a>}
                  {canWrite && <button onClick={() => openEdit(d)} className="h-8 w-8 inline-flex items-center justify-center text-muted-foreground hover:text-foreground" title="تعديل"><Pencil className="h-4 w-4" /></button>}
                  {canWrite && <button onClick={() => setDeleteTarget(d)} className="h-8 w-8 inline-flex items-center justify-center text-destructive hover:text-destructive/80" title="حذف"><Trash2 className="h-4 w-4" /></button>}
                </div>
              </div>
            );
          })}
        </div>
      )}

      <Dialog open={dialogOpen} onOpenChange={(o) => { if (!o && !saving) setDialogOpen(false); }}>
        <DialogContent className="sm:max-w-xl">
          <DialogHeader><DialogTitle>{editing ? "تعديل مستند" : "مستند جديد"}</DialogTitle></DialogHeader>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 py-2 max-h-[68vh] overflow-y-auto pl-1">
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">نوع المستند</Label>
              <select value={form.type} onChange={(e) => setForm({ ...form, type: e.target.value })} className={selectClass}>
                {DOCUMENT_TYPES.map((t) => <option key={t.code} value={t.code}>{t.labelAr}</option>)}
              </select>
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">اسم المستند</Label>
              <Input value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} className="bg-secondary border-border" placeholder="مثال: إقامة سارية" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">رقم المستند</Label>
              <Input value={form.documentNumber} onChange={(e) => setForm({ ...form, documentNumber: e.target.value })} className="bg-secondary border-border font-mono" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">تاريخ الإصدار</Label>
              <Input type="date" value={form.issueDate} onChange={(e) => setForm({ ...form, issueDate: e.target.value })} className="bg-secondary border-border" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">تاريخ الانتهاء</Label>
              <Input type="date" value={form.expiryDate} onChange={(e) => setForm({ ...form, expiryDate: e.target.value })} className="bg-secondary border-border" />
            </div>
            <div className="space-y-2 sm:col-span-2">
              <Label className="text-xs font-bold uppercase tracking-wider">ملاحظات</Label>
              <Input value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} className="bg-secondary border-border" />
            </div>
            <div className="space-y-2 sm:col-span-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الملف</Label>
              <div className="flex items-center gap-3">
                <label className="inline-flex items-center gap-2 h-9 px-4 border border-border bg-secondary text-sm cursor-pointer hover:bg-card transition-colors shrink-0">
                  {uploading ? <Loader2 className="h-4 w-4 animate-spin" /> : <Upload className="h-4 w-4" />}
                  {uploading ? "جاري الرفع..." : (editing ? "استبدال الملف" : "رفع الملف")}
                  <input type="file" className="hidden" onChange={onFile} disabled={uploading} />
                </label>
                {form.fileName ? <span className="text-xs text-muted-foreground truncate">{form.fileName}</span> : <span className="text-xs text-muted-foreground">لم يتم اختيار ملف</span>}
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)} disabled={saving}>إلغاء</Button>
            <Button onClick={save} disabled={saving || uploading} className="font-bold">{saving ? "جاري الحفظ..." : "حفظ"}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={!!deleteTarget} onOpenChange={(o) => { if (!o && !deleting) setDeleteTarget(null); }}>
        <DialogContent showCloseButton={false}>
          <DialogHeader>
            <DialogTitle>حذف مستند</DialogTitle>
            <DialogDescription>هل أنت متأكد من حذف <span className="font-bold text-foreground">{deleteTarget?.title}</span>؟</DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteTarget(null)} disabled={deleting}>إلغاء</Button>
            <Button onClick={confirmDelete} disabled={deleting} className="bg-destructive text-white hover:bg-destructive/90">{deleting ? "جاري الحذف..." : "حذف"}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
