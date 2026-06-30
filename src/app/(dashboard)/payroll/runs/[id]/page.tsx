"use client";

import { use, useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { ArrowRight, Loader2, Calculator, ShieldCheck, Send, CheckCircle2, PlayCircle, XCircle, AlertTriangle } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { ApiError } from "@/lib/api-client";
import { AccessGuard } from "@/components/access/access-guard";
import { usePermissions } from "@/lib/permissions";
import { StateBadge } from "@/components/payroll/state-badge";
import {
  getRun, calculateRun, validateRun, submitRun, approveRun, executeRun, cancelRun, money,
  type PayrollRunDetail,
} from "@/lib/api/payroll";

function notifyError(err: unknown, fallback: string) {
  if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
    toast.error(err instanceof ApiError ? err.message : fallback);
  }
}

export default function RunDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  return <AccessGuard anyOf={["Payroll.View"]}><Inner id={id} /></AccessGuard>;
}

function Inner({ id }: { id: string }) {
  const { has } = usePermissions();
  const [run, setRun] = useState<PayrollRunDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);

  const load = useCallback(async () => {
    try { setRun(await getRun(id)); } catch (err) { notifyError(err, "تعذر تحميل المسيّر"); }
  }, [id]);

  useEffect(() => {
    (async () => { setLoading(true); try { setRun(await getRun(id)); } catch (err) { notifyError(err, "تعذر التحميل"); } finally { setLoading(false); } })();
  }, [id]);

  async function act(fn: () => Promise<PayrollRunDetail>, ok: string) {
    setBusy(true);
    try { setRun(await fn()); toast.success(ok); }
    catch (err) { notifyError(err, "تعذر تنفيذ العملية"); await load(); } finally { setBusy(false); }
  }

  if (loading) return <div className="py-20 text-center text-muted-foreground"><Loader2 className="h-5 w-5 animate-spin inline" /> جاري التحميل…</div>;
  if (!run) return <div className="py-20 text-center text-muted-foreground">المسيّر غير موجود</div>;

  const s = run.state;
  const errors = run.validation.filter((f) => f.severity === "Error");
  const warnings = run.validation.filter((f) => f.severity === "Warning");

  const canRun = has("Payroll.Run"), canApprove = has("Payroll.Approve"), canExec = has("Payroll.Lock");
  const preExec = ["Draft", "Preview", "Validated", "PendingApproval", "Approved"].includes(s);

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2 text-sm">
        <Link href="/payroll" className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1">
          <ArrowRight className="h-4 w-4" /> الرواتب
        </Link>
        <span className="text-muted-foreground">/</span><span dir="ltr">{run.runNumber}</span>
      </div>

      <div className="flex items-start justify-between flex-wrap gap-4">
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold" dir="ltr">{run.runNumber}</h1>
            <StateBadge state={s} />
          </div>
          <p className="text-sm text-muted-foreground mt-1" dir="ltr">{run.periodStart.slice(0, 10)} → {run.periodEnd.slice(0, 10)}</p>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          {canRun && (s === "Draft" || s === "Preview") && <Button onClick={() => act(() => calculateRun(id), "تم الاحتساب")} disabled={busy} variant="outline" className="gap-2"><Calculator className="h-4 w-4" /> احتساب</Button>}
          {canRun && (s === "Preview" || s === "Validated") && <Button onClick={() => act(() => validateRun(id), "تم التحقق")} disabled={busy} variant="outline" className="gap-2"><ShieldCheck className="h-4 w-4" /> تحقّق</Button>}
          {canRun && s === "Validated" && <Button onClick={() => act(() => submitRun(id), "تم الإرسال للاعتماد")} disabled={busy} variant="outline" className="gap-2"><Send className="h-4 w-4" /> إرسال للاعتماد</Button>}
          {canApprove && s === "PendingApproval" && <Button onClick={() => act(() => approveRun(id), "تم الاعتماد")} disabled={busy} className="gap-2 font-bold"><CheckCircle2 className="h-4 w-4" /> اعتماد</Button>}
          {canExec && (s === "Approved" || s === "Failed") && <Button onClick={() => act(() => executeRun(id), "تم تنفيذ المسيّر وترحيله للأستاذ")} disabled={busy} className="gap-2 font-bold"><PlayCircle className="h-4 w-4" /> تنفيذ وترحيل</Button>}
          {canRun && preExec && <Button onClick={() => act(() => cancelRun(id, "إلغاء يدوي"), "تم الإلغاء")} disabled={busy} variant="outline" className="gap-2 text-destructive border-destructive/30 hover:bg-destructive/10"><XCircle className="h-4 w-4" /> إلغاء</Button>}
        </div>
      </div>

      <div className="grid gap-3 sm:grid-cols-4">
        <Stat label="الموظفون" value={String(run.employeeCount)} />
        <Stat label="الإجمالي" value={money(run.grossTotal, run.currency)} />
        <Stat label="الاستقطاعات" value={money(run.deductionTotal, run.currency)} />
        <Stat label="الصافي" value={money(run.netTotal, run.currency)} accent />
      </div>

      {(errors.length > 0 || warnings.length > 0) && (
        <div className="border border-border bg-card p-4 space-y-2">
          <div className="flex items-center gap-2 text-sm font-bold"><AlertTriangle className="h-4 w-4 text-amber-600" /> نتائج التحقق</div>
          {errors.map((f, i) => <div key={`e${i}`} className="text-sm text-destructive">• {f.message}</div>)}
          {warnings.map((f, i) => <div key={`w${i}`} className="text-sm text-amber-600">• {f.message}</div>)}
        </div>
      )}

      <div className="border border-border">
        <div className="px-4 py-3 border-b border-border text-sm font-bold">قسائم الرواتب ({run.payslips.length})</div>
        <Table>
          <TableHeader>
            <TableRow className="border-border hover:bg-transparent">
              {["الموظف", "الرقم", "الإجمالي", "الاستقطاعات", "الصافي", "الترحيل"].map((h, i) => (
                <TableHead key={i} className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">{h}</TableHead>
              ))}
            </TableRow>
          </TableHeader>
          <TableBody>
            {run.payslips.length === 0 ? (
              <TableRow className="hover:bg-transparent"><TableCell colSpan={6} className="py-10 text-center text-sm text-muted-foreground">لا توجد قسائم — اضغط «احتساب»</TableCell></TableRow>
            ) : run.payslips.map((p) => (
              <PayslipRows key={p.id} payslip={p} currency={run.currency} />
            ))}
          </TableBody>
        </Table>
      </div>

      {run.transitions.length > 0 && (
        <div className="border border-border bg-card p-4">
          <div className="text-sm font-bold mb-2">سجل الحالة</div>
          <ol className="space-y-1 text-xs text-muted-foreground">
            {run.transitions.map((t, i) => (
              <li key={i} dir="ltr" className="flex justify-between">
                <span>{t.fromState} → {t.toState}{t.reason ? ` · ${t.reason}` : ""}</span>
                <span>{new Date(t.at).toLocaleString("ar-SA")}</span>
              </li>
            ))}
          </ol>
        </div>
      )}
    </div>
  );
}

