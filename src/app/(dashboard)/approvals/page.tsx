"use client";

import { type ReactNode, useCallback, useEffect, useMemo, useState } from "react";
import { AlertTriangle, Check, Clock, Loader2, Paperclip, RotateCcw, UserCheck, X } from "lucide-react";
import { toast } from "sonner";
import { ApiError } from "@/lib/api-client";
import { fileUrl } from "@/lib/api/files";
import { usePermissions } from "@/lib/permissions";
import {
  approvalStatusColor, ApprovalDetail, ApprovalTab, ApprovalTask, approveTask, classifyTab,
  getApproval, getMyApprovals, rejectTask, returnTask,
} from "@/lib/api/approvals";

const TABS: { key: ApprovalTab; label: string }[] = [
  { key: "pending", label: "بانتظاري" },
  { key: "overdue", label: "متأخرة" },
  { key: "approved", label: "تمت الموافقة" },
  { key: "rejected", label: "مرفوضة" },
  { key: "returned", label: "معادة للتعديل" },
];

const STATUS_AR: Record<string, string> = { Pending: "بانتظار", Approved: "تمت الموافقة", Rejected: "مرفوض", Returned: "معاد للتعديل", Skipped: "تم التخطي" };

export default function ApprovalsPage() {
  const { has } = usePermissions();
  const canViewAll = has("approvals.view_all") || has("Platform.Admin.View");
  const [tab, setTab] = useState<ApprovalTab>("pending");
  const [all, setAll] = useState(false);
  const [tasks, setTasks] = useState<ApprovalTask[]>([]);
  const [loading, setLoading] = useState(true);
  const [openId, setOpenId] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    try { setTasks(await getMyApprovals(all && canViewAll)); }
    catch (e) { if (!(e instanceof ApiError) || ![401, 403, 500].includes(e.status)) toast.error("تعذر تحميل الموافقات"); }
    finally { setLoading(false); }
  }, [all, canViewAll]);

  useEffect(() => { load(); }, [load]);

  const counts = useMemo(() => Object.fromEntries(TABS.map((t) => [t.key, tasks.filter((x) => classifyTab(x, t.key)).length])), [tasks]);
  const filtered = useMemo(() => tasks.filter((t) => classifyTab(t, tab)), [tasks, tab]);

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold">مركز الموافقات</h1>
          <p className="mt-1 text-sm text-muted-foreground">الطلبات والمهام الموكلة إليك للموافقة</p>
        </div>
        {canViewAll && (
          <label className="flex items-center gap-2 text-sm text-muted-foreground">
            <input type="checkbox" checked={all} onChange={(e) => setAll(e.target.checked)} /> عرض كل الموافقات (مسؤول)
          </label>
        )}
      </div>

      <div className="flex flex-wrap items-center gap-1 border-b border-border">
        {TABS.map((t) => (
          <button key={t.key} onClick={() => setTab(t.key)} className={`-mb-px flex items-center gap-1.5 border-b-2 px-4 py-2 text-sm ${tab === t.key ? "border-primary font-bold" : "border-transparent text-muted-foreground hover:text-foreground"}`}>
            {t.key === "overdue" && counts.overdue > 0 && <AlertTriangle className="h-3.5 w-3.5 text-red-400" />}
            {t.label}
            {counts[t.key] > 0 && <span className="bg-secondary px-1.5 text-xs text-muted-foreground">{counts[t.key]}</span>}
          </button>
        ))}
      </div>

      {loading ? (
        <div className="flex h-64 items-center justify-center text-muted-foreground"><Loader2 className="h-6 w-6 animate-spin" /></div>
      ) : filtered.length === 0 ? (
        <div className="border border-dashed border-border py-16 text-center text-muted-foreground">لا توجد عناصر</div>
      ) : (
        <div className="space-y-2">
          {filtered.map((t) => (
            <button key={t.id} onClick={() => setOpenId(t.id)} className="flex w-full flex-wrap items-center justify-between gap-3 border border-border bg-card px-4 py-3 text-right hover:bg-muted/40">
              <div className="min-w-0">
                <div className="flex items-center gap-2">
                  <span className="font-medium">{t.requestTypeNameAr}</span>
                  <span className="font-mono text-xs text-muted-foreground">{t.requestNumber}</span>
                </div>
                <div className="mt-0.5 flex flex-wrap items-center gap-x-3 gap-y-0.5 text-xs text-muted-foreground">
                  <span className="inline-flex items-center gap-1"><UserCheck className="h-3 w-3" /> {t.ownerName}</span>
                  {t.department && <span>{t.department}</span>}
                  <span>{new Date(t.submittedAt).toLocaleDateString("ar")}</span>
                  <span>الخطوة: {t.currentStepNameAr}</span>
                  {t.daysCount ? <span>{t.daysCount} يوم</span> : null}
                </div>
              </div>
              <div className="flex items-center gap-2">
                {t.status === "Pending" && (
                  <span className={`inline-flex items-center gap-1 border px-2 py-1 text-xs ${t.overdue ? "border-red-500/30 bg-red-500/10 text-red-400" : "border-border text-muted-foreground"}`}>
                    <Clock className="h-3 w-3" /> {t.overdue ? "متأخر" : t.dueAt ? `حتى ${new Date(t.dueAt).toLocaleDateString("ar")}` : "ضمن المهلة"}
                  </span>
                )}
                <span className={`border px-2 py-1 text-xs ${approvalStatusColor(t.status)}`}>{STATUS_AR[t.status] ?? t.status}</span>
              </div>
            </button>
          ))}
        </div>
      )}

      {openId && <ApprovalDrawer id={openId} onClose={() => setOpenId(null)} onDecided={() => { setOpenId(null); load(); }} />}
    </div>
  );
}

