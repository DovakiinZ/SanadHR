"use client";

import { useEffect, useState } from "react";
import { GitBranch, Loader2, Pencil, Play, Plus, Trash2, X } from "lucide-react";
import { toast } from "sonner";
import { ApiError } from "@/lib/api-client";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  createWorkflowDefinition, deleteWorkflowDefinition, listWorkflowDefinitions,
  startWorkflowRequest, type WorkflowDefinitionSummaryDto,
} from "@/lib/api/workflow-builder";

export function DefinitionsPanel({
  canManage, reloadToken, onOpen, onStarted,
}: {
  canManage: boolean;
  reloadToken: number;
  onOpen: (id: string) => void;
  onStarted: () => void;
}) {
  const [defs, setDefs] = useState<WorkflowDefinitionSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [creating, setCreating] = useState(false);
  const [code, setCode] = useState("");
  const [name, setName] = useState("");
  const [busy, setBusy] = useState(false);
  const [startFor, setStartFor] = useState<WorkflowDefinitionSummaryDto | null>(null);

  useEffect(() => {
    setLoading(true);
    listWorkflowDefinitions()
      .then((d) => setDefs(d ?? []))
      .catch((e) => toast.error(e instanceof ApiError ? e.message : "تعذر تحميل المسارات"))
      .finally(() => setLoading(false));
  }, [reloadToken]);

  async function create() {
    if (!code.trim() || !name.trim()) { toast.error("أدخل الرمز والاسم"); return; }
    setBusy(true);
    try {
      const def = await createWorkflowDefinition({ code: code.trim(), name: name.trim() });
      toast.success("تم إنشاء المسار");
      setCreating(false); setCode(""); setName("");
      onOpen(def.id); // jump straight into the builder
    } catch (e) {
      toast.error(e instanceof ApiError ? e.message : "تعذر الإنشاء");
    } finally { setBusy(false); }
  }

  async function remove(d: WorkflowDefinitionSummaryDto) {
    if (!confirm(`حذف المسار "${d.name}"؟`)) return;
    try {
      await deleteWorkflowDefinition(d.id);
      toast.success("تم الحذف");
      setDefs((xs) => xs.filter((x) => x.id !== d.id));
    } catch (e) { toast.error(e instanceof ApiError ? e.message : "تعذر الحذف"); }
  }

  if (loading) return <div className="flex justify-center p-12"><Loader2 className="animate-spin text-muted-foreground" /></div>;

  return (
    <div className="space-y-4">
      {canManage && (
        creating ? (
          <div className="flex flex-wrap items-end gap-2 rounded-xl border border-border bg-card p-3">
            <label className="space-y-1"><span className="text-xs text-muted-foreground">الرمز</span>
              <Input value={code} onChange={(e) => setCode(e.target.value)} placeholder="annual-leave-approval" className="w-56" /></label>
            <label className="space-y-1"><span className="text-xs text-muted-foreground">الاسم</span>
              <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="اعتماد الإجازة السنوية" className="w-64" /></label>
            <Button onClick={create} disabled={busy}>{busy ? <Loader2 className="animate-spin" /> : <Plus />} إنشاء وفتح المُصمّم</Button>
            <Button variant="ghost" onClick={() => setCreating(false)}><X /> إلغاء</Button>
          </div>
        ) : (
          <Button onClick={() => setCreating(true)}><Plus /> مسار جديد</Button>
        )
      )}

      {defs.length === 0 ? (
        <div className="rounded-xl border border-dashed border-border p-10 text-center text-sm text-muted-foreground">
          لا توجد مسارات بعد.
        </div>
      ) : (
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {defs.map((d) => (
            <div key={d.id} className="flex flex-col rounded-xl border border-border bg-card p-4">
              <div className="flex items-start gap-2">
                <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary/10 text-primary"><GitBranch className="h-4 w-4" /></div>
                <div className="min-w-0 flex-1">
                  <div className="flex items-center gap-1.5">
                    <span className="truncate text-sm font-medium">{d.name}</span>
                    {d.isActive ? <Badge variant="secondary">نشط</Badge> : <Badge variant="outline">معطّل</Badge>}
                  </div>
                  <span className="text-xs text-muted-foreground">{d.code}</span>
                </div>
              </div>
              <div className="mt-3 flex gap-3 text-xs text-muted-foreground">
                <span>الإصدار v{d.version}</span>
                <span>{d.stepCount} خطوة</span>
                <span>{d.requestCount} طلب</span>
              </div>
              <div className="mt-3 flex gap-1.5">
                <Button variant="outline" size="sm" onClick={() => onOpen(d.id)}><Pencil /> المُصمّم</Button>
                {d.isActive && d.stepCount > 0 && (
                  <Button variant="secondary" size="sm" onClick={() => setStartFor(d)}><Play /> بدء طلب</Button>
                )}
                {canManage && <Button variant="ghost" size="icon-sm" className="ms-auto" onClick={() => remove(d)}><Trash2 /></Button>}
              </div>
            </div>
          ))}
        </div>
      )}

      {startFor && (
        <StartRequestModal
          def={startFor}
          onClose={() => setStartFor(null)}
          onStarted={() => { setStartFor(null); onStarted(); }}
        />
      )}
    </div>
  );
}

function StartRequestModal({
  def, onClose, onStarted,
}: { def: WorkflowDefinitionSummaryDto; onClose: () => void; onStarted: () => void }) {
  const [payload, setPayload] = useState('{\n  "amount": 1000\n}');
  const [busy, setBusy] = useState(false);

  async function start() {
    let parsed: string;
    try { parsed = JSON.stringify(JSON.parse(payload)); }
    catch { toast.error("البيانات ليست JSON صالحاً"); return; }
    setBusy(true);
    try {
      const req = await startWorkflowRequest({ definitionId: def.id, payload: parsed });
      toast.success(`تم بدء الطلب ${req.requestNumber}`);
      onStarted();
    } catch (e) { toast.error(e instanceof ApiError ? e.message : "تعذر بدء الطلب"); }
    finally { setBusy(false); }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/30 p-4" onClick={onClose}>
      <div className="w-full max-w-md rounded-xl border border-border bg-popover p-4 shadow-lg" onClick={(e) => e.stopPropagation()}>
        <div className="mb-2 flex items-center justify-between">
          <h3 className="text-sm font-medium">بدء طلب — {def.name}</h3>
          <Button variant="ghost" size="icon-sm" onClick={onClose}><X /></Button>
        </div>
        <p className="mb-2 text-xs text-muted-foreground">بيانات الطلب (JSON) — تُستخدم في الشروط ورسائل البريد.</p>
        <textarea value={payload} onChange={(e) => setPayload(e.target.value)} rows={8} dir="ltr"
          className="w-full rounded-lg border border-input bg-transparent px-2.5 py-1.5 font-mono text-xs outline-none focus-visible:border-ring" />
        <div className="mt-3 flex justify-end gap-2">
          <Button variant="ghost" onClick={onClose}>إلغاء</Button>
          <Button onClick={start} disabled={busy}>{busy ? <Loader2 className="animate-spin" /> : <Play />} بدء</Button>
        </div>
      </div>
    </div>
  );
}
