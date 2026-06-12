"use client";

import { useEffect, useMemo, useState } from "react";
import { Check, ChevronLeft, ChevronRight, Loader2, Search, Sparkles, Wand2 } from "lucide-react";
import { toast } from "sonner";
import { aiSuggestWidget, getCatalog } from "@/lib/api/dashboards";
import { AggregationName, CatalogField, CatalogObject, WidgetQuerySpec } from "@/types/dashboard";
import { WidgetCard } from "./widget-card";

interface WidgetBuilderProps {
  onSave: (args: { spec: WidgetQuerySpec; visualization: string; titleAr: string; titleEn: string }) => Promise<void> | void;
  onCancel: () => void;
  saving?: boolean;
}

const AGGREGATIONS: { value: AggregationName; label: string; needsField: boolean; measureOnly: boolean }[] = [
  { value: "Count", label: "العدد", needsField: false, measureOnly: false },
  { value: "Sum", label: "المجموع", needsField: true, measureOnly: true },
  { value: "Average", label: "المتوسط", needsField: true, measureOnly: true },
  { value: "Min", label: "الأدنى", needsField: true, measureOnly: true },
  { value: "Max", label: "الأعلى", needsField: true, measureOnly: true },
  { value: "DistinctCount", label: "عدد القيم المميزة", needsField: true, measureOnly: false },
  { value: "Percentage", label: "نسبة مئوية", needsField: false, measureOnly: false },
];

const VIS_LABELS: Record<string, string> = {
  KpiCard: "بطاقة مؤشر", Gauge: "مقياس", BarChart: "أعمدة", HorizontalBar: "أعمدة أفقية",
  PieChart: "دائري", DonutChart: "حلقي", LineChart: "خطي", TrendChart: "مساحي",
  ProgressWidget: "أشرطة تقدم", Leaderboard: "لوحة صدارة", Table: "جدول",
};

const STEPS = ["الكائن", "الخاصية", "طريقة الحساب", "العرض", "الحفظ"];