function ApprovalDrawer({ id, onClose, onDecided }: { id: string; onClose: () => void; onDecided: () => void }) {
  const [d, setD] = useState<ApprovalDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [comment, setComment] = useState("");
  const [acting, setActing] = useState<string | null>(null);

  useEffect(() => {
    getApproval(id).then(setD).catch(() => toast.error("تعذر تحميل التفاصيل")).finally(() => setLoading(false));
  }, [id]);

  const act = async (kind: "approve" | "reject" | "return") => {
    if ((kind === "reject" || kind === "return") && !comment.trim()) { toast.error("يرجى إضافة تعليق"); return; }
    setActing(kind);
    try {
      if (kind === "approve") await approveTask(id, comment || undefined);
      else if (kind === "reject") await rejectTask(id, comment);
      else await returnTask(id, comment);
      toast.success(kind === "approve" ? "تمت الموافقة" : kind === "reject" ? "تم الرفض" : "أُعيد للتعديل");
      onDecided();
    } catch (e) {
      toast.error(e instanceof ApiError ? e.message : "تعذر تنفيذ الإجراء");
    } finally {
      setActing(null);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex justify-start">
      <div className="absolute inset-0 bg-black/50" onClick={onClose} />
      <div className="relative z-10 flex h-full w-full max-w-xl flex-col border-l border-border bg-background">
        {loading || !d ? (
          <div className="flex flex-1 items-center justify-center text-muted-foreground"><Loader2 className="h-6 w-6 animate-spin" /></div>
        ) : (
          <>
            <div className="flex items-center justify-between border-b border-border px-5 py-4">
              <div>
                <h3 className="font-bold">{d.requestTypeNameAr}</h3>
                <p className="font-mono text-xs text-muted-foreground">{d.requestNumber}</p>
              </div>
              <button onClick={onClose} className="text-muted-foreground hover:text-foreground"><X className="h-5 w-5" /></button>
            </div>

            <div className="flex-1 space-y-5 overflow-auto p-5">
              {/* Employee */}
              <Section title="مقدّم الطلب">
                <Kv k="الاسم" v={d.ownerName} />
                {d.ownerNumber && <Kv k="الرقم الوظيفي" v={d.ownerNumber} />}
                {d.department && <Kv k="الإدارة" v={d.department} />}
                {d.jobTitle && <Kv k="المسمى الوظيفي" v={d.jobTitle} />}
              </Section>

              {/* Request details */}
              <Section title="تفاصيل الطلب">
                {d.leaveTypeName && <Kv k="نوع الإجازة" v={d.leaveTypeName} />}
                {d.startDate && <Kv k="من" v={new Date(d.startDate).toLocaleDateString("ar")} />}
                {d.endDate && <Kv k="إلى" v={new Date(d.endDate).toLocaleDateString("ar")} />}
                {d.daysCount ? <Kv k="عدد الأيام" v={`${d.daysCount}`} /> : null}
                {d.details.map((x, i) => <Kv key={i} k={x.label} v={x.value} />)}
              </Section>

              {/* Attachments */}
              {d.attachments.length > 0 && (
                <Section title="المرفقات">
                  {d.attachments.map((a, i) => (
                    <a key={i} href={fileUrl(a.value)} target="_blank" rel="noreferrer" className="flex items-center gap-2 text-sm text-primary hover:underline">
                      <Paperclip className="h-3.5 w-3.5" /> {a.label}
                    </a>
                  ))}
                </Section>
              )}

              {/* Impact — what will happen */}
              {d.impact.length > 0 && (
                <div className="border border-amber-500/30 bg-amber-500/5 p-3">
                  <p className="mb-2 text-xs font-bold uppercase tracking-wider text-amber-400">عند الموافقة سيحدث التالي</p>
                  <ul className="list-inside list-disc space-y-1 text-sm">
                    {d.impact.map((x, i) => <li key={i}>{x}</li>)}
                  </ul>
                </div>
              )}

              {/* Previous approvers / chain */}
              <Section title="سلسلة الموافقات">
                {d.approvals.map((a) => (
                  <div key={a.stepOrder} className="flex items-center justify-between text-sm">
                    <span>{a.stepOrder}. {a.stepNameAr}{a.comment ? ` — ${a.comment}` : ""}</span>
                    <span className={`border px-2 py-0.5 text-xs ${approvalStatusColor(a.status)}`}>{STATUS_AR[a.status] ?? a.status}</span>
                  </div>
                ))}
              </Section>

              {/* Timeline */}
              <Section title="المسار الزمني">
                {d.timeline.map((h, i) => (
                  <div key={i} className="flex items-start gap-2 text-sm">
                    <span className="mt-1 h-2 w-2 shrink-0 rounded-full bg-primary" />
                    <span>{h.noteAr ?? h.toStatus} <span className="text-xs text-muted-foreground">· {new Date(h.at).toLocaleString("ar")}</span></span>
                  </div>
                ))}
              </Section>
            </div>

            {/* Decision */}
            {d.canAct && (
              <div className="space-y-2 border-t border-border p-4">
                <textarea value={comment} onChange={(e) => setComment(e.target.value)} placeholder="تعليق (مطلوب عند الرفض أو الإعادة)" rows={2} className="w-full border border-border bg-secondary px-3 py-2 text-sm" />
                <div className="flex items-center gap-2">
                  <button onClick={() => act("approve")} disabled={!!acting} className="inline-flex h-10 flex-1 items-center justify-center gap-2 bg-primary text-sm font-bold uppercase tracking-wider text-primary-foreground hover:bg-primary/80 disabled:opacity-50">
                    {acting === "approve" ? <Loader2 className="h-4 w-4 animate-spin" /> : <Check className="h-4 w-4" />} موافقة
                  </button>
                  <button onClick={() => act("return")} disabled={!!acting} className="inline-flex h-10 items-center justify-center gap-2 border border-orange-500/40 px-3 text-sm text-orange-400 hover:bg-orange-500/10 disabled:opacity-50">
                    {acting === "return" ? <Loader2 className="h-4 w-4 animate-spin" /> : <RotateCcw className="h-4 w-4" />} إعادة
                  </button>
                  <button onClick={() => act("reject")} disabled={!!acting} className="inline-flex h-10 items-center justify-center gap-2 border border-destructive/50 px-3 text-sm text-destructive hover:bg-destructive/10 disabled:opacity-50">
                    {acting === "reject" ? <Loader2 className="h-4 w-4 animate-spin" /> : <X className="h-4 w-4" />} رفض
                  </button>
                </div>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}

function Section({ title, children }: { title: string; children: ReactNode }) {
  return (
    <div>
      <p className="mb-2 text-xs font-bold uppercase tracking-wider text-muted-foreground">{title}</p>
      <div className="space-y-1.5">{children}</div>
    </div>
  );
}
function Kv({ k, v }: { k: string; v: string }) {
  return <div className="flex items-center justify-between text-sm"><span className="text-muted-foreground">{k}</span><span className="font-medium">{v}</span></div>;
}
