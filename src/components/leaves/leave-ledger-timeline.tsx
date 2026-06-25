"use client";

import { useMemo } from "react";
import type { LeaveLedgerView } from "@/lib/api/leave-ledger";
import { LEDGER_TYPE_AR } from "@/lib/api/leave-ledger";

/**
 * Vertical editorial timeline of the leave accrual ledger.
 * - Each transaction is a dot on a vertical stem (no horizontal flowcharts).
 * - Solid terracotta stem for active accrual; dotted/grey for unpaid-leave periods.
 * - Hover a dot to reveal the exact fractional delta at that point in time.
 */

type TimelineItem =
  | { kind: "entry"; date: string; type: string; amount: number; running: number; reason?: string | null; unpaid: boolean }
  | { kind: "gap"; date: string; end: string; days: number };

function fmt(n: number, max = 2): string {
  return n.toLocaleString("ar-SA", { minimumFractionDigits: 0, maximumFractionDigits: max });
}

function fmtSigned(n: number): string {
  const s = n.toLocaleString("ar-SA", { minimumFractionDigits: 0, maximumFractionDigits: 3 });
  return n > 0 ? `+${s}` : s;
}

function fmtDate(d: string): string {
  return new Date(d).toLocaleDateString("ar-SA", { year: "numeric", month: "short", day: "numeric" });
}

export function LeaveLedgerTimeline({ ledger }: { ledger: LeaveLedgerView }) {
  const items = useMemo<TimelineItem[]>(() => {
    const list: TimelineItem[] = ledger.entries.map((e) => ({
      kind: "entry" as const, date: e.date, type: e.type, amount: e.amount,
      running: e.runningBalance, reason: e.reason, unpaid: e.isUnpaidPeriod,
    }));
    for (const g of ledger.unpaidPeriods) list.push({ kind: "gap", date: g.start, end: g.end, days: g.days });
    return list.sort((a, b) => +new Date(a.date) - +new Date(b.date));
  }, [ledger]);

  if (items.length === 0) {
    return (
      <div className="border border-dashed border-border bg-card px-6 py-16 text-center text-sm text-muted-foreground">
        لا توجد حركات في سجل الإجازة بعد. أعد الاحتساب لبناء دفتر الاستحقاق.
      </div>
    );
  }

  return (
    <ol className="relative">
      {items.map((it, i) => {
        const last = i === items.length - 1;
        return (
          <li key={i} className="relative flex gap-4 pb-6">
            {/* Stem + dot column */}
            <div className="relative flex w-4 shrink-0 flex-col items-center">
              <Dot item={it} />
              {!last && <Stem item={it} />}
            </div>

            {/* Content */}
            <div className="-mt-0.5 flex-1">
              {it.kind === "gap" ? (
                <div className="text-sm">
                  <div className="font-medium text-muted-foreground">إجازة بدون راتب — يتوقف الاستحقاق</div>
                  <div className="mt-0.5 text-xs text-muted-foreground/80 tabular-nums">
                    {fmtDate(it.date)} ← {fmtDate(it.end)} · {fmt(it.days)} يوم
                  </div>
                </div>
              ) : (
                <div
                  className="group flex items-start justify-between gap-3 text-sm"
                  title={`الحركة: ${fmtSigned(it.amount)} يوم`}
                >
                  <div>
                    <div className="font-medium text-foreground">
                      {LEDGER_TYPE_AR[it.type] ?? it.type}
                      <span
                        className={`ms-2 tabular-nums ${it.amount >= 0 ? "text-primary" : "text-destructive"}`}
                      >
                        {fmtSigned(it.amount)}
                      </span>
                    </div>
                    <div className="mt-0.5 text-xs text-muted-foreground tabular-nums">
                      {fmtDate(it.date)}
                      {it.reason ? <span className="mx-1">·</span> : null}
                      {it.reason}
                    </div>
                  </div>
                  <div className="shrink-0 text-end">
                    <div className="font-heading text-base tabular-nums text-foreground">{fmt(it.running)}</div>
                    <div className="text-[10px] text-muted-foreground">الرصيد</div>
                  </div>
                </div>
              )}
            </div>
          </li>
        );
      })}
    </ol>
  );
}

function Dot({ item }: { item: TimelineItem }) {
  if (item.kind === "gap")
    return <span className="z-10 mt-1 h-2.5 w-2.5 rounded-full border-2 border-dashed border-muted-foreground/50 bg-background" />;
  const accrual = item.type === "Accrual";
  const positive = item.amount >= 0;
  return (
    <span
      className={`z-10 mt-1 h-2.5 w-2.5 rounded-full ${
        accrual ? "bg-primary" : positive ? "bg-primary/50" : "border border-destructive/60 bg-background"
      }`}
    />
  );
}

function Stem({ item }: { item: TimelineItem }) {
  // Dotted/grey while unpaid (accrual paused); solid terracotta during active accrual.
  const dotted = item.kind === "gap" || (item.kind === "entry" && item.unpaid);
  return (
    <span
      className={`mt-1 w-px flex-1 ${
        dotted
          ? "border-s border-dashed border-muted-foreground/40"
          : "bg-primary/30"
      }`}
    />
  );
}
