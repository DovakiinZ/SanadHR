"use client";

/**
 * ScopeBuilder — renders the "Selection Scope" picker for a payroll type version.
 *
 * SelectionScope JSON shape:
 *   { mode: "All" | "Criteria", include: [{dimension, valueIds}], exclude: [{dimension, valueIds}], includeEmployeeIds: [], excludeEmployeeIds: [] }
 *
 * EmployeeStatus → deterministic GUID contract (StatusScopeProvider.StatusId):
 *   Format: 00000000-0000-0000-0000-0000000000NN  where NN = (int)EmployeeStatus zero-padded to 2 digits.
 *   Active     = 1  → 00000000-0000-0000-0000-000000000001
 *   OnLeave    = 2  → 00000000-0000-0000-0000-000000000002
 *   Suspended  = 3  → 00000000-0000-0000-0000-000000000003
 *   Terminated = 4  → 00000000-0000-0000-0000-000000000004
 *   Resigned   = 5  → 00000000-0000-0000-0000-000000000005
 */

import { useCallback, useEffect, useRef, useState } from "react";
import { Loader2, ChevronDown, X, AlertCircle, Users } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { payrollTypesApi, type ScopeDimension } from "@/lib/api/payroll-types";
import { getMasterDataItems, type MasterDataItem } from "@/lib/api/master-data";

// ── EmployeeStatus GUID mapping ─────────────────────────────────────────────
// These GUIDs MUST match the backend StatusScopeProvider.StatusId formula:
// Guid.Parse($"00000000-0000-0000-0000-{((int)status):D2}")  — (int) zero-padded to 2 digits
const EMPLOYEE_STATUS_OPTIONS: { id: string; label: string; labelAr: string }[] = [
  { id: "00000000-0000-0000-0000-000000000001", label: "Active",     labelAr: "نشط" },
  { id: "00000000-0000-0000-0000-000000000002", label: "On Leave",   labelAr: "في إجازة" },
  { id: "00000000-0000-0000-0000-000000000003", label: "Suspended",  labelAr: "موقوف" },
  { id: "00000000-0000-0000-0000-000000000004", label: "Terminated", labelAr: "منتهية خدمته" },
  { id: "00000000-0000-0000-0000-000000000005", label: "Resigned",   labelAr: "مستقيل" },
];

// ── Slug → ObjectType mapping for master-data lookups ──────────────────────
// The dimension.valueSourceRef is the kebab slug (e.g. "departments").
// getMasterDataItems expects the ObjectType (PascalCase).
const SLUG_TO_OBJECT_TYPE: Record<string, string> = {
  departments: "Department",
  branches: "Branch",
  "job-titles": "JobTitle",
  "employment-types": "EmploymentType",
  "contract-types": "ContractType",
  grades: "Grade",
  nationalities: "Nationality",
  "cost-centers": "CostCenter",
  "payroll-groups": "PayrollGroup",
  "work-locations": "WorkLocation",
  "leave-types": "LeaveType",
  "shift-types": "ShiftType",
};

function slugToObjectType(slug: string): string {
  return SLUG_TO_OBJECT_TYPE[slug] ?? slug.replace(/-([a-z])/g, (_, c: string) => c.toUpperCase()).replace(/^./, (c: string) => c.toUpperCase());
}

// ── Types ─────────────────────────────────────────────────────────────────
interface ScopeEntry { dimension: string; valueIds: string[] }
interface SelectionScope {
  mode: "All" | "Criteria";
  include: ScopeEntry[];
  exclude: ScopeEntry[];
  includeEmployeeIds: string[];
  excludeEmployeeIds: string[];
}

const EMPTY_SCOPE: SelectionScope = {
  mode: "All",
  include: [],
  exclude: [],
  includeEmployeeIds: [],
  excludeEmployeeIds: [],
};

function parseScope(json: string | null): SelectionScope {
  if (!json) return EMPTY_SCOPE;
  try { return { ...EMPTY_SCOPE, ...(JSON.parse(json) as Partial<SelectionScope>) }; }
  catch { return EMPTY_SCOPE; }
}

// ── Option type used in selects ───────────────────────────────────────────
interface Opt { id: string; label: string }

function dimensionOptions(dim: ScopeDimension): Opt[] | null {
  if (dim.valueSourceKind === "StaticEnum" && dim.valueSourceRef === "EmployeeStatus") {
    return EMPLOYEE_STATUS_OPTIONS.map((s) => ({ id: s.id, label: s.labelAr }));
  }
  return null; // fetched async for MasterData
}

