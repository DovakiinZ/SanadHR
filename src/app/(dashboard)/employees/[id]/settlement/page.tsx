"use client";

import { use, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { ArrowRight, Loader2, Scale, AlertTriangle } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Combobox } from "@/components/ui/combobox";
import { usePermissions } from "@/lib/permissions";
import { getEmployee } from "@/lib/api/employees";
import type { Employee } from "@/types";
import {
  previewSettlement,
  SCENARIO_AR, SCENARIO_OPTIONS, CONTRACT_TERM_AR,
  type SettlementResult, type SettlementInput, type TerminationScenario, type ContractTermType,
} from "@/lib/api/settlement";
import { requestTermination } from "@/lib/api/terminations";

const inputCls = "h-9 w-full rounded-lg border border-input bg-secondary px-3 text-sm outline-none focus-visible:border-ring";

function money(n: number, currency = "SAR"): string {
  return `${n.toLocaleString("ar-SA", { maximumFractionDigits: 2, minimumFractionDigits: 2 })} ${currency}`;
}

export default function SettlementPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const { hasAny } = usePermissions();
  const canTerminate = hasAny("Employees.Terminate");

  const [employee, setEmployee] = useState<Employee | null>(null);
  const [scenario, setScenario] = useState<TerminationScenario>("NormalEmployerTermination");
  const [termType, setTermType] = useState<ContractTermType>("Indefinite");
  const [terminationDate, setTerminationDate] = useState(() => new Date().toISOString().slice(0, 10));
  const [contractEndDate, setContractEndDate] = useState("");
  const [notes, setNotes] = useState("");

  const [preview, setPreview] = useState<SettlementResult | null>(null);
  const [previewing, setPreviewing] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [submitted, setSubmitted] = useState<{ id: string } | null>(null);

  useEffect(() => { getEmployee(id).then(setEmployee).catch(() => {}); }, [id]);

  const input = useMemo<SettlementInput>(() => ({
    terminationDate,
    scenario,
    contractTermType: termType,
    contractEndDate: termType === "FixedTerm" && contractEndDate ? contractEndDate : null,
    notes: notes || null,
  }), [terminationDate, scenario, termType, contractEndDate, notes]);

  // Live preview (debounced) whenever the inputs change. setState is done inside the timeout/promise
  // callbacks (not synchronously in the effect body) to keep renders clean.
  useEffect(() => {
    if (submitted) return; // already submitted for approval
    let active = true;
    if (!terminationDate) { const c = setTimeout(() => active && setPreview(null), 0); return () => { active = false; clearTimeout(c); }; }
    const t = setTimeout(() => {
      if (!active) return;
      setPreviewing(true);
      previewSettlement(id, input)
        .then((r) => { if (active) setPreview(r); })
        .catch(() => { if (active) setPreview(null); })
        .finally(() => { if (active) setPreviewing(false); });
    }, 350);
    return () => { active = false; clearTimeout(t); };
  }, [id, input, terminationDate, submitted]);

  async function doTerminate() {
    if (!canTerminate) return;
    if (!confirm("سيتم تقديم طلب إنهاء الخدمة للاعتماد (المدير → الموارد البشرية → المالية). متابعة؟")) return;
    setSubmitting(true);
    try {
      const r = await requestTermination({
        employeeId: id,
        terminationDate,
        scenario,
        contractTermType: termType,
        contractEndDate: termType === "FixedTerm" && contractEndDate ? contractEndDate : null,
        notes: notes || null,
      });
      setSubmitted({ id: r.id });
      toast.success("تم تقديم طلب إنهاء الخدمة للاعتماد");
    } catch (e) {
      toast.error((e as Error)?.message || "تعذر تقديم الطلب");
    } finally {
      setSubmitting(false);
    }
  }

  const result = preview;

  return (
    <div className="mx-auto max-w-3xl space-y-6 p-6">
      {/* Header */}
      <div className="flex items-center justify-between gap-4">
        <div>
          <h1 className="flex items-center gap-2 font-heading text-2xl text-foreground">
            <Scale className="h-6 w-6 text-primary" />
            مخالصة نهاية الخدمة
          </h1>
          <p className="mt-1 text-sm text-muted-foreground">
            {employee ? employee.name : "…"} · احتساب وفق نظام العمل السعودي (المواد 77 / 80 / 81 على مكافأة 84 / 85)
          </p>
        </div>
        <Link href={`/employees/${id}`} className="text-sm text-muted-foreground hover:text-foreground">
          <span className="inline-flex items-center gap-1"><ArrowRight className="h-4 w-4" /> الملف</span>
        </Link>
      </div>

      {submitted && (
        <div className="border border-primary/30 bg-primary/5 px-4 py-3 text-sm text-foreground">
          تم تقديم طلب إنهاء الخدمة للاعتماد. سيمر عبر سلسلة الموافقات (المدير → الموارد البشرية → المالية)، وعند الاعتماد النهائي تُنشأ مخالصة مالية في المصروفات ويُولّد مستند المخالصة.{" "}
          <Link href="/employees/terminations" className="font-medium text-primary hover:underline">عرض طلبات الإنهاء</Link>
        </div>
      )}

      {/* Form */}
      <div className="grid gap-4 border border-border bg-card p-5 sm:grid-cols-2">
        <div>
          <label className="mb-1 block text-xs text-muted-foreground">سبب إنهاء الخدمة</label>
          <Combobox
            value={scenario}
            onChange={(v) => v && setScenario(v as TerminationScenario)}
            options={SCENARIO_OPTIONS.map((s) => ({ value: s, label: SCENARIO_AR[s] }))}
            allowClear={false}
            placeholder="اختر السبب"
          />
        </div>
        <div>
          <label className="mb-1 block text-xs text-muted-foreground">نوع العقد</label>
          <Combobox
            value={termType}
            onChange={(v) => v && setTermType(v as ContractTermType)}
            options={(Object.keys(CONTRACT_TERM_AR) as ContractTermType[]).map((t) => ({ value: t, label: CONTRACT_TERM_AR[t] }))}
            allowClear={false}
          />
        </div>
        <div>
          <label className="mb-1 block text-xs text-muted-foreground">تاريخ انتهاء الخدمة</label>
          <input type="date" value={terminationDate} onChange={(e) => setTerminationDate(e.target.value)} className={inputCls} disabled={!!submitted} />
        </div>
        {termType === "FixedTerm" && (
          <div>
            <label className="mb-1 block text-xs text-muted-foreground">تاريخ نهاية العقد (للمادة 77)</label>
            <input type="date" value={contractEndDate} onChange={(e) => setContractEndDate(e.target.value)} className={inputCls} disabled={!!submitted} />
          </div>
        )}
        <div className="sm:col-span-2">
          <label className="mb-1 block text-xs text-muted-foreground">ملاحظات</label>
          <textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={2} className="w-full rounded-lg border border-input bg-secondary px-3 py-2 text-sm outline-none focus-visible:border-ring" disabled={!!submitted} />
        </div>
      </div>

      {/* Breakdown */}
      <div className="border border-border bg-card p-5">
        <div className="mb-3 flex items-center justify-between">
          <h2 className="font-heading text-lg text-foreground">تفصيل المستحقات</h2>
          {previewing && !submitted && <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />}
        </div>

        {!result ? (
          <div className="py-10 text-center text-sm text-muted-foreground">أدخل البيانات لعرض الاحتساب.</div>
        ) : (
          <>
            <div className="mb-4 grid grid-cols-3 gap-3 text-center">
              <Meta label="الأجر الشهري" value={money(result.monthlyWage, result.currency)} />
              <Meta label="سنوات الخدمة" value={result.serviceYears.toLocaleString("ar-SA", { maximumFractionDigits: 2 })} />
              <Meta label="أيام بدون راتب" value={result.unpaidLeaveDays.toLocaleString("ar-SA", { maximumFractionDigits: 0 })} />
            </div>

            <ul className="divide-y divide-border">
              {result.lines.map((l, i) => (
                <li key={i} className="flex items-center justify-between gap-3 py-2.5 text-sm">
                  <div className="flex items-center gap-2">
                    <span className="inline-flex items-center rounded bg-primary/10 px-1.5 py-0.5 text-[10px] font-medium text-primary">{l.articleRef}</span>
                    <span className="text-foreground">{l.labelAr}</span>
                  </div>
                  <span className="shrink-0 tabular-nums text-foreground">{money(l.amount, result.currency)}</span>
                </li>
              ))}
            </ul>

            <div className="mt-3 flex items-center justify-between border-t-2 border-foreground/80 pt-3">
              <span className="font-heading text-base text-foreground">الإجمالي المستحق</span>
              <span className="font-heading text-xl tabular-nums text-primary">{money(result.totalAward, result.currency)}</span>
            </div>

            {scenario === "Article80ForCause" && (
              <div className="mt-3 flex items-center gap-2 text-xs text-muted-foreground">
                <AlertTriangle className="h-4 w-4 text-amber-600" />
                الفصل لسبب مشروع (مادة 80): لا تُستحق مكافأة نهاية الخدمة ولا تعويض الإشعار.
              </div>
            )}
          </>
        )}
      </div>

      {/* Action */}
      {!submitted && (
        <div className="flex justify-end">
          <Button onClick={doTerminate} disabled={!canTerminate || submitting || !result} variant="destructive">
            {submitting ? <Loader2 className="h-4 w-4 animate-spin" /> : <Scale className="h-4 w-4" />}
            تقديم طلب إنهاء الخدمة للاعتماد
          </Button>
        </div>
      )}
    </div>
  );
}

function Meta({ label, value }: { label: string; value: string }) {
  return (
    <div className="border border-border bg-secondary p-2.5">
      <div className="text-[11px] text-muted-foreground">{label}</div>
      <div className="mt-0.5 text-sm font-medium tabular-nums text-foreground">{value}</div>
    </div>
  );
}
