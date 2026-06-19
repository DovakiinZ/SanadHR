"use client";

import { useEffect, useMemo, useState } from "react";
import {
  ArrowDown, ArrowUp, Check, ChevronLeft, ChevronRight, Loader2, Plus, Trash2, User, X,
} from "lucide-react";
import { toast } from "sonner";
import { ApiError } from "@/lib/api-client";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Combobox, type ComboboxOption } from "@/components/ui/combobox";
import {
  APPROVER_TYPES, ApproverType, CONDITION_FIELDS, CONDITION_OPERATORS,
  createApprovalWorkflow, getApprovalWorkflow, getRequestTypesAdmin, updateApprovalWorkflow,
  type ApprovalWorkflowStep, type RequestTypeAdmin,
} from "@/lib/api/approval-workflows";
import { getEmployees } from "@/lib/api/employees";
import { getRoles } from "@/lib/api/notification-rules";
import { getDepartments, getBranches } from "@/lib/api/org";
import { getLookup, lookupLabel } from "@/lib/api/lookups";

function blankStep(approverType = ApproverType.DirectManager): ApprovalWorkflowStep {
  return {
    approverType, nameAr: "", nameEn: "", specificEntityId: null, chainLevel: 1,
    required: true, canReject: true, canReturn: true, canDelegate: false, conditions: [],
  };
}

interface Lookups {
  employees: ComboboxOption[];
  roles: ComboboxOption[];
  departments: ComboboxOption[];
  branches: ComboboxOption[];
  leaveTypes: ComboboxOption[];
  employmentTypes: ComboboxOption[];
  jobTitles: ComboboxOption[];
}
const emptyLookups: Lookups = { employees: [], roles: [], departments: [], branches: [], leaveTypes: [], employmentTypes: [], jobTitles: [] };

