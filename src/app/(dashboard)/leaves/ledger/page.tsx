"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { Loader2, RefreshCw, GitCommitVertical } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Combobox } from "@/components/ui/combobox";
import { usePermissions } from "@/lib/permissions";
import { getEmployees } from "@/lib/api/employees";
import { getMasterDataItems, type MasterDataItem } from "@/lib/api/master-data";
import { getLeaveLedger, recalculateLedger, type LeaveLedgerView } from "@/lib/api/leave-ledger";
import { LeaveLedgerTimeline } from "@/components/leaves/leave-ledger-timeline";

function Stat({ label, value, accent }: { label: string; value: string; accent?: boolean }) {
  return (
    <div className="border border-border bg-card p-4">
      <div className="text-xs text-muted-foreground">{label}</div>
      <div className={`mt-1 font-heading text-2xl tabular-nums ${accent ? "text-primary" : "text-foreground"}`}>{value}</div>
    </div>
  );
}

export default function LeaveLedgerPage() {
  const { hasAny } = usePermissions();
  const canRecalc = hasAny("Leaves.Edit", "Employees.Edit");

  const [employees, setEmployees] = useState<{ value: string; label: string; hint?: string }[]>([]);
  const [leaveTypes, setLeaveTypes] = useState<MasterDataItem[]>([]);
  const [employeeId, setEmployeeId] = useState<string | null>(null);
  const [leaveTypeId, setLeaveTypeId] = useState<string | null>(null);

  const [ledger, setLedger] = useState<LeaveLedgerView | null>(null);
  const [loading, setLoading] = useState(false);
  const [recalcing, setRecalcing] = useState(false);

  useEffect(() => {
    getEmployees({ pageSize: 500 })
      .then((list) => setEmployees(list.map((e) => ({ value: e.id, label: e.name, hint: e.employeeId }))))
      .catch(() => {});
    getMasterDataItems("LeaveType")
      .then((types) => {
        setLeaveTypes(types);
        const annual = types.find((t) => t.code === "ANNUAL") ?? types[0];
        if (annual) setLeaveTypeId(annual.id);
      })
      .catch(() => {});
  }, []);

  const leaveTypeOptions = useMemo(
    () => leaveTypes.map((t) => ({ value: t.id, label: t.nameAr || t.nameEn || t.code })),
    [leaveTypes],
  );

  const load = useCallback(() => {
    if (!employeeId) { setLedger(null); return; }
    setLoading(true);
    getLeaveLedger(employeeId, leaveTypeId ?? undefined)
      .then(setLedger)
      .catch((e) => { toast.error((e as Error)?.message || "تعذر تحميل السجل"); setLedger(null); })
      .finally(() => setLoading(false));
  }, [employeeId, leaveTypeId]);
  useEffect(() => { load(); }, [load]);

  async function doRecalc() {
    if (!employeeId) return;
    setRecalcing(true);
    try {
      const view = await recalculateLedger(employeeId, leaveTypeId ?? undefined);
      setLedger(view);
      toast.success("تم إعادة احتساب دفتر الاستحقاق");
    } catch (e) {
      toast.error((e as Error)?.message || "تعذر إعادة الاحتساب");
    } finally {
      setRecalcing(false);
    }
  }

  return (
    <div className="mx-auto max-w-4xl space-y-6 p-6">
      {/* Header */}
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="flex items-center gap-2 font-heading text-2xl text-foreground">
            <GitCommitVertical className="h-6 w-6 text-primary" />
            دفتر استحقاق الإجازات
          </h1>
          <p className="mt-1 text-sm text-muted-foreground">
            سجل زمني للاستحقاق والاستخدام مع إعادة الاحتساب عند احتساب الإجازات بدون راتب.
          </p>
        </div>
        {canRecalc && employeeId && (
          <Button variant="outline" onClick={doRecalc} disabled={recalcing}>
            {recalcing ? <Loader2 className="h-4 w-4 animate-spin" /> : <RefreshCw className="h-4 w-4" />}
            إعادة الاحتساب
          </Button>
        )}
      </div>

      {/* Pickers */}
      <div className="grid gap-3 sm:grid-cols-2">
        <div>
          <label className="mb-1 block text-xs text-muted-foreground">الموظف</label>
          <Combobox value={employeeId} onChange={setEmployeeId} options={employees} placeholder="اختر موظفاً…" />
        </div>
        <div>
          <label className="mb-1 block text-xs text-muted-foreground">نوع الإجازة</label>
          <Combobox value={leaveTypeId} onChange={setLeaveTypeId} options={leaveTypeOptions} placeholder="نوع الإجازة" allowClear={false} />
        </div>
      </div>

      {/* Body */}
      {!employeeId ? (
        <div className="border border-dashed border-border bg-card px-6 py-16 text-center text-sm text-muted-foreground">
          اختر موظفاً لعرض دفتر استحقاق إجازاته.
        </div>
      ) : loading ? (
        <div className="flex items-center justify-center py-20 text-muted-foreground">
          <Loader2 className="h-6 w-6 animate-spin" />
        </div>
      ) : ledger ? (
        <>
          <div className="grid gap-3 sm:grid-cols-3">
            <Stat label="الرصيد الحالي" value={ledger.currentBalance.toLocaleString("ar-SA", { maximumFractionDigits: 2 })} accent />
            <Stat label="إجمالي الاستحقاق" value={ledger.accruedToDate.toLocaleString("ar-SA", { maximumFractionDigits: 2 })} />
            <Stat label="إجمالي الاستخدام" value={ledger.usedToDate.toLocaleString("ar-SA", { maximumFractionDigits: 2 })} />
          </div>
          <div className="border border-border bg-card p-5">
            <LeaveLedgerTimeline ledger={ledger} />
          </div>
        </>
      ) : null}
    </div>
  );
}