function Stat({ label, value, accent }: { label: string; value: string; accent?: boolean }) {
  return (
    <div className="border border-border bg-card p-4">
      <div className="text-xs text-muted-foreground">{label}</div>
      <div className={`mt-1 text-lg font-bold tabular-nums ${accent ? "text-primary" : ""}`}>{value}</div>
    </div>
  );
}

interface PayslipComponent {
  Code: string;
  ComponentCode: string;
  Kind?: number | null;
  Amount: number;
  Applied?: boolean;
}

function PayslipRows({ payslip: p, currency }: { payslip: import("@/lib/api/payroll").PayslipDto; currency: string }) {
  const [expanded, setExpanded] = useState(false);

  const components: PayslipComponent[] = (() => {
    if (!p.componentsJson) return [];
    try {
      const parsed = JSON.parse(p.componentsJson) as { components?: PayslipComponent[] };
      return Array.isArray(parsed?.components) ? parsed.components : [];
    } catch { return []; }
  })();

  const txnLines = components.filter((c) => c.Code?.startsWith("TXN:"));

  return (
    <>
      <TableRow
        className="border-border hover:bg-card/50 cursor-pointer"
        onClick={() => txnLines.length > 0 && setExpanded((x) => !x)}
      >
        <TableCell className="font-medium">
          {txnLines.length > 0 && (
            <span className="mr-1 text-xs text-muted-foreground">{expanded ? "▼" : "▶"}</span>
          )}
          {p.employeeName}
        </TableCell>
        <TableCell className="text-sm text-muted-foreground" dir="ltr">{p.employeeNumber}</TableCell>
        <TableCell className="text-sm tabular-nums">{money(p.grossEarnings, p.currency || currency)}</TableCell>
        <TableCell className="text-sm tabular-nums">{money(p.totalDeductions, p.currency || currency)}</TableCell>
        <TableCell className="text-sm tabular-nums font-medium">{money(p.netAmount, p.currency || currency)}</TableCell>
        <TableCell>{p.ledgerPosted ? <CheckCircle2 className="h-4 w-4 text-green-600" /> : <span className="text-muted-foreground">—</span>}</TableCell>
      </TableRow>
      {expanded && txnLines.map((c, i) => (
        <TableRow key={i} className="border-border bg-muted/30 hover:bg-muted/40">
          <TableCell colSpan={2} className="pr-8 text-xs text-muted-foreground">
            {c.ComponentCode || c.Code}
          </TableCell>
          <TableCell colSpan={4} className="text-xs tabular-nums text-muted-foreground">
            {money(c.Amount, p.currency || currency)}
          </TableCell>
        </TableRow>
      ))}
    </>
  );
}