export function ApprovalWorkflowWizard({
  workflowId, initialName, initialSteps, onClose, onSaved,
}: {
  workflowId: string | null;
  initialName?: string;
  initialSteps?: ApprovalWorkflowStep[];
  onClose: () => void;
  onSaved: () => void;
}) {
  const [phase, setPhase] = useState<1 | 2>(1);
  const [name, setName] = useState(initialName ?? "");
  const [description, setDescription] = useState("");
  const [requestTypeIds, setRequestTypeIds] = useState<string[]>([]);
  const [steps, setSteps] = useState<ApprovalWorkflowStep[]>(initialSteps?.length ? initialSteps : [blankStep()]);
  const [requestTypes, setRequestTypes] = useState<RequestTypeAdmin[]>([]);
  const [lk, setLk] = useState<Lookups>(emptyLookups);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    (async () => {
      setLoading(true);
      const [types, emps, roles, depts, brs, leaves, empTypes, jobs] = await Promise.allSettled([
        getRequestTypesAdmin(), getEmployees({ pageSize: 300 }), getRoles(),
        getDepartments(), getBranches(), getLookup("leave-types"), getLookup("employment-types"), getLookup("job-titles"),
      ]);
      if (types.status === "fulfilled") setRequestTypes(types.value ?? []);
      setLk({
        employees: emps.status === "fulfilled" ? emps.value.map((e) => ({ value: e.id, label: e.name, hint: e.employeeId })) : [],
        roles: roles.status === "fulfilled" ? roles.value.map((r) => ({ value: r.id, label: r.nameAr || r.name })) : [],
        departments: depts.status === "fulfilled" ? depts.value.map((d) => ({ value: d.id, label: d.nameAr || d.name })) : [],
        branches: brs.status === "fulfilled" ? brs.value.map((b) => ({ value: b.id, label: b.nameAr || b.name })) : [],
        leaveTypes: leaves.status === "fulfilled" ? leaves.value.map((l) => ({ value: l.id, label: lookupLabel(l) })) : [],
        employmentTypes: empTypes.status === "fulfilled" ? empTypes.value.map((l) => ({ value: l.id, label: lookupLabel(l) })) : [],
        jobTitles: jobs.status === "fulfilled" ? jobs.value.map((l) => ({ value: l.id, label: lookupLabel(l) })) : [],
      });

      if (workflowId) {
        try {
          const wf = await getApprovalWorkflow(workflowId);
          setName(wf.name); setDescription(wf.description ?? "");
          setRequestTypeIds(wf.requestTypeIds);
          setSteps(wf.steps.length ? wf.steps : [blankStep()]);
        } catch (e) { toast.error(e instanceof ApiError ? e.message : "تعذر تحميل المسار"); }
      }
      setLoading(false);
    })();
  }, [workflowId]);

  const optionsFor = (kind: string): ComboboxOption[] =>
    kind === "department" ? lk.departments : kind === "branch" ? lk.branches
    : kind === "leaveType" ? lk.leaveTypes : kind === "employmentType" ? lk.employmentTypes
    : kind === "jobTitle" ? lk.jobTitles : [];

  // ── step mutations ──
  const setStep = (i: number, patch: Partial<ApprovalWorkflowStep>) =>
    setSteps((s) => s.map((st, idx) => (idx === i ? { ...st, ...patch } : st)));
  const addStep = () => setSteps((s) => [...s, blankStep()]);
  const removeStep = (i: number) => setSteps((s) => s.filter((_, idx) => idx !== i));
  const move = (i: number, dir: -1 | 1) => setSteps((s) => {
    const j = i + dir; if (j < 0 || j >= s.length) return s;
    const copy = [...s]; [copy[i], copy[j]] = [copy[j], copy[i]]; return copy;
  });

  const labelFor = (s: ApprovalWorkflowStep): string => {
    const base = APPROVER_TYPES.find((a) => a.value === s.approverType);
    const needs = base?.needs;
    if (needs === "employee") return `موظف: ${lk.employees.find((e) => e.value === s.specificEntityId)?.label ?? "—"}`;
    if (needs === "role") return `${lk.roles.find((r) => r.value === s.specificEntityId)?.label ?? "دور"} (دور)`;
    if (needs === "level") return `سلسلة المدراء (مستوى ${s.chainLevel})`;
    return base?.label ?? "خطوة";
  };

  // ── validation ──
  const stepErrors = useMemo(() => steps.map((s) => {
    const needs = APPROVER_TYPES.find((a) => a.value === s.approverType)?.needs;
    if ((needs === "employee" || needs === "role") && !s.specificEntityId) return "اختر الجهة المعتمِدة";
    return null;
  }), [steps]);
  const errors = useMemo(() => {
    const e: string[] = [];
    if (!name.trim()) e.push("أدخل اسم المسار");
    if (steps.length === 0) e.push("أضف خطوة واحدة على الأقل");
    if (stepErrors.some(Boolean)) e.push("أكمل بيانات كل خطوة (الجهة المعتمِدة)");
    return e;
  }, [name, steps, stepErrors]);

  async function save() {
    if (errors.length) { toast.error(errors[0]); return; }
    setSaving(true);
    try {
      // Stamp a readable name on each step from its approver type + entity.
      const body = {
        name: name.trim(), description: description.trim() || null, isActive: true, requestTypeIds,
        steps: steps.map((s) => ({ ...s, nameAr: s.nameAr?.trim() || labelFor(s), nameEn: s.nameEn?.trim() || labelFor(s) })),
      };
      if (workflowId) { await updateApprovalWorkflow(workflowId, body); toast.success("تم حفظ المسار"); }
      else { await createApprovalWorkflow(body); toast.success("تم إنشاء المسار"); }
      onSaved();
    } catch (e) { toast.error(e instanceof ApiError ? e.message : "تعذر الحفظ"); }
    finally { setSaving(false); }
  }

  return (
    <div className="fixed inset-0 z-40 flex flex-col bg-background">
      {/* header */}
      <div className="flex items-center gap-3 border-b border-border px-5 py-3">
        <Button variant="ghost" size="icon-sm" onClick={onClose}><X /></Button>
        <h2 className="flex-1 text-base font-semibold">{workflowId ? "تعديل مسار الموافقة" : "مسار موافقة جديد"}</h2>
        <Stepper phase={phase} />
      </div>

      {loading ? (
        <div className="flex flex-1 items-center justify-center"><Loader2 className="animate-spin text-muted-foreground" /></div>
      ) : (
        <div className="mx-auto w-full max-w-3xl flex-1 overflow-auto p-6">
          {phase === 1 ? (
            <div className="space-y-6">
              <Section title="١) التعريف">
                <Field label="اسم المسار">
                  <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="مثال: اعتماد الإجازة السنوية" />
                </Field>
                <Field label="الوصف (اختياري)">
                  <Input value={description} onChange={(e) => setDescription(e.target.value)} />
                </Field>
              </Section>

              <Section title="أنواع الطلبات المرتبطة">
                <p className="text-xs text-muted-foreground">اختر أنواع الطلبات التي ستستخدم هذا المسار.</p>
                <div className="grid max-h-72 grid-cols-1 gap-1.5 overflow-auto sm:grid-cols-2">
                  {requestTypes.map((t) => {
                    const checked = requestTypeIds.includes(t.id);
                    const assignedElsewhere = t.workflowName && !checked;
                    return (
                      <label key={t.id} className={`flex cursor-pointer items-center gap-2 rounded-lg border px-3 py-2 text-sm ${checked ? "border-primary bg-primary/5" : "border-border"}`}>
                        <input type="checkbox" checked={checked}
                          onChange={(e) => setRequestTypeIds((ids) => e.target.checked ? [...ids, t.id] : ids.filter((x) => x !== t.id))} />
                        <span className="truncate">{t.nameAr}</span>
                        {assignedElsewhere && <span className="ms-auto truncate text-[10px] text-amber-500" title="مرتبط حالياً بمسار آخر">{t.workflowName}</span>}
                      </label>
                    );
                  })}
                  {requestTypes.length === 0 && <p className="text-xs text-muted-foreground">لا توجد أنواع طلبات.</p>}
                </div>
              </Section>
            </div>
          ) : (
            <div className="space-y-4">
              <Section title="٢) مسار الموافقة">
                <Timeline
                  steps={steps} stepErrors={stepErrors} lk={lk} optionsFor={optionsFor} labelFor={labelFor}
                  onSet={setStep} onRemove={removeStep} onMove={move}
                />
                <Button variant="outline" size="sm" onClick={addStep}><Plus /> إضافة خطوة</Button>
              </Section>
            </div>
          )}
        </div>
      )}

      {/* footer */}
      <div className="flex items-center justify-between border-t border-border px-5 py-3">
        <div className="text-xs">
          {phase === 2 && errors.length > 0
            ? <span className="text-destructive">{errors[0]}</span>
            : <span className="text-muted-foreground">© 2026 · صُمّم بواسطة <span className="font-medium text-foreground/70">Dovakin</span></span>}
        </div>
        <div className="flex items-center gap-2">
          {phase === 2 && <Button variant="outline" onClick={() => setPhase(1)}><ChevronRight /> السابق</Button>}
          {phase === 1 ? (
            <Button onClick={() => setPhase(2)} disabled={!name.trim()}>التالي <ChevronLeft /></Button>
          ) : (
            <Button onClick={save} disabled={saving || errors.length > 0}>
              {saving ? <Loader2 className="animate-spin" /> : <Check />} حفظ المسار
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}

function Stepper({ phase }: { phase: 1 | 2 }) {
  return (
    <div className="flex items-center gap-2 text-xs">
      {[1, 2].map((n) => (
        <span key={n} className={`flex h-6 w-6 items-center justify-center rounded-full ${phase >= n ? "bg-primary text-primary-foreground" : "bg-secondary text-muted-foreground"}`}>{n}</span>
      ))}
    </div>
  );
}

function Timeline({
  steps, stepErrors, lk, optionsFor, labelFor, onSet, onRemove, onMove,
}: {
  steps: ApprovalWorkflowStep[];
  stepErrors: (string | null)[];
  lk: Lookups;
  optionsFor: (kind: string) => ComboboxOption[];
  labelFor: (s: ApprovalWorkflowStep) => string;
  onSet: (i: number, patch: Partial<ApprovalWorkflowStep>) => void;
  onRemove: (i: number) => void;
  onMove: (i: number, dir: -1 | 1) => void;
}) {
  return (
    <div className="space-y-2">
      <Node icon={<User className="h-4 w-4" />} label="الموظف (مقدّم الطلب)" tone="muted" />
      {steps.map((s, i) => (
        <div key={i}>
          <Connector />
          <StepEditor
            index={i} step={s} error={stepErrors[i]} lk={lk} optionsFor={optionsFor} labelFor={labelFor}
            canUp={i > 0} canDown={i < steps.length - 1}
            onSet={(p) => onSet(i, p)} onRemove={() => onRemove(i)} onMove={(d) => onMove(i, d)}
          />
        </div>
      ))}
      <Connector />
      <Node icon={<Check className="h-4 w-4" />} label="تمت الموافقة" tone="success" />
    </div>
  );
}

function StepEditor({
  index, step, error, lk, optionsFor, labelFor, canUp, canDown, onSet, onRemove, onMove,
}: {
  index: number; step: ApprovalWorkflowStep; error: string | null;
  lk: Lookups; optionsFor: (kind: string) => ComboboxOption[]; labelFor: (s: ApprovalWorkflowStep) => string;
  canUp: boolean; canDown: boolean;
  onSet: (patch: Partial<ApprovalWorkflowStep>) => void; onRemove: () => void; onMove: (dir: -1 | 1) => void;
}) {
  const needs = APPROVER_TYPES.find((a) => a.value === step.approverType)?.needs ?? "none";
  return (
    <div className={`rounded-xl border bg-card p-3 ${error ? "border-destructive/50" : "border-border"}`}>
      <div className="flex items-center gap-2">
        <span className="flex h-6 w-6 items-center justify-center rounded-full bg-primary/10 text-xs font-bold text-primary">{index + 1}</span>
        <span className="flex-1 truncate text-sm font-medium">{labelFor(step)}</span>
        <Button variant="ghost" size="icon-xs" disabled={!canUp} onClick={() => onMove(-1)}><ArrowUp /></Button>
        <Button variant="ghost" size="icon-xs" disabled={!canDown} onClick={() => onMove(1)}><ArrowDown /></Button>
        <Button variant="ghost" size="icon-xs" onClick={onRemove}><Trash2 /></Button>
      </div>

      <div className="mt-3 grid grid-cols-1 gap-3 sm:grid-cols-2">
        <Field label="نوع المعتمِد">
          <select value={step.approverType} onChange={(e) => onSet({ approverType: Number(e.target.value), specificEntityId: null })}
            className="h-9 w-full rounded-lg border border-input bg-secondary px-2.5 text-sm outline-none focus-visible:border-ring">
            {APPROVER_TYPES.map((a) => <option key={a.value} value={a.value}>{a.label}</option>)}
          </select>
        </Field>

        {needs === "employee" && (
          <Field label="الموظف"><Combobox value={step.specificEntityId} onChange={(v) => onSet({ specificEntityId: v })} options={lk.employees} placeholder="اختر موظفاً" /></Field>
        )}
        {needs === "role" && (
          <Field label="الدور / الفريق"><Combobox value={step.specificEntityId} onChange={(v) => onSet({ specificEntityId: v })} options={lk.roles} placeholder="اختر دوراً" /></Field>
        )}
        {needs === "level" && (
          <Field label="المستوى (1 = المدير المباشر)">
            <Input type="number" min={1} value={step.chainLevel} onChange={(e) => onSet({ chainLevel: Math.max(1, Number(e.target.value) || 1) })} />
          </Field>
        )}
      </div>

      {/* rules */}
      <div className="mt-3 flex flex-wrap gap-x-4 gap-y-1.5 text-xs">
        <Toggle label="مطلوبة" checked={step.required} onChange={(v) => onSet({ required: v })} />
        <Toggle label="يمكن الرفض" checked={step.canReject} onChange={(v) => onSet({ canReject: v })} />
        <Toggle label="يمكن الإرجاع" checked={step.canReturn} onChange={(v) => onSet({ canReturn: v })} />
        <Toggle label="يمكن التفويض" checked={step.canDelegate} onChange={(v) => onSet({ canDelegate: v })} />
      </div>

      <ConditionEditor conditions={step.conditions} optionsFor={optionsFor} onChange={(c) => onSet({ conditions: c })} />
      {error && <p className="mt-2 text-xs text-destructive">{error}</p>}
    </div>
  );
}

function ConditionEditor({
  conditions, optionsFor, onChange,
}: {
  conditions: ApprovalWorkflowStep["conditions"];
  optionsFor: (kind: string) => ComboboxOption[];
  onChange: (c: ApprovalWorkflowStep["conditions"]) => void;
}) {
  const add = () => onChange([...conditions, { field: "leaveDays", operator: "gt", value: "" }]);
  const set = (i: number, patch: Partial<(typeof conditions)[number]>) => onChange(conditions.map((c, idx) => (idx === i ? { ...c, ...patch } : c)));
  const remove = (i: number) => onChange(conditions.filter((_, idx) => idx !== i));

  return (
    <div className="mt-3 rounded-lg border border-dashed border-border p-2.5">
      <div className="mb-1.5 flex items-center justify-between">
        <span className="text-xs font-medium text-muted-foreground">شروط تطبيق الخطوة (اختياري — تُطبّق كلها معاً)</span>
        <Button variant="ghost" size="xs" onClick={add}><Plus /> شرط</Button>
      </div>
      {conditions.length === 0 ? (
        <p className="text-[11px] text-muted-foreground">بدون شروط — الخطوة تُطبّق دائماً.</p>
      ) : (
        <div className="space-y-1.5">
          {conditions.map((c, i) => {
            const fieldDef = CONDITION_FIELDS.find((f) => f.value === c.field);
            const kind = fieldDef?.kind ?? "number";
            return (
              <div key={i} className="grid grid-cols-[1fr_auto_1fr_auto] items-center gap-1.5">
                <select value={c.field} onChange={(e) => set(i, { field: e.target.value, value: "" })}
                  className="h-8 rounded-lg border border-input bg-secondary px-2 text-xs outline-none">
                  {CONDITION_FIELDS.map((f) => <option key={f.value} value={f.value}>{f.label}</option>)}
                </select>
                <select value={c.operator} onChange={(e) => set(i, { operator: e.target.value })}
                  className="h-8 rounded-lg border border-input bg-secondary px-2 text-xs outline-none">
                  {CONDITION_OPERATORS.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
                </select>
                {kind === "number" ? (
                  <Input type="number" value={c.value} onChange={(e) => set(i, { value: e.target.value })} className="h-8" placeholder="القيمة" />
                ) : (
                  <Combobox value={c.value || null} onChange={(v) => set(i, { value: v ?? "" })} options={optionsFor(kind)} placeholder="اختر" />
                )}
                <Button variant="ghost" size="icon-xs" onClick={() => remove(i)}><X /></Button>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}

function Node({ icon, label, tone }: { icon: React.ReactNode; label: string; tone: "muted" | "success" }) {
  return (
    <div className={`flex items-center gap-2 rounded-xl border px-3 py-2 text-sm ${tone === "success" ? "border-emerald-500/30 bg-emerald-500/5 text-emerald-600" : "border-border bg-secondary text-muted-foreground"}`}>
      {icon} {label}
    </div>
  );
}
function Connector() { return <div className="mr-[1.4rem] h-3 w-px bg-border" />; }

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="space-y-3">
      <h3 className="text-xs font-bold uppercase tracking-wider text-primary">{title}</h3>
      {children}
    </div>
  );
}
function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return <label className="block space-y-1"><span className="text-xs font-medium text-muted-foreground">{label}</span>{children}</label>;
}
function Toggle({ label, checked, onChange }: { label: string; checked: boolean; onChange: (v: boolean) => void }) {
  return <label className="flex cursor-pointer items-center gap-1.5"><input type="checkbox" checked={checked} onChange={(e) => onChange(e.target.checked)} /> {label}</label>;
}