// ── Sub-component: one dimension row ─────────────────────────────────────
function DimensionRow({
  dimensions,
  entry,
  onChange,
  onRemove,
  fetchedOptions,
  fetchOptions,
  disabled,
}: {
  dimensions: ScopeDimension[];
  entry: ScopeEntry;
  onChange: (e: ScopeEntry) => void;
  onRemove: () => void;
  fetchedOptions: Record<string, Opt[]>;
  fetchOptions: (dim: ScopeDimension) => void;
  disabled?: boolean;
}) {
  const dim = dimensions.find((d) => d.key === entry.dimension);
  const staticOpts = dim ? dimensionOptions(dim) : null;
  const opts: Opt[] = staticOpts ?? fetchedOptions[entry.dimension] ?? [];

  function handleDimChange(key: string) {
    if (disabled) return;
    const newDim = dimensions.find((d) => d.key === key);
    onChange({ dimension: key, valueIds: [] });
    if (newDim && !staticOpts && !fetchedOptions[key]) fetchOptions(newDim);
  }

  useEffect(() => {
    if (dim && dim.valueSourceKind === "MasterData" && !fetchedOptions[dim.key]) {
      fetchOptions(dim);
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [dim?.key]);

  const selectClass = "h-8 bg-secondary border border-border px-2 text-sm text-foreground rounded-none";

  return (
    <div className={`flex items-start gap-2 p-2 bg-secondary/40 border border-border ${disabled ? "opacity-60" : ""}`}>
      {/* Dimension picker */}
      <select
        value={entry.dimension}
        onChange={(e) => handleDimChange(e.target.value)}
        disabled={disabled}
        className={`${selectClass} min-w-[150px] disabled:cursor-not-allowed`}
      >
        <option value="">— اختر بُعداً —</option>
        {dimensions.map((d) => (
          <option key={d.key} value={d.key} disabled={!d.isAvailable}>
            {d.nameAr}{!d.isAvailable ? ` (${d.unavailableNote ?? "غير متاح"})` : ""}
          </option>
        ))}
      </select>

      {/* Value multiselect */}
      {entry.dimension && (
        <div className="flex flex-wrap gap-1 flex-1 min-w-0">
          {opts.length === 0 && (
            <span className="text-xs text-muted-foreground self-center">
              <Loader2 className="h-3 w-3 animate-spin inline ml-1" />جاري تحميل القيم…
            </span>
          )}
          {opts.map((o) => {
            const sel = entry.valueIds.includes(o.id);
            return (
              <button
                key={o.id}
                type="button"
                disabled={disabled}
                onClick={() => {
                  if (disabled) return;
                  const ids = sel ? entry.valueIds.filter((x) => x !== o.id) : [...entry.valueIds, o.id];
                  onChange({ ...entry, valueIds: ids });
                }}
                className={`text-xs px-2 py-0.5 border transition-colors ${
                  sel ? "bg-primary text-primary-foreground border-primary" : "border-border text-muted-foreground hover:border-primary/50"
                } disabled:cursor-not-allowed`}
              >
                {o.label}
              </button>
            );
          })}
        </div>
      )}

      <button
        type="button"
        disabled={disabled}
        onClick={disabled ? undefined : onRemove}
        className="h-8 w-8 shrink-0 inline-flex items-center justify-center text-muted-foreground hover:text-destructive disabled:cursor-not-allowed disabled:opacity-50"
      >
        <X className="h-4 w-4" />
      </button>
    </div>
  );
}

// ── Section (Include / Exclude) ───────────────────────────────────────────
function ScopeSection({
  label,
  entries,
  dimensions,
  onChange,
  fetchedOptions,
  fetchOptions,
  disabled,
}: {
  label: string;
  entries: ScopeEntry[];
  dimensions: ScopeDimension[];
  onChange: (entries: ScopeEntry[]) => void;
  fetchedOptions: Record<string, Opt[]>;
  fetchOptions: (dim: ScopeDimension) => void;
  disabled?: boolean;
}) {
  function addRow() {
    if (disabled) return;
    onChange([...entries, { dimension: "", valueIds: [] }]);
  }
  function updateRow(i: number, e: ScopeEntry) {
    const next = [...entries];
    next[i] = e;
    onChange(next);
  }
  function removeRow(i: number) {
    onChange(entries.filter((_, idx) => idx !== i));
  }

  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between">
        <span className="text-xs font-bold uppercase tracking-wider text-muted-foreground">{label}</span>
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={addRow}
          disabled={disabled}
          className="h-7 text-xs disabled:opacity-60 disabled:cursor-not-allowed"
        >
          + إضافة شرط
        </Button>
      </div>
      {entries.length === 0 && (
        <p className="text-xs text-muted-foreground py-2">لا توجد شروط — يُطبَّق على الجميع</p>
      )}
      <div className="space-y-1">
        {entries.map((entry, i) => (
          <DimensionRow
            key={i}
            dimensions={dimensions}
            entry={entry}
            onChange={(e) => updateRow(i, e)}
            onRemove={() => removeRow(i)}
            fetchedOptions={fetchedOptions}
            fetchOptions={fetchOptions}
            disabled={disabled}
          />
        ))}
      </div>
    </div>
  );
}

// ── Main ScopeBuilder ────────────────────────────────────────────────────
export function ScopeBuilder({
  value,
  onChange,
  disabled = false,
}: {
  value: string | null;
  onChange: (json: string) => void;
  disabled?: boolean;
}) {
  const scope = parseScope(value);
  const [dimensions, setDimensions] = useState<ScopeDimension[]>([]);
  const [loadingDims, setLoadingDims] = useState(true);
  const [fetchedOptions, setFetchedOptions] = useState<Record<string, Opt[]>>({});
  const [liveCount, setLiveCount] = useState<{ includedCount: number; excludedCount: number; warnings: string[] } | null>(null);
  const [countLoading, setCountLoading] = useState(false);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    payrollTypesApi.scopeDimensions()
      .then((dims) => setDimensions(dims ?? []))
      .catch(() => {/* silently ignore */})
      .finally(() => setLoadingDims(false));
  }, []);

  function fetchOptions(dim: ScopeDimension) {
    if (dim.valueSourceKind === "StaticEnum") return; // static, no fetch needed
    if (dim.valueSourceKind === "MasterData" && dim.valueSourceRef) {
      const objectType = slugToObjectType(dim.valueSourceRef);
      getMasterDataItems(objectType, { includeInactive: false })
        .then((items: MasterDataItem[]) => {
          setFetchedOptions((prev) => ({
            ...prev,
            [dim.key]: items.map((it) => ({ id: it.id, label: it.nameAr || it.nameEn })),
          }));
        })
        .catch(() => {/* silently ignore */});
    }
  }

  const emitChange = useCallback((next: SelectionScope) => {
    const json = JSON.stringify(next);
    onChange(json);
    // debounce resolveScope
    if (debounceRef.current) clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(async () => {
      setCountLoading(true);
      try {
        const result = await payrollTypesApi.resolveScope(json);
        setLiveCount(result);
      } catch {
        setLiveCount(null);
      } finally {
        setCountLoading(false);
      }
    }, 600);
  }, [onChange]);

  function setMode(mode: "All" | "Criteria") {
    emitChange({ ...scope, mode });
  }

  function setInclude(entries: ScopeEntry[]) {
    emitChange({ ...scope, include: entries });
  }

  function setExclude(entries: ScopeEntry[]) {
    emitChange({ ...scope, exclude: entries });
  }

  if (loadingDims) {
    return (
      <div className="flex items-center gap-2 text-sm text-muted-foreground py-4">
        <Loader2 className="h-4 w-4 animate-spin" /> جاري تحميل الأبعاد…
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {/* Mode toggle */}
      <div className="flex items-center gap-2">
        <span className="text-xs font-bold uppercase tracking-wider text-muted-foreground">نطاق التطبيق</span>
        <div className={`flex border border-border ${disabled ? "opacity-60" : ""}`}>
          <button
            type="button"
            disabled={disabled}
            onClick={() => { if (!disabled) setMode("All"); }}
            className={`px-4 py-1.5 text-sm transition-colors disabled:cursor-not-allowed ${scope.mode === "All" ? "bg-primary text-primary-foreground" : "text-muted-foreground hover:text-foreground"}`}
          >
            جميع الموظفين
          </button>
          <button
            type="button"
            disabled={disabled}
            onClick={() => { if (!disabled) setMode("Criteria"); }}
            className={`px-4 py-1.5 text-sm border-r border-border transition-colors disabled:cursor-not-allowed ${scope.mode === "Criteria" ? "bg-primary text-primary-foreground" : "text-muted-foreground hover:text-foreground"}`}
          >
            معايير محددة
          </button>
        </div>
      </div>

      {/* Criteria sections */}
      {scope.mode === "Criteria" && (
        <div className="space-y-4 border border-border p-4">
          <ScopeSection
            label="تضمين"
            entries={scope.include}
            dimensions={dimensions}
            onChange={setInclude}
            fetchedOptions={fetchedOptions}
            fetchOptions={fetchOptions}
            disabled={disabled}
          />
          <div className="border-t border-border" />
          <ScopeSection
            label="استثناء"
            entries={scope.exclude}
            dimensions={dimensions}
            onChange={setExclude}
            fetchedOptions={fetchedOptions}
            fetchOptions={fetchOptions}
            disabled={disabled}
          />
        </div>
      )}

      {/* Live count */}
      <div className="flex items-center gap-2 text-xs text-muted-foreground">
        <Users className="h-3.5 w-3.5" />
        {countLoading ? (
          <><Loader2 className="h-3 w-3 animate-spin" /> جاري الحساب…</>
        ) : liveCount ? (
          <>
            <span className="text-green-500 font-medium">{liveCount.includedCount} مشمول</span>
            <span>·</span>
            <span className="text-red-500 font-medium">{liveCount.excludedCount} مستثنى</span>
            {liveCount.warnings.length > 0 && (
              <span className="flex items-center gap-1 text-yellow-500">
                <AlertCircle className="h-3 w-3" />
                {liveCount.warnings[0]}
              </span>
            )}
          </>
        ) : (
          <span>احفظ لحساب عدد الموظفين المشمولين</span>
        )}
      </div>
    </div>
  );
}
