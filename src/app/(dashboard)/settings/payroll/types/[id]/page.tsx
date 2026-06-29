"use client";

import { useCallback, useEffect, useState } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import {
  ArrowRight, Loader2, Copy, Send, Play, Save, Eye, CheckCircle2,
  Clock, XCircle, ChevronRight,
} from "lucide-react";
import { toast } from "sonner";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle,
} from "@/components/ui/dialog";
import { AccessGuard } from "@/components/access/access-guard";
import { usePermissions } from "@/lib/permissions";
import { getMasterDataItems, type MasterDataItem } from "@/lib/api/master-data";
import { ApiError } from "@/lib/api-client";
import {
  payrollTypesApi,
  type PayrollTypeDetail,
  type PayrollVersion,
} from "@/lib/api/payroll-types";
import type { PayrollPreviewDto } from "@/lib/api/payroll";
import { ScopeBuilder } from "@/components/payroll/scope-builder";

// ── helpers ────────────────────────────────────────────────────────────────
function notifyError(err: unknown, fallback: string) {
  if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
    toast.error(err instanceof ApiError ? err.message : fallback);
  }
}

const STATUS_AR: Record<string, string> = {
  Active: "نشط", Inactive: "غير نشط", Draft: "مسودة", Archived: "مؤرشف",
};

function statusBadge(status: string) {
  const label = STATUS_AR[status] ?? status;
  const cls =
    status === "Active" ? "bg-green-500/10 text-green-500 border-green-500/20" :
    status === "Draft"  ? "bg-yellow-500/10 text-yellow-500 border-yellow-500/20" :
    "bg-zinc-500/10 text-zinc-400 border-zinc-500/20";
  return <Badge variant="outline" className={`text-xs ${cls}`}>{label}</Badge>;
}

const VERSION_STATUS_AR: Record<string, string> = {
  Draft: "مسودة", Published: "منشور", Superseded: "مستبدل",
};

function versionBadge(status: string) {
  const label = VERSION_STATUS_AR[status] ?? status;
  const cls =
    status === "Published"  ? "bg-green-500/10 text-green-500 border-green-500/20" :
    status === "Draft"      ? "bg-yellow-500/10 text-yellow-500 border-yellow-500/20" :
    "bg-zinc-500/10 text-zinc-400 border-zinc-500/20";
  return <Badge variant="outline" className={`text-xs ${cls}`}>{label}</Badge>;
}

const FREQUENCY_OPTIONS = ["Monthly", "BiWeekly", "Weekly", "SemiMonthly", "Quarterly", "Annual"];
const FREQUENCY_AR: Record<string, string> = {
  Monthly: "شهري", BiWeekly: "كل أسبوعين", Weekly: "أسبوعي",
  SemiMonthly: "نصف شهري", Quarterly: "ربع سنوي", Annual: "سنوي",
};
const DAY_BASIS_OPTIONS = ["CalendarMonth", "Fixed30", "WorkingDays"];
const DAY_BASIS_AR: Record<string, string> = {
  CalendarMonth: "أيام الشهر التقويمي", Fixed30: "30 يوماً ثابتة", WorkingDays: "أيام العمل",
};

const CALC_TOGGLES: Array<{ key: keyof CalcSettings; label: string }> = [
  { key: "includeAllowances",            label: "تضمين البدلات" },
  { key: "additionsInGross",             label: "الإضافات في الإجمالي" },
  { key: "includeAdditions",             label: "تضمين الإضافات" },
  { key: "includeDeductions",            label: "تضمين الاستقطاعات" },
  { key: "includeAttendanceDeductions",  label: "استقطاعات الحضور" },
  { key: "includeLoans",                 label: "تضمين السلف" },
  { key: "includeGosi",                  label: "تضمين GOSI" },
  { key: "includeUnpaidLeave",           label: "تضمين الإجازة بدون راتب" },
  { key: "includeOvertime",              label: "تضمين الإضافي" },
];

interface CalcSettings {
  includeAllowances: boolean;
  additionsInGross: boolean;
  includeAdditions: boolean;
  includeDeductions: boolean;
  includeAttendanceDeductions: boolean;
  includeLoans: boolean;
  includeGosi: boolean;
  includeUnpaidLeave: boolean;
  includeOvertime: boolean;
}

