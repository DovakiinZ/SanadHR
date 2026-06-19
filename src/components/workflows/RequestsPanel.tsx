"use client";

import { useEffect, useState } from "react";
import { Ban, Check, Loader2, X } from "lucide-react";
import { toast } from "sonner";
import { ApiError } from "@/lib/api-client";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Sheet, SheetContent, SheetHeader, SheetTitle, SheetDescription,
} from "@/components/ui/sheet";
import {
  cancelWorkflowRequest, executeWorkflowStep, getWorkflowRequest, listWorkflowRequests,
  STATUS_LABEL, WorkflowRequestStatus, type WorkflowRequestDto,
} from "@/lib/api/workflow-builder";

const STATUS_VARIANT: Record<WorkflowRequestStatus, "default" | "secondary" | "destructive" | "outline"> = {
  [WorkflowRequestStatus.Pending]: "outline",
  [WorkflowRequestStatus.InProgress]: "secondary",
  [WorkflowRequestStatus.Completed]: "default",
  [WorkflowRequestStatus.Cancelled]: "outline",
  [WorkflowRequestStatus.Rejected]: "destructive",
};

export function RequestsPanel({ canApprove, reloadToken }: { canApprove: boolean; reloadToken: number }) {
  const [rows, setRows] = useState<WorkflowRequestDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<WorkflowRequestStatus | "">("");
  const [selected, setSelected] = useState<WorkflowRequestDto | null>(null);
  const [local, setLocal] = useState(0); // bump after an action to reload

  useEffect(() => {
    setLoading(true);
    listWorkflowRequests(filter === "" ? {} : { status: filter })
      .then((d) => setRows(d ?? []))
      .catch((e) => toast.error(e instanceof ApiError ? e.message : "تعذر تحميل الطلبات"))
      .finally(() => setLoading(false));
  }, [filter, reloadToken, local]);

  async function open(id: string) {
    try { setSelected(await getWorkflowRequest(id)); }
    catch (e) { toast.error(e instanceof ApiError ? e.message : "تعذر فتح الطلب"); }
  }

  return (
    <div className="space-y-3">
      <div className="flex items-center gap-2">
        <span className="text-xs text-muted-foreground">الحالة:</span>
        <select value={filter} onChange={(e) => setFilter(e.target.value === "" ? "" : Number(e.target.value) as WorkflowRequestStatus)}
          className="h-8 rounded-lg border border-input bg-transparent px-2 text-sm outline-none focus-visible:border-ring">
          <option value="">الكل</option>
          {Object.values(WorkflowRequestStatus).filter((v) => typeof v === "number").map((v) => (
            <option key={v} value={v}>{STATUS_LABEL[v as WorkflowRequestStatus]}</option>
          ))}
        </select>
      </div>

      {loading ? (
        <div className="flex justify-center p-12"><Loader2 className="animate-spin text-muted-foreground" /></div>
      ) : rows.length === 0 ? (
        <div className="rounded-xl border border-dashed border-border p-10 text-center text-sm text-muted-foreground">لا توجد طلبات.</div>
      ) : (
        <div className="overflow-hidden rounded-xl border border-border">
          <table className="w-full text-sm">
            <thead className="bg-secondary text-xs text-muted-foreground">
              <tr>
                <th className="px-3 py-2 text-start">الرقم</th>
                <th className="px-3 py-2 text-start">المسار</th>
                <th className="px-3 py-2 text-start">الحالة</th>
                <th className="px-3 py-2 text-start">الخطوة الحالية</th>
                <th className="px-3 py-2 text-start">البدء</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((r) => (
                <tr key={r.id} onClick={() => open(r.id)} className="cursor-pointer border-t border-border hover:bg-muted/50">
                  <td className="px-3 py-2 font-mono text-xs">{r.requestNumber}</td>
                  <td className="px-3 py-2">{r.definitionName}</td>
                  <td className="px-3 py-2"><Badge variant={STATUS_VARIANT[r.status]}>{STATUS_LABEL[r.status]}</Badge></td>
                  <td className="px-3 py-2 text-muted-foreground">{r.currentStepName ?? "—"}</td>
                  <td className="px-3 py-2 text-xs text-muted-foreground">{new Date(r.startedAt).toLocaleString("ar")}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <Sheet open={!!selected} onOpenChange={(o) => !o && setSelected(null)}>
        <SheetContent className="w-full sm:max-w-md">
          {selected && (
            <RequestDetail
              req={selected}
              canApprove={canApprove}
              onActed={() => { setSelected(null); setLocal((x) => x + 1); }}
            />
          )}
        </SheetContent>
      </Sheet>
    </div>
  );
}

function RequestDetail({
  req, canApprove, onActed,
}: { req: WorkflowRequestDto; canApprove: boolean; onActed: () => void }) {
  const [comment, setComment] = useState("");
  const [busy, setBusy] = useState(false);
  const isOpen = req.status === WorkflowRequestStatus.InProgress;
  const awaitingApproval = isOpen && !!req.currentStepName;

  async function act(fn: () => Promise<unknown>, ok: string) {
    setBusy(true);
    try { await fn(); toast.success(ok); onActed(); }
    catch (e) { toast.error(e instanceof ApiError ? e.message : "تعذر تنفيذ الإجراء"); }
    finally { setBusy(false); }
  }

  return (
    <>
      <SheetHeader>
        <SheetTitle className="font-mono">{req.requestNumber}</SheetTitle>
        <SheetDescription>{req.definitionName} — {STATUS_LABEL[req.status]}</SheetDescription>
      </SheetHeader>

      <div className="space-y-4 overflow-auto px-4 pb-4">
        {awaitingApproval && (
          <div className="rounded-lg border border-border p-3">
            <p className="mb-1 text-xs font-medium text-muted-foreground">الخطوة الحالية</p>
            <p className="text-sm">{req.currentStepName}</p>
            {canApprove && (
              <>
                <textarea value={comment} onChange={(e) => setComment(e.target.value)} rows={2} placeholder="ملاحظة (اختياري)"
                  className="mt-2 w-full rounded-lg border border-input bg-transparent px-2.5 py-1.5 text-sm outline-none focus-visible:border-ring" />
                <div className="mt-2 flex gap-2">
                  <Button size="sm" disabled={busy}
                    onClick={() => act(() => executeWorkflowStep(req.id, { approved: true, comment }), "تمت الموافقة")}>
                    <Check /> موافقة
                  </Button>
                  <Button size="sm" variant="destructive" disabled={busy}
                    onClick={() => act(() => executeWorkflowStep(req.id, { approved: false, comment }), "تم الرفض")}>
                    <X /> رفض
                  </Button>
                </div>
              </>
            )}
          </div>
        )}

        {/* Payload */}
        <div>
          <p className="mb-1 text-xs font-medium text-muted-foreground">البيانات</p>
          <pre dir="ltr" className="overflow-auto rounded-lg border border-border bg-secondary p-2 font-mono text-xs">{prettyJson(req.payload)}</pre>
        </div>

        {/* Audit trail timeline */}
        <div>
          <p className="mb-2 text-xs font-medium text-muted-foreground">سجل التتبّع</p>
          <ol className="relative space-y-3 border-s border-border ps-4">
            {req.auditTrail.map((a) => (
              <li key={a.id} className="relative">
                <span className="absolute -start-[1.30rem] top-1 h-2 w-2 rounded-full bg-primary" />
                <div className="flex items-center gap-2">
                  <span className="text-sm font-medium">{translateAction(a.action)}</span>
                  {a.stepName && <span className="text-xs text-muted-foreground">· {a.stepName}</span>}
                </div>
                {a.result && <p className="text-xs text-muted-foreground">{a.result}</p>}
                {a.comment && <p className="text-xs">"{a.comment}"</p>}
                <p className="text-[11px] text-muted-foreground">{new Date(a.occurredAt).toLocaleString("ar")}</p>
              </li>
            ))}
          </ol>
        </div>

        {isOpen && (
          <Button variant="outline" size="sm" disabled={busy}
            onClick={() => act(() => cancelWorkflowRequest(req.id), "تم إلغاء الطلب")}>
            <Ban /> إلغاء الطلب
          </Button>
        )}
      </div>
    </>
  );
}

function prettyJson(s: string): string {
  try { return JSON.stringify(JSON.parse(s), null, 2); } catch { return s; }
}

const ACTION_LABEL: Record<string, string> = {
  Submitted: "تم التقديم",
  Approved: "تمت الموافقة",
  Rejected: "تم الرفض",
  ActionExecuted: "تنفيذ إجراء",
  ConditionEvaluated: "تقييم شرط",
  Completed: "اكتمل",
  Cancelled: "أُلغي",
};
function translateAction(a: string): string { return ACTION_LABEL[a] ?? a; }