export function WidgetBuilder({ onSave, onCancel, saving }: WidgetBuilderProps) {
  const [catalog, setCatalog] = useState<CatalogObject[]>([]);
  const [loadingCatalog, setLoadingCatalog] = useState(true);
  const [step, setStep] = useState(0);
  const [search, setSearch] = useState("");

  const [objectCode, setObjectCode] = useState("");
  const [groupBy, setGroupBy] = useState("");           // dimension / property
  const [granularity, setGranularity] = useState("month");
  const [aggregation, setAggregation] = useState<AggregationName>("Count");
  const [aggField, setAggField] = useState("");
  const [visualization, setVisualization] = useState("KpiCard");
  const [titleAr, setTitleAr] = useState("");
  const [requiredPermission, setRequiredPermission] = useState("");
  const [aiPrompt, setAiPrompt] = useState("");
  const [aiLoading, setAiLoading] = useState(false);

  useEffect(() => {
    getCatalog().then(setCatalog).catch(() => setCatalog([])).finally(() => setLoadingCatalog(false));
  }, []);

  const object = useMemo(() => catalog.find((o) => o.code === objectCode) ?? null, [catalog, objectCode]);
  const groupField = useMemo<CatalogField | null>(
    () => object?.fields.find((f) => f.code === groupBy) ?? null, [object, groupBy]);

  const measureFields = useMemo(() => object?.fields.filter((f) => f.isMeasure) ?? [], [object]);
  const groupableFields = useMemo(() => object?.fields.filter((f) => f.isGroupable) ?? [], [object]);

  // Compatible visualizations given the current shape.
  const visOptions = useMemo(() => {
    if (!groupBy) return aggregation === "Percentage" ? ["Gauge", "KpiCard"] : ["KpiCard", "Gauge"];
    if (groupField?.isDate) return ["LineChart", "TrendChart", "BarChart", "Table"];
    return ["BarChart", "HorizontalBar", "PieChart", "DonutChart", "ProgressWidget", "Leaderboard", "Table", "LineChart"];
  }, [groupBy, groupField, aggregation]);

  // Keep visualization valid when the shape changes.
  useEffect(() => {
    if (!visOptions.includes(visualization)) setVisualization(visOptions[0]);
  }, [visOptions, visualization]);

  // Percentage is a scalar concept → clear group-by.
  useEffect(() => {
    if (aggregation === "Percentage" && groupBy) setGroupBy("");
  }, [aggregation, groupBy]);

  const spec: WidgetQuerySpec = useMemo(() => ({
    objectCode,
    aggregation,
    aggregationField: AGGREGATIONS.find((a) => a.value === aggregation)?.needsField ? aggField || null : null,
    groupByField: groupBy || null,
    dateGranularity: groupField?.isDate ? granularity : null,
    visualization,
    limit: 12,
    requiredPermission: requiredPermission.trim() || null,
    filters: [],
  }), [objectCode, aggregation, aggField, groupBy, groupField, granularity, visualization, requiredPermission]);

  const runAi = async () => {
    if (!aiPrompt.trim()) return;
    setAiLoading(true);
    try {
      const s = await aiSuggestWidget(aiPrompt.trim());
      setObjectCode(s.spec.objectCode);
      setGroupBy(s.spec.groupByField ?? "");
      setAggregation(s.spec.aggregation);
      setAggField(s.spec.aggregationField ?? "");
      setGranularity(s.spec.dateGranularity ?? "month");
      setVisualization(s.visualization);
      setTitleAr(s.titleAr || aiPrompt.trim());
      setStep(4);
      toast.success("تم إنشاء العنصر — راجع واحفظ");
    } catch {
      toast.error("تعذر تفسير الوصف");
    } finally {
      setAiLoading(false);
    }
  };

  const aggDef = AGGREGATIONS.find((a) => a.value === aggregation)!;
  const canPreview = !!objectCode && (!aggDef.needsField || !!aggField);
  const canSave = canPreview && titleAr.trim().length > 0 && !!visualization;

  const filteredCatalog = useMemo(() => {
    const q = search.trim().toLowerCase();
    const list = q ? catalog.filter((o) => o.nameAr.includes(search) || o.nameEn.toLowerCase().includes(q) || o.code.toLowerCase().includes(q)) : catalog;
    const groups = new Map<string, CatalogObject[]>();
    for (const o of list) { (groups.get(o.module) ?? groups.set(o.module, []).get(o.module)!).push(o); }
    return [...groups.entries()];
  }, [catalog, search]);

  const next = () => setStep((s) => Math.min(STEPS.length - 1, s + 1));
  const back = () => setStep((s) => Math.max(0, s - 1));

  const stepValid = [
    !!objectCode,
    true, // property is optional
    !aggDef.needsField || !!aggField,
    !!visualization,
    titleAr.trim().length > 0,
  ];

  return (
    <div className="flex h-full max-h-[85vh] flex-col">
      {/* Stepper */}
      <div className="flex items-center gap-1 border-b border-border px-5 py-3">
        {STEPS.map((label, i) => (
          <button key={label} onClick={() => i <= step && setStep(i)} className="flex items-center gap-1">
            <span className={`flex h-6 w-6 items-center justify-center rounded-full text-xs font-bold ${
              i < step ? "bg-primary text-primary-foreground" : i === step ? "border border-primary text-primary" : "border border-border text-muted-foreground"}`}>
              {i < step ? <Check className="h-3 w-3" /> : i + 1}
            </span>
            <span className={`text-xs ${i === step ? "font-bold" : "text-muted-foreground"} ${i > step ? "" : "cursor-pointer"}`}>{label}</span>
            {i < STEPS.length - 1 && <ChevronLeft className="mx-1 h-3 w-3 text-muted-foreground" />}
          </button>
        ))}
      </div>

      <div className="grid min-h-0 flex-1 grid-cols-1 lg:grid-cols-[1fr_360px]">
        {/* Controls */}
        <div className="min-h-0 overflow-auto p-5">
          {loadingCatalog ? (
            <div className="flex h-40 items-center justify-center text-muted-foreground"><Loader2 className="h-5 w-5 animate-spin" /></div>
          ) : (
            <>
              {/* Step 1 — Object */}
              {step === 0 && (
                <div className="space-y-3">
                  {/* AI builder */}
                  <div className="border border-primary/40 bg-primary/5 p-3">
                    <p className="mb-2 flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-primary">
                      <Sparkles className="h-3.5 w-3.5" /> صف ما تريد
                    </p>
                    <div className="flex gap-2">
                      <input
                        value={aiPrompt}
                        onChange={(e) => setAiPrompt(e.target.value)}
                        onKeyDown={(e) => e.key === "Enter" && runAi()}
                        placeholder="مثال: عدد الموظفين حسب الإدارة"
                        className="h-9 flex-1 border border-border bg-secondary px-3 text-sm"
                      />
                      <button onClick={runAi} disabled={aiLoading || !aiPrompt.trim()}
                        className="inline-flex h-9 items-center gap-1.5 bg-primary px-3 text-sm font-bold text-primary-foreground hover:bg-primary/80 disabled:opacity-40">
                        {aiLoading ? <Loader2 className="h-4 w-4 animate-spin" /> : <Wand2 className="h-4 w-4" />} توليد
                      </button>
                    </div>
                  </div>
                  <div className="relative">
                    <Search className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                    <input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="ابحث عن كائن..."
                      className="h-10 w-full border border-border bg-secondary pr-9 pl-3 text-sm" />
                  </div>
                  <div className="space-y-4">
                    {filteredCatalog.map(([module, objs]) => (
                      <div key={module}>
                        <p className="mb-1.5 text-xs font-bold uppercase tracking-wider text-muted-foreground">{module}</p>
                        <div className="grid grid-cols-2 gap-2 sm:grid-cols-3">
                          {objs.map((o) => (
                            <button key={o.code} onClick={() => { setObjectCode(o.code); setGroupBy(""); setAggregation("Count"); setAggField(""); if (!titleAr) setTitleAr(o.nameAr); }}
                              className={`border p-3 text-right text-sm transition-colors ${objectCode === o.code ? "border-primary bg-primary/10" : "border-border hover:border-primary/50"}`}>
                              <span className="block font-medium">{o.nameAr}</span>
                              <span className="block text-xs text-muted-foreground">{o.fieldCount} حقل</span>
                            </button>
                          ))}
                        </div>
                      </div>
                    ))}
                    {filteredCatalog.length === 0 && <p className="text-sm text-muted-foreground">لا توجد كائنات مطابقة</p>}
                  </div>
                </div>
              )}

              {/* Step 2 — Property (dimension) */}
              {step === 1 && object && (
                <div className="space-y-3">
                  <p className="text-sm text-muted-foreground">اختر الخاصية التي تريد تجميع البيانات حسبها (اتركها فارغة لمؤشر إجمالي واحد).</p>
                  <div className="grid grid-cols-2 gap-2 sm:grid-cols-3">
                    <button onClick={() => setGroupBy("")} className={`border p-3 text-right text-sm ${!groupBy ? "border-primary bg-primary/10" : "border-border hover:border-primary/50"}`}>
                      <span className="block font-medium">بدون تجميع</span>
                      <span className="block text-xs text-muted-foreground">مؤشر واحد (KPI)</span>
                    </button>
                    {groupableFields.map((f) => (
                      <button key={f.code} onClick={() => setGroupBy(f.code)} disabled={aggregation === "Percentage"}
                        className={`border p-3 text-right text-sm disabled:opacity-40 ${groupBy === f.code ? "border-primary bg-primary/10" : "border-border hover:border-primary/50"}`}>
                        <span className="block font-medium">{f.nameAr}</span>
                        <span className="block text-xs text-muted-foreground">{f.isReference ? "مرجع" : f.isDate ? "تاريخ" : f.fieldType}</span>
                      </button>
                    ))}
                  </div>
                  {groupField?.isDate && (
                    <div className="flex items-center gap-2 pt-2">
                      <label className="text-xs font-bold uppercase tracking-wider text-muted-foreground">التقسيم الزمني</label>
                      <select value={granularity} onChange={(e) => setGranularity(e.target.value)} className="h-9 border border-border bg-secondary px-3 text-sm">
                        <option value="day">يومي</option><option value="week">أسبوعي</option><option value="month">شهري</option><option value="quarter">ربع سنوي</option><option value="year">سنوي</option>
                      </select>
                    </div>
                  )}
                </div>
              )}

              {/* Step 3 — Aggregation */}
              {step === 2 && object && (
                <div className="space-y-3">
                  <p className="text-sm text-muted-foreground">كيف يتم حساب القيمة؟</p>
                  <div className="grid grid-cols-2 gap-2 sm:grid-cols-3">
                    {AGGREGATIONS.filter((a) => !(a.value === "Percentage" && groupBy)).map((a) => (
                      <button key={a.value} onClick={() => { setAggregation(a.value); if (!a.needsField) setAggField(""); }}
                        className={`border p-3 text-right text-sm ${aggregation === a.value ? "border-primary bg-primary/10" : "border-border hover:border-primary/50"}`}>
                        {a.label}
                      </button>
                    ))}
                  </div>
                  {aggDef.needsField && (
                    <div className="space-y-1 pt-2">
                      <label className="text-xs font-bold uppercase tracking-wider text-muted-foreground">الحقل</label>
                      <select value={aggField} onChange={(e) => setAggField(e.target.value)} className="h-9 w-full border border-border bg-secondary px-3 text-sm">
                        <option value="">— اختر حقلاً —</option>
                        {(aggDef.measureOnly ? measureFields : object.fields).map((f) => (
                          <option key={f.code} value={f.code}>{f.nameAr}</option>
                        ))}
                      </select>
                      {aggDef.measureOnly && measureFields.length === 0 && (
                        <p className="text-xs text-destructive">لا توجد حقول رقمية في هذا الكائن</p>
                      )}
                    </div>
                  )}
                </div>
              )}

              {/* Step 4 — Visualization */}
              {step === 3 && (
                <div className="space-y-3">
                  <p className="text-sm text-muted-foreground">اختر طريقة عرض البيانات</p>
                  <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
                    {visOptions.map((v) => (
                      <button key={v} onClick={() => setVisualization(v)}
                        className={`border p-3 text-center text-sm ${visualization === v ? "border-primary bg-primary/10" : "border-border hover:border-primary/50"}`}>
                        {VIS_LABELS[v] ?? v}
                      </button>
                    ))}
                  </div>
                </div>
              )}

              {/* Step 5 — Save */}
              {step === 4 && (
                <div className="space-y-3">
                  <div className="space-y-1">
                    <label className="text-xs font-bold uppercase tracking-wider text-muted-foreground">عنوان العنصر</label>
                    <input value={titleAr} onChange={(e) => setTitleAr(e.target.value)} placeholder="مثال: الموظفون حسب الإدارة"
                      className="h-10 w-full border border-border bg-secondary px-3 text-sm" />
                  </div>
                  <div className="space-y-1">
                    <label className="text-xs font-bold uppercase tracking-wider text-muted-foreground">صلاحية مطلوبة (اختياري)</label>
                    <input value={requiredPermission} onChange={(e) => setRequiredPermission(e.target.value)} placeholder="مثال: Payroll.View — يُخفي العنصر عمن لا يملكها"
                      className="h-10 w-full border border-border bg-secondary px-3 text-sm" dir="ltr" />
                  </div>
                  <div className="border border-border bg-secondary/40 p-3 text-xs text-muted-foreground">
                    <p>الكائن: <span className="text-foreground">{object?.nameAr}</span></p>
                    <p>الحساب: <span className="text-foreground">{aggDef.label}{aggField ? ` (${object?.fields.find((f) => f.code === aggField)?.nameAr})` : ""}</span></p>
                    <p>التجميع: <span className="text-foreground">{groupField?.nameAr ?? "بدون"}</span></p>
                    <p>العرض: <span className="text-foreground">{VIS_LABELS[visualization]}</span></p>
                  </div>
                </div>
              )}
            </>
          )}
        </div>

        {/* Live preview */}
        <div className="min-h-0 border-t border-border bg-background p-4 lg:border-r lg:border-t-0">
          <p className="mb-2 text-xs font-bold uppercase tracking-wider text-muted-foreground">معاينة مباشرة</p>
          <div className="h-64">
            {canPreview ? (
              <WidgetCard spec={spec} visualization={visualization} titleAr={titleAr || object?.nameAr || "معاينة"} enableDrilldown={false} />
            ) : (
              <div className="flex h-full items-center justify-center border border-dashed border-border text-sm text-muted-foreground">
                أكمل الخطوات لعرض المعاينة
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Footer */}
      <div className="flex items-center justify-between border-t border-border px-5 py-3">
        <button onClick={onCancel} className="h-10 px-4 text-sm text-muted-foreground hover:text-foreground">إلغاء</button>
        <div className="flex items-center gap-2">
          {step > 0 && (
            <button onClick={back} className="flex h-10 items-center gap-1 border border-border px-4 text-sm hover:bg-muted">
              <ChevronRight className="h-4 w-4" /> السابق
            </button>
          )}
          {step < STEPS.length - 1 ? (
            <button onClick={next} disabled={!stepValid[step]}
              className="flex h-10 items-center gap-1 bg-primary px-4 text-sm font-bold uppercase tracking-wider text-primary-foreground hover:bg-primary/80 disabled:opacity-40">
              التالي <ChevronLeft className="h-4 w-4" />
            </button>
          ) : (
            <button onClick={() => onSave({ spec, visualization, titleAr: titleAr.trim(), titleEn: titleAr.trim() })} disabled={!canSave || saving}
              className="flex h-10 items-center gap-2 bg-primary px-5 text-sm font-bold uppercase tracking-wider text-primary-foreground hover:bg-primary/80 disabled:opacity-40">
              {saving ? <Loader2 className="h-4 w-4 animate-spin" /> : <Check className="h-4 w-4" />} حفظ العنصر
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
