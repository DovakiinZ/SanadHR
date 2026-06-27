"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { ArrowRight, Loader2, Scale, CheckCircle2, XCircle, FileDown, Wallet } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { ApiError } from "@/lib/api-client";
import {
  getPendingTerminations, decideTermination, fileUrl,
  ROLE_AR, STEP_STATUS_AR, SETTLEMENT_STATUS_AR, type TerminationDto,
} from "@/lib/api/terminations";

function notifyError(err: unknown, fallback: string) {
  if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
    toast.error(err instanceof ApiError ? err.message : fallback);
  }
}

function money(n: number, c = "SAR") { return `${n.toLocaleString("ar-SA", { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ${c}`; }

export default function TerminationApprovalsPage() {
  const [items, setItems] = useState<TerminationDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [busyId, setBusyId] = useState<string | null>(null);
  const [justApproved, setJustApproved] = useState<TerminationDto | null>(null);

  const load = useCallback(async () => {
    try { setItems(await getPendingTerminations()); } catch (err) { notifyError(err, "تعذر تحميل الطلبات"); }
  }, []);

  useEffect(() => {
    (async () => { setLoading(true); try { setItems(await getPendingTerminations()); } catch (err) { notifyError(err, "تعذر التحميل"); } finally { setLoading(false); } })();
  }, []);

  async function decide(t: TerminationDto, approve: boolean) {
    const comment = approve ? undefined : (prompt("سبب الرفض (اختياري)") ?? undefined);
    setBusyId(t.id);
    try {
      const updated = await decideTermination(t.id, approve, comment);
      if (approve && updated.status === "Approved") { setJustApproved(updated); toast.success("تم الاعتماد النهائي — أُنشئت المخالصة في المصروفات وتولّد المستند"); }
      else if (approve) toast.success("تمت الموافقة على خطوتك");
      else toast.success("تم رفض الطلب");
      await load();
    } catch (err) { notifyError(err, "تعذر تنفيذ القرار"); } finally { setBusyId(null); }
  }

  return (
    <div className="space-y-6 p-1">
      <div className="flex items-center gap-2 text-sm">
        <Link href="/employees" className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1">
          <ArrowRight className="h-4 w-4" /> الموظفون
        </Link>
        <span className="text-muted-foreground">/</span><span>اعتماد إنهاء الخدمة</span>
      </div>

      <div>
        <h1 className="flex items-center gap-2 text-2xl font-bold"><Scale className="h-6 w-6 text-primary" /> اعتماد إنهاء الخدمة</h1>
        <p className="text-sm text-muted-foreground mt-1">طلبات إنهاء الخدمة بانتظار قرارك (المدير → الموارد البشرية → المالية)</p>
      </div>

      {justApproved && (
        <div className="border border-green-500/30 bg-green-500/5 p-4 text-sm space-y-2">
          <div className="font-bold text-green-700 flex items-center gap-2"><CheckCircle2 className="h-4 w-4" /> تم الاعتماد النهائي لمخالصة {justApproved.employeeName}</div>
          <div className="flex flex-wrap items-center gap-3">
            <span className="inline-flex items-center gap-1 text-muted-foreground"><Wallet className="h-4 w-4" /> أُنشئت مصروفة بمبلغ {money(justApproved.totalAward, justApproved.currency)} (بانتظار اعتماد المصروفات)</span>
            {justApproved.documentFileId && (
              <a href={fileUrl(justApproved.documentFileId)} target="_blank" rel="noreferrer" className="inline-flex items-center gap-1 text-primary hover:underline">
                <FileDown className="h-4 w-4" /> تنزيل مستند المخالصة
              </a>
            )}
          </div>
        </div>
      )}

      {loading ? (
        <div className="py-16 text-center text-muted-foreground"><Loader2 className="h-5 w-5 animate-spin inline" /> جاري التحميل…</div>
      ) : items.length === 0 ? (
        <div className="border border-dashed border-border p-12 text-center text-sm text-muted-foreground">لا توجد طلبات إنهاء بانتظار قرارك.</div>
      ) : (
        <div className="space-y-4">
          {items.map((t) => {
            const current = t.steps.find((s) => s.order === t.currentStep);
            return (
              <div key={t.id} className="border border-border bg-card p-5">
                <div className="flex items-start justify-between flex-wrap gap-3">
                  <div>
                    <div className="font-bold">{t.employeeName} <span className="text-sm text-muted-foreground" dir="ltr">({t.employeeNumber})</span></div>
                    <div className="text-sm text-muted-foreground mt-0.5">إجمالي المستحق: <span className="font-medium text-foreground tabular-nums">{money(t.totalAward, t.currency)}</span> · سنوات الخدمة {t.serviceYears.toFixed(2)}</div>
                  </div>
                  <Badge variant="outline" className="text-xs">{SETTLEMENT_STATUS_AR[t.status] ?? t.status}</Badge>
                </div>

                <div className="mt-3 flex flex-wrap items-center gap-2">
                  {t.steps.map((s) => (
                    <span key={s.order} className={`inline-flex items-center gap-1 px-2 h-7 text-xs border ${
                      s.status === "Approved" ? "bg-green-500/10 text-green-600 border-green-500/20"
                      : s.status === "Rejected" ? "bg-destructive/10 text-destructive border-destructive/20"
                      : s.order === t.currentStep ? "bg-amber-500/10 text-amber-600 border-amber-500/20"
                      : "text-muted-foreground border-border"}`}>
                      {ROLE_AR[s.role] ?? s.role}: {STEP_STATUS_AR[s.status] ?? s.status}
                    </span>
                  ))}
                </div>

                <div className="mt-4 flex items-center justify-between gap-2">
                  <span className="text-xs text-muted-foreground">الخطوة الحالية: {current ? (ROLE_AR[current.role] ?? current.role) : "—"}</span>
                  <div className="flex items-center gap-2">
                    <Button onClick={() => decide(t, false)} disabled={busyId === t.id} variant="outline" className="gap-2 text-destructive border-destructive/30 hover:bg-destructive/10 h-9">
                      <XCircle className="h-4 w-4" /> رفض
                    </Button>
                    <Button onClick={() => decide(t, true)} disabled={busyId === t.id} className="gap-2 font-bold h-9">
                      {busyId === t.id ? <Loader2 className="h-4 w-4 animate-spin" /> : <CheckCircle2 className="h-4 w-4" />} موافقة
                    </Button>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