const DEFAULT_CALC: CalcSettings = {
  includeAllowances: true, additionsInGross: true, includeAdditions: true,
  includeDeductions: true, includeAttendanceDeductions: true, includeLoans: true,
  includeGosi: true, includeUnpaidLeave: true, includeOvertime: true,
};

function parseCalcSettings(json: string | null): CalcSettings {
  if (!json) return DEFAULT_CALC;
  try { return { ...DEFAULT_CALC, ...(JSON.parse(json) as Partial<CalcSettings>) }; }
  catch { return DEFAULT_CALC; }
}

// ── Root page with AccessGuard ──────────────────────────────────────────────
export default function PayrollTypeDetailPage() {
  return (
    <AccessGuard anyOf={["Payroll.View"]}>
      <Inner />
    </AccessGuard>
  );
}

// ── Tabs ────────────────────────────────────────────────────────────────────
type Tab = "general" | "scope" | "calc" | "cutoff";

// ── Inner component ─────────────────────────────────────────────────────────
function Inner() {
  const { id } = useParams<{ id: string }>();
  const { has } = usePermissions();
  const canConfigure = has("Payroll.Configure");

  // Data
  const [detail, setDetail] = useState<PayrollTypeDetail | null>(null);
  const [loading, setLoading] = useState(true);

  // Lookups
  const [categories, setCategories] = useState<MasterDataItem[]>([]);
  const [paymentMethods, setPaymentMethods] = useState<MasterDataItem[]>([]);
  const [exportFormats, setExportFormats] = useState<MasterDataItem[]>([]);
  const [allowanceTypes, setAllowanceTypes] = useState<MasterDataItem[]>([]);

  // Header edit
  const [headerName, setHeaderName] = useState("");
  const [headerNameAr, setHeaderNameAr] = useState("");
  const [headerCategory, setHeaderCategory] = useState("");
  const [headerStatus, setHeaderStatus] = useState("");
  const [savingHeader, setSavingHeader] = useState(false);

  // Version state
  const [selectedVid, setSelectedVid] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<Tab>("general");

  // Draft version fields
  const [vFrequency, setVFrequency] = useState("");
  const [vCurrency, setVCurrency] = useState("SAR");
  const [vPaymentMethodId, setVPaymentMethodId] = useState("");
  const [vExportFormatId, setVExportFormatId] = useState("");
  const [vApprovalWorkflowId, setVApprovalWorkflowId] = useState("");
  const [vRuleSetVersionId, setVRuleSetVersionId] = useState("");
  const [vScopeJson, setVScopeJson] = useState<string | null>(null);
  const [vDayBasis, setVDayBasis] = useState("CalendarMonth");
  const [vCalcSettings, setVCalcSettings] = useState<CalcSettings>(DEFAULT_CALC);
  const [vExcludedAllowances, setVExcludedAllowances] = useState<string[]>([]);
  const [vCutoffDay, setVCutoffDay] = useState(25);
  const [vClosingDate, setVClosingDate] = useState("");
  const [vPaymentDate, setVPaymentDate] = useState("");
  const [vCarry, setVCarry] = useState(false);
  const [savingVersion, setSavingVersion] = useState(false);

  // Simulate dialog
  const [simDialogVid, setSimDialogVid] = useState<string | null>(null);
  const [simYear, setSimYear] = useState(new Date().getFullYear());
  const [simMonth, setSimMonth] = useState(new Date().getMonth() + 1);
  const [simRunning, setSimRunning] = useState(false);
  const [simResult, setSimResult] = useState<PayrollPreviewDto | null>(null);

  // Clone/Publish busy
  const [actingVid, setActingVid] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const [d, cats, pm, ef, at] = await Promise.all([
        payrollTypesApi.get(id),
        getMasterDataItems("PayrollTypeCategory"),
        getMasterDataItems("PaymentMethod"),
        getMasterDataItems("PayrollExportFormat"),
        getMasterDataItems("AllowanceType"),
      ]);
      setDetail(d);
      setCategories(cats ?? []);
      setPaymentMethods(pm ?? []);
      setExportFormats(ef ?? []);
      setAllowanceTypes(at ?? []);

      // Init header fields
      setHeaderName(d.name);
      setHeaderNameAr(d.nameAr ?? "");
      setHeaderCategory(d.categoryId ?? "");
      setHeaderStatus(d.status);

      // If there's a current version, select it
      if (d.currentVersionId) {
        selectVersion(d.currentVersionId, d.versions);
      } else if (d.versions.length > 0) {
        selectVersion(d.versions[0].id, d.versions);
      }
    } catch (err) {
      notifyError(err, "تعذر تحميل بيانات النوع");
    } finally {
      setLoading(false);
    }
  }, [id]);

  function selectVersion(vid: string, versions: PayrollVersion[]) {
    const v = versions.find((x) => x.id === vid);
    if (!v) return;
    setSelectedVid(vid);
    setVFrequency(v.frequency ?? "Monthly");
    setVCurrency(v.currency ?? "SAR");
    setVPaymentMethodId(v.paymentMethodId ?? "");
    setVExportFormatId(v.defaultExportFormatId ?? "");
    setVApprovalWorkflowId(v.approvalWorkflowId ?? "");
    setVRuleSetVersionId(v.ruleSetVersionId ?? "");
    setVScopeJson(v.selectionScopeJson);
    setVDayBasis(v.dayBasis ?? "CalendarMonth");
    // Restore calcSettings toggles AND excludedAllowanceTypeIds from the same JSON blob
    const parsedCalc = (() => {
      if (!v.calcSettingsJson) return { settings: DEFAULT_CALC, excluded: [] as string[] };
      try {
        const obj = JSON.parse(v.calcSettingsJson) as Partial<CalcSettings> & { excludedAllowanceTypeIds?: string[] };
        const { excludedAllowanceTypeIds = [], ...rest } = obj;
        return { settings: { ...DEFAULT_CALC, ...rest } as CalcSettings, excluded: excludedAllowanceTypeIds };
      } catch { return { settings: DEFAULT_CALC, excluded: [] as string[] }; }
    })();
    setVCalcSettings(parsedCalc.settings);
    setVExcludedAllowances(parsedCalc.excluded);
    setVCutoffDay(v.cutoffDay ?? 25);
    setVClosingDate(v.closingDate?.slice(0, 10) ?? "");
    setVPaymentDate(v.paymentDate?.slice(0, 10) ?? "");
    setVCarry(v.carryToNextPeriod ?? false);
    setActiveTab("general");
  }

  useEffect(() => { load(); }, [load]);

  // ── Header save ────────────────────────────────────────────────────────
  async function saveHeader() {
    if (!headerName.trim()) { toast.error("الاسم الإنجليزي مطلوب"); return; }
    setSavingHeader(true);
    try {
      await payrollTypesApi.updateHeader(id, {
        name: headerName.trim(),
        nameAr: headerNameAr.trim() || undefined,
        categoryId: headerCategory || undefined,
        status: headerStatus,
      });
      toast.success("تم تحديث البيانات الأساسية");
      await load();
    } catch (err) {
      notifyError(err, "تعذر حفظ البيانات الأساسية");
    } finally {
      setSavingHeader(false);
    }
  }

  // ── Version save ───────────────────────────────────────────────────────
  async function saveVersion() {
    if (!selectedVid) return;
    setSavingVersion(true);
    try {
      const body: Partial<PayrollVersion> = {
        frequency: vFrequency,
        currency: vCurrency,
        paymentMethodId: vPaymentMethodId || null,
        defaultExportFormatId: vExportFormatId || null,
        approvalWorkflowId: vApprovalWorkflowId || null,
        ruleSetVersionId: vRuleSetVersionId || null,
        selectionScopeJson: vScopeJson,
        dayBasis: vDayBasis,
        calcSettingsJson: JSON.stringify({ ...vCalcSettings, excludedAllowanceTypeIds: vExcludedAllowances }),
        cutoffDay: vCutoffDay,
        closingDate: vClosingDate || null,
        paymentDate: vPaymentDate || null,
        carryToNextPeriod: vCarry,
      };
      await payrollTypesApi.updateVersion(id, selectedVid, body);
      toast.success("تم حفظ الإصدار");
      await load();
    } catch (err) {
      notifyError(err, "تعذر حفظ الإصدار");
    } finally {
      setSavingVersion(false);
    }
  }

  // ── Clone ──────────────────────────────────────────────────────────────
  async function cloneVersion(vid: string) {
    setActingVid(vid);
    try {
      const newVid = await payrollTypesApi.cloneVersion(id, vid);
      toast.success("تم نسخ الإصدار");
      // Single fetch: load detail, then select the new draft version
      const d = await payrollTypesApi.get(id);
      setDetail(d);
      if (newVid) {
        selectVersion(newVid, d.versions);
      }
    } catch (err) {
      notifyError(err, "تعذر نسخ الإصدار");
    } finally {
      setActingVid(null);
    }
  }

  // ── Publish ────────────────────────────────────────────────────────────
  async function publishVersion(vid: string) {
    setActingVid(vid);
    try {
      await payrollTypesApi.publishVersion(id, vid);
      toast.success("تم نشر الإصدار");
      await load();
    } catch (err) {
      notifyError(err, "تعذر نشر الإصدار");
    } finally {
      setActingVid(null);
    }
  }

  // ── Simulate ───────────────────────────────────────────────────────────
  async function runSimulate() {
    if (!simDialogVid) return;
    setSimRunning(true);
    setSimResult(null);
    try {
      const r = await payrollTypesApi.simulate(id, simDialogVid, simYear, simMonth);
      setSimResult(r);
    } catch (err) {
      notifyError(err, "تعذر تشغيل المحاكاة");
    } finally {
      setSimRunning(false);
    }
  }

  // ── Computed ───────────────────────────────────────────────────────────
  const selectedVersion = detail?.versions.find((v) => v.id === selectedVid) ?? null;
  const isDraft = selectedVersion?.status === "Draft";

  const selectClass =
    "w-full h-9 bg-secondary border border-border px-3 text-sm text-foreground rounded-none";

  if (loading) {
    return (
      <div className="flex items-center justify-center py-24 text-muted-foreground">
        <Loader2 className="h-5 w-5 animate-spin" />
      </div>
    );
  }

  if (!detail) return null;

  const cat = categories.find((c) => c.id === detail.categoryId);

  return (
    <div className="space-y-6">
      {/* Breadcrumb */}
      <div className="flex items-center gap-2 text-sm">
        <Link
          href="/settings/payroll"
          className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1"
        >
          <ArrowRight className="h-4 w-4" /> إعدادات الرواتب
        </Link>
        <span className="text-muted-foreground">/</span>
        <Link href="/settings/payroll/types" className="text-muted-foreground hover:text-foreground transition-colors">
          أنواع المسير
        </Link>
        <span className="text-muted-foreground">/</span>
        <span className="font-mono text-xs">{detail.code}</span>
      </div>

      {/* Header Section */}
      <div className="border border-border p-6 space-y-4">
        <div className="flex items-start justify-between gap-4">
          <div>
            <div className="flex items-center gap-3">
              <span className="font-mono text-lg font-bold text-primary">{detail.code}</span>
              {statusBadge(detail.status)}
            </div>
            {cat && <p className="text-xs text-muted-foreground mt-1">{cat.nameAr}</p>}
          </div>
          {canConfigure && (
            <Button onClick={saveHeader} disabled={savingHeader} className="shrink-0 font-bold">
              {savingHeader ? <><Loader2 className="h-4 w-4 animate-spin ml-1" /> جاري الحفظ…</> : <><Save className="h-4 w-4 ml-1" /> حفظ</>}
            </Button>
          )}
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">الاسم (إنجليزي)</Label>
            <Input
              value={headerName}
              onChange={(e) => setHeaderName(e.target.value)}
              disabled={!canConfigure}
              className="bg-secondary border-border disabled:opacity-60"
            />
          </div>
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">الاسم (عربي)</Label>
            <Input
              value={headerNameAr}
              onChange={(e) => setHeaderNameAr(e.target.value)}
              disabled={!canConfigure}
              className="bg-secondary border-border disabled:opacity-60"
            />
          </div>
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">الفئة</Label>
            <select
              value={headerCategory}
              onChange={(e) => setHeaderCategory(e.target.value)}
              disabled={!canConfigure}
              className={`${selectClass} disabled:opacity-60`}
            >
              <option value="">— بدون فئة —</option>
              {categories.map((c) => (
                <option key={c.id} value={c.id}>{c.nameAr}</option>
              ))}
            </select>
          </div>
          <div className="space-y-2">
            <Label className="text-xs font-bold uppercase tracking-wider">الحالة</Label>
            <select
              value={headerStatus}
              onChange={(e) => setHeaderStatus(e.target.value)}
              disabled={!canConfigure}
              className={`${selectClass} disabled:opacity-60`}
            >
              {["Active", "Inactive", "Draft", "Archived"].map((s) => (
                <option key={s} value={s}>{STATUS_AR[s] ?? s}</option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Version timeline + editor */}
      <div className="grid grid-cols-1 lg:grid-cols-[280px_1fr] gap-4">
        {/* Timeline */}
        <div className="border border-border">
          <div className="px-4 py-3 border-b border-border flex items-center justify-between">
            <span className="text-xs font-bold uppercase tracking-wider text-muted-foreground">
              الإصدارات ({detail.versions.length})
            </span>
            {canConfigure && (
              <Button
                size="sm"
                variant="outline"
                className="h-7 text-xs"
                onClick={async () => {
                  try {
                    const vid = await payrollTypesApi.createVersion(id);
                    toast.success("تم إنشاء إصدار جديد");
                    const d = await payrollTypesApi.get(id);
                    setDetail(d);
                    selectVersion(vid, d.versions);
                  } catch (err) {
                    notifyError(err, "تعذر إنشاء إصدار");
                  }
                }}
              >
                + إصدار
              </Button>
            )}
          </div>
          <div className="divide-y divide-border">
            {detail.versions.length === 0 && (
              <p className="p-4 text-xs text-muted-foreground text-center">لا توجد إصدارات</p>
            )}
            {detail.versions.map((v) => (
              <div
                key={v.id}
                onClick={() => selectVersion(v.id, detail.versions)}
                className={`p-3 cursor-pointer hover:bg-card/50 transition-colors ${selectedVid === v.id ? "bg-card border-r-2 border-r-primary" : ""}`}
              >
                <div className="flex items-center justify-between gap-2">
                  <span className="text-sm font-bold">الإصدار {v.versionNumber}</span>
                  {versionBadge(v.status)}
                </div>
                <div className="text-[10px] text-muted-foreground mt-1 space-y-0.5">
                  {v.effectiveFrom && <div>من: {v.effectiveFrom.slice(0, 10)}</div>}
                  {v.effectiveTo && <div>إلى: {v.effectiveTo.slice(0, 10)}</div>}
                </div>
                {canConfigure && (
                  <div className="flex gap-1 mt-2" onClick={(e) => e.stopPropagation()}>
                    <button
                      title="نسخ"
                      disabled={actingVid === v.id}
                      onClick={() => cloneVersion(v.id)}
                      className="h-6 w-6 inline-flex items-center justify-center text-muted-foreground hover:text-foreground disabled:opacity-40"
                    >
                      <Copy className="h-3.5 w-3.5" />
                    </button>
                    {v.status === "Draft" && (
                      <button
                        title="نشر"
                        disabled={actingVid === v.id}
                        onClick={() => publishVersion(v.id)}
                        className="h-6 w-6 inline-flex items-center justify-center text-muted-foreground hover:text-green-500 disabled:opacity-40"
                      >
                        <Send className="h-3.5 w-3.5" />
                      </button>
                    )}
                    <button
                      title="محاكاة"
                      onClick={() => { setSimDialogVid(v.id); setSimResult(null); }}
                      className="h-6 w-6 inline-flex items-center justify-center text-muted-foreground hover:text-primary"
                    >
                      <Play className="h-3.5 w-3.5" />
                    </button>
                  </div>
                )}
              </div>
            ))}
          </div>
        </div>

        {/* Version editor */}
        {selectedVersion ? (
          <div className="border border-border">
            {/* Editor header */}
            <div className="px-6 py-4 border-b border-border flex items-center justify-between">
              <div className="flex items-center gap-2">
                <span className="font-bold">الإصدار {selectedVersion.versionNumber}</span>
                {versionBadge(selectedVersion.status)}
              </div>
              {isDraft && canConfigure && (
                <Button onClick={saveVersion} disabled={savingVersion} className="font-bold">
                  {savingVersion ? <><Loader2 className="h-4 w-4 animate-spin ml-1" /> جاري الحفظ…</> : <><Save className="h-4 w-4 ml-1" /> حفظ الإصدار</>}
                </Button>
              )}
              {!isDraft && canConfigure && (
                <Button
                  variant="outline"
                  onClick={() => cloneVersion(selectedVersion.id)}
                  disabled={actingVid === selectedVersion.id}
                >
                  <Copy className="h-4 w-4 ml-1" /> نسخ للتعديل
                </Button>
              )}
            </div>

            {/* Read-only notice for non-draft */}
            {!isDraft && (
              <div className="px-6 py-3 bg-zinc-500/5 border-b border-border flex items-center gap-2 text-xs text-muted-foreground">
                <Eye className="h-3.5 w-3.5" />
                هذا الإصدار ({VERSION_STATUS_AR[selectedVersion.status] ?? selectedVersion.status}) للعرض فقط. انسخه لإجراء تعديلات.
              </div>
            )}

            {/* Tabs */}
            <div className="flex border-b border-border">
              {([
                { key: "general", label: "عام" },
                { key: "scope",   label: "النطاق" },
                { key: "calc",    label: "الاحتساب" },
                { key: "cutoff",  label: "الإغلاق" },
              ] as { key: Tab; label: string }[]).map((t) => (
                <button
                  key={t.key}
                  onClick={() => setActiveTab(t.key)}
                  className={`px-5 py-3 text-sm border-b-2 transition-colors ${activeTab === t.key ? "border-primary text-foreground font-medium" : "border-transparent text-muted-foreground hover:text-foreground"}`}
                >
                  {t.label}
                </button>
              ))}
            </div>

            {/* Tab content */}
            <div className="p-6 space-y-4">
              {/* ── General ── */}
              {activeTab === "general" && (
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label className="text-xs font-bold uppercase tracking-wider">الدورية</Label>
                    <select
                      value={vFrequency}
                      onChange={(e) => setVFrequency(e.target.value)}
                      disabled={!isDraft || !canConfigure}
                      className={`${selectClass} disabled:opacity-60`}
                    >
                      {FREQUENCY_OPTIONS.map((f) => (
                        <option key={f} value={f}>{FREQUENCY_AR[f] ?? f}</option>
                      ))}
                    </select>
                  </div>
                  <div className="space-y-2">
                    <Label className="text-xs font-bold uppercase tracking-wider">العملة</Label>
                    <Input
                      value={vCurrency}
                      onChange={(e) => setVCurrency(e.target.value)}
                      disabled={!isDraft || !canConfigure}
                      className="bg-secondary border-border font-mono disabled:opacity-60"
                      placeholder="SAR"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label className="text-xs font-bold uppercase tracking-wider">طريقة الدفع</Label>
                    <select
                      value={vPaymentMethodId}
                      onChange={(e) => setVPaymentMethodId(e.target.value)}
                      disabled={!isDraft || !canConfigure}
                      className={`${selectClass} disabled:opacity-60`}
                    >
                      <option value="">— اختياري —</option>
                      {paymentMethods.map((p) => (
                        <option key={p.id} value={p.id}>{p.nameAr}</option>
                      ))}
                    </select>
                  </div>
                  <div className="space-y-2">
                    <Label className="text-xs font-bold uppercase tracking-wider">صيغة التصدير</Label>
                    <select
                      value={vExportFormatId}
                      onChange={(e) => setVExportFormatId(e.target.value)}
                      disabled={!isDraft || !canConfigure}
                      className={`${selectClass} disabled:opacity-60`}
                    >
                      <option value="">— اختياري —</option>
                      {exportFormats.map((e) => (
                        <option key={e.id} value={e.id}>{e.nameAr}</option>
                      ))}
                    </select>
                  </div>
                  <div className="space-y-2">
                    <Label className="text-xs font-bold uppercase tracking-wider">معرف سير الاعتماد</Label>
                    <Input
                      value={vApprovalWorkflowId}
                      onChange={(e) => setVApprovalWorkflowId(e.target.value)}
                      disabled={!isDraft || !canConfigure}
                      className="bg-secondary border-border font-mono text-xs disabled:opacity-60"
                      placeholder="UUID (اختياري)"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label className="text-xs font-bold uppercase tracking-wider">معرف إصدار مجموعة القواعد</Label>
                    <Input
                      value={vRuleSetVersionId}
                      onChange={(e) => setVRuleSetVersionId(e.target.value)}
                      disabled={!isDraft || !canConfigure}
                      className="bg-secondary border-border font-mono text-xs disabled:opacity-60"
                      placeholder="UUID (اختياري)"
                    />
                  </div>
                </div>
              )}

              {/* ── Scope ── */}
              {activeTab === "scope" && (
                <ScopeBuilder
                  value={vScopeJson}
                  disabled={!isDraft || !canConfigure}
                  onChange={(json) => {
                    if (isDraft && canConfigure) setVScopeJson(json);
                  }}
                />
              )}

              {/* ── Calculation ── */}
              {activeTab === "calc" && (
                <div className="space-y-4">
                  <div className="space-y-2">
                    <Label className="text-xs font-bold uppercase tracking-wider">أساس الأيام</Label>
                    <select
                      value={vDayBasis}
                      onChange={(e) => setVDayBasis(e.target.value)}
                      disabled={!isDraft || !canConfigure}
                      className={`${selectClass} max-w-sm disabled:opacity-60`}
                    >
                      {DAY_BASIS_OPTIONS.map((d) => (
                        <option key={d} value={d}>{DAY_BASIS_AR[d] ?? d}</option>
                      ))}
                    </select>
                  </div>

                  <div className="border-t border-border pt-4">
                    <p className="text-xs font-bold uppercase tracking-wider text-muted-foreground mb-3">
                      خيارات الاحتساب
                    </p>
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                      {CALC_TOGGLES.map(({ key, label }) => (
                        <label
                          key={key}
                          className={`flex items-center gap-2 text-sm border border-border px-3 py-2 ${isDraft && canConfigure ? "cursor-pointer hover:bg-card/50" : "opacity-60 cursor-not-allowed"}`}
                        >
                          <input
                            type="checkbox"
                            checked={vCalcSettings[key]}
                            disabled={!isDraft || !canConfigure}
                            onChange={(e) =>
                              setVCalcSettings((prev) => ({ ...prev, [key]: e.target.checked }))
                            }
                          />
                          {label}
                        </label>
                      ))}
                    </div>
                  </div>

                  <div className="border-t border-border pt-4">
                    <p className="text-xs font-bold uppercase tracking-wider text-muted-foreground mb-3">
                      البدلات المستثناة من الاحتساب
                    </p>
                    <div className="flex flex-wrap gap-2">
                      {allowanceTypes.map((a) => {
                        const sel = vExcludedAllowances.includes(a.id);
                        return (
                          <button
                            key={a.id}
                            type="button"
                            disabled={!isDraft || !canConfigure}
                            onClick={() => {
                              if (!isDraft || !canConfigure) return;
                              setVExcludedAllowances((prev) =>
                                sel ? prev.filter((x) => x !== a.id) : [...prev, a.id]
                              );
                            }}
                            className={`text-xs px-3 py-1.5 border transition-colors ${sel ? "bg-primary text-primary-foreground border-primary" : "border-border text-muted-foreground hover:border-primary/50"} disabled:opacity-60 disabled:cursor-not-allowed`}
                          >
                            {a.nameAr}
                          </button>
                        );
                      })}
                      {allowanceTypes.length === 0 && (
                        <p className="text-xs text-muted-foreground">لا توجد بدلات مضافة</p>
                      )}
                    </div>
                  </div>
                </div>
              )}

              {/* ── Cutoff ── */}
              {activeTab === "cutoff" && (
                <div className="space-y-4">
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label className="text-xs font-bold uppercase tracking-wider">يوم الإغلاق (1-31)</Label>
                      <Input
                        type="number"
                        min={1}
                        max={31}
                        value={vCutoffDay}
                        onChange={(e) => setVCutoffDay(Number(e.target.value))}
                        disabled={!isDraft || !canConfigure}
                        className="bg-secondary border-border disabled:opacity-60"
                      />
                    </div>
                    <div className="space-y-2">
                      <Label className="text-xs font-bold uppercase tracking-wider">تاريخ الإغلاق</Label>
                      <Input
                        type="date"
                        value={vClosingDate}
                        onChange={(e) => setVClosingDate(e.target.value)}
                        disabled={!isDraft || !canConfigure}
                        className="bg-secondary border-border disabled:opacity-60"
                      />
                    </div>
                    <div className="space-y-2">
                      <Label className="text-xs font-bold uppercase tracking-wider">تاريخ الدفع</Label>
                      <Input
                        type="date"
                        value={vPaymentDate}
                        onChange={(e) => setVPaymentDate(e.target.value)}
                        disabled={!isDraft || !canConfigure}
                        className="bg-secondary border-border disabled:opacity-60"
                      />
                    </div>
                    <div className="space-y-2">
                      <Label className="text-xs font-bold uppercase tracking-wider">الترحيل للفترة التالية</Label>
                      <label className={`flex items-center gap-2 text-sm border border-border px-3 py-2 h-9 ${isDraft && canConfigure ? "cursor-pointer" : "opacity-60 cursor-not-allowed"}`}>
                        <input
                          type="checkbox"
                          checked={vCarry}
                          disabled={!isDraft || !canConfigure}
                          onChange={(e) => setVCarry(e.target.checked)}
                        />
                        ترحيل الحركات المتأخرة
                      </label>
                    </div>
                  </div>

                  {/* Derived bilingual message */}
                  <div className="border border-border bg-secondary/30 p-4 space-y-1">
                    <p className="text-sm text-muted-foreground">
                      تُرحّل الحركات بعد يوم <span className="font-bold text-foreground">{vCutoffDay}</span> إلى الفترة التالية
                    </p>
                    <p className="text-xs text-muted-foreground font-mono">
                      Transactions after day <span className="font-bold text-foreground">{vCutoffDay}</span> carry to the next period
                    </p>
                  </div>
                </div>
              )}
            </div>
          </div>
        ) : (
          <div className="border border-border flex items-center justify-center text-muted-foreground text-sm py-16">
            اختر إصداراً من القائمة لعرض تفاصيله
          </div>
        )}
      </div>

      {/* Simulate Dialog */}
      <Dialog open={!!simDialogVid} onOpenChange={(o) => { if (!o && !simRunning) { setSimDialogVid(null); setSimResult(null); } }}>
        <DialogContent className="sm:max-w-lg">
          <DialogHeader>
            <DialogTitle>محاكاة الإصدار</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-2">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label className="text-xs font-bold uppercase tracking-wider">السنة</Label>
                <Input
                  type="number"
                  min={2020}
                  max={2099}
                  value={simYear}
                  onChange={(e) => setSimYear(Number(e.target.value))}
                  className="bg-secondary border-border"
                />
              </div>
              <div className="space-y-2">
                <Label className="text-xs font-bold uppercase tracking-wider">الشهر</Label>
                <Input
                  type="number"
                  min={1}
                  max={12}
                  value={simMonth}
                  onChange={(e) => setSimMonth(Number(e.target.value))}
                  className="bg-secondary border-border"
                />
              </div>
            </div>

            {simResult && (
              <div className="border border-border bg-secondary/30 p-4 space-y-2">
                <div className="grid grid-cols-2 gap-x-4 gap-y-1 text-sm">
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">الموظفون</span>
                    <span className="font-bold">{simResult.employeeCount}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">إجمالي الراتب</span>
                    <span className="font-bold text-green-500">
                      {simResult.grossTotal.toLocaleString("ar-SA", { minimumFractionDigits: 2 })} {simResult.currency}
                    </span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">الاستقطاعات</span>
                    <span className="font-bold text-red-500">
                      {simResult.deductionTotal.toLocaleString("ar-SA", { minimumFractionDigits: 2 })} {simResult.currency}
                    </span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">الصافي</span>
                    <span className="font-bold text-primary">
                      {simResult.netTotal.toLocaleString("ar-SA", { minimumFractionDigits: 2 })} {simResult.currency}
                    </span>
                  </div>
                </div>
                {simResult.findings.length > 0 && (
                  <div className="text-xs text-yellow-500 border-t border-border pt-2 space-y-0.5">
                    {simResult.findings.slice(0, 3).map((f, i) => (
                      <div key={i}>{f.message}</div>
                    ))}
                  </div>
                )}
              </div>
            )}
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => { setSimDialogVid(null); setSimResult(null); }} disabled={simRunning}>
              إغلاق
            </Button>
            <Button onClick={runSimulate} disabled={simRunning} className="font-bold">
              {simRunning ? <><Loader2 className="h-4 w-4 animate-spin ml-1" /> جاري المحاكاة…</> : <><Play className="h-4 w-4 ml-1" /> تشغيل</>}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
