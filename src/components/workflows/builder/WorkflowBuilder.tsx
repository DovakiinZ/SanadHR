"use client";

import { useMemo, useOptimistic, useState, useTransition } from "react";
import {
  DndContext, closestCenter, PointerSensor, useSensor, useSensors, type DragEndEvent,
} from "@dnd-kit/core";
import {
  SortableContext, verticalListSortingStrategy, useSortable, arrayMove,
} from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import {
  AlertTriangle, ArrowRight, CheckCircle2, Filter, Flag, GitBranch, GripVertical, Loader2,
  Mail, Plus, Save, Square, Trash2, X,
} from "lucide-react";
import { toast } from "sonner";
import { ApiError } from "@/lib/api-client";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  Sheet, SheetContent, SheetHeader, SheetTitle, SheetDescription,
} from "@/components/ui/sheet";
import {
  updateWorkflowDefinition, WorkflowStepType, type WorkflowDefinitionDto,
} from "@/lib/api/workflow-builder";
import {
  newStep, stepsFromDto, stepsToInput, validateGraph, type StepNode,
} from "./model";

const TYPE_ICON: Record<WorkflowStepType, typeof Mail> = {
  [WorkflowStepType.Approval]: CheckCircle2,
  [WorkflowStepType.Action]: Mail,
  [WorkflowStepType.Condition]: Filter,
  [WorkflowStepType.End]: Square,
};
const TYPE_LABEL: Record<WorkflowStepType, string> = {
  [WorkflowStepType.Approval]: "موافقة",
  [WorkflowStepType.Action]: "إجراء (بريد)",
  [WorkflowStepType.Condition]: "شرط",
  [WorkflowStepType.End]: "نهاية",
};

export function WorkflowBuilder({
  def, canEdit, onClose, onSaved,
}: {
  def: WorkflowDefinitionDto;
  canEdit: boolean;
  onClose: () => void;
  onSaved: () => void;
}) {
  const [name, setName] = useState(def.name);
  const [description, setDescription] = useState(def.description ?? "");
  const [isActive, setIsActive] = useState(def.isActive);
  const [steps, setSteps] = useState<StepNode[]>(() => stepsFromDto(def));
  const [rootStepId, setRootStepId] = useState<string | null>(def.rootStepId);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [, startTransition] = useTransition();

  // useOptimistic gives the timeline an instant reorder while React commits the real state in a
  // transition — no flicker between drop and re-render.
  const [optimisticSteps, setOptimisticSteps] = useOptimistic<StepNode[], StepNode[]>(
    steps, (_cur, next) => next
  );

  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 5 } }));
  const selected = steps.find((s) => s.id === selectedId) ?? null;
  const validation = useMemo(() => validateGraph(rootStepId, steps), [rootStepId, steps]);
  const locked = !canEdit;

  function commitSteps(next: StepNode[], nextRoot: string | null = rootStepId) {
    setSteps(next);
    setRootStepId(nextRoot);
  }

  function addStep(type: WorkflowStepType) {
    if (locked) return;
    const step = newStep(type);
    const next = [...steps];
    // Convenience wiring: chain the new step onto the previous tail if it has no success target yet.
    const tail = next[next.length - 1];
    if (tail && tail.type !== WorkflowStepType.End && !tail.nextStepIdSuccess) {
      tail.nextStepIdSuccess = step.id;
    }
    next.push(step);
    commitSteps(next, rootStepId ?? step.id); // first step becomes the root
    setSelectedId(step.id);
  }

  function patchStep(id: string, patch: Partial<StepNode>) {
    commitSteps(steps.map((s) => (s.id === id ? { ...s, ...patch } : s)));
  }
  function patchConfig(id: string, key: string, value: unknown) {
    commitSteps(steps.map((s) => (s.id === id ? { ...s, config: { ...s.config, [key]: value } } : s)));
  }

  function removeStep(id: string) {
    // Drop the step and null out any pointers that referenced it so the graph stays consistent.
    const next = steps
      .filter((s) => s.id !== id)
      .map((s) => ({
        ...s,
        nextStepIdSuccess: s.nextStepIdSuccess === id ? null : s.nextStepIdSuccess,
        nextStepIdFailure: s.nextStepIdFailure === id ? null : s.nextStepIdFailure,
      }));
    commitSteps(next, rootStepId === id ? next[0]?.id ?? null : rootStepId);
    if (selectedId === id) setSelectedId(null);
  }

  function onDragEnd(e: DragEndEvent) {
    const { active, over } = e;
    if (locked || !over || active.id === over.id) return;
    const oldIdx = steps.findIndex((s) => s.id === active.id);
    const newIdx = steps.findIndex((s) => s.id === over.id);
    if (oldIdx < 0 || newIdx < 0) return;
    const reordered = arrayMove(steps, oldIdx, newIdx);
    startTransition(() => {
      setOptimisticSteps(reordered); // instant visual feedback
      setSteps(reordered);           // committed source of truth
    });
  }

  async function save() {
    if (!name.trim()) { toast.error("أدخل اسم المسار"); return; }
    if (validation.errors.length) { toast.error("لا يمكن الحفظ: المخطط يحتوي على أخطاء"); return; }
    setSaving(true);
    try {
      await updateWorkflowDefinition(def.id, {
        name: name.trim(),
        description: description.trim() || null,
        isActive,
        rootStepId,
        steps: stepsToInput(steps),
      });
      toast.success("تم حفظ المسار");
      onSaved();
    } catch (e) {
      toast.error(e instanceof ApiError ? e.message : "تعذر الحفظ");
    } finally {
      setSaving(false);
    }
  }

  const stepLabel = (id: string | null) =>
    id ? steps.find((s) => s.id === id)?.name ?? "—" : "إنهاء (نهاية المسار)";

  return (
    <div className="fixed inset-0 z-40 flex flex-col bg-background">
      {/* Header */}
      <div className="flex items-center gap-3 border-b border-border px-5 py-3">
        <Button variant="ghost" size="icon-sm" onClick={onClose}><X /></Button>
        <div className="flex-1">
          <div className="flex items-center gap-2">
            <GitBranch className="h-4 w-4 text-primary" />
            <Input
              value={name}
              onChange={(e) => setName(e.target.value)}
              disabled={locked}
              className="h-7 max-w-sm border-transparent bg-transparent px-1 text-base font-medium focus-visible:border-input"
            />
            <Badge variant="outline">v{def.version}</Badge>
            <span className="text-xs text-muted-foreground">{def.code}</span>
          </div>
        </div>
        <label className="flex items-center gap-1.5 text-xs text-muted-foreground">
          <input type="checkbox" checked={isActive} disabled={locked} onChange={(e) => setIsActive(e.target.checked)} />
          نشط
        </label>
        <Button onClick={save} disabled={locked || saving || validation.errors.length > 0}>
          {saving ? <Loader2 className="animate-spin" /> : <Save />} حفظ
        </Button>
      </div>

      <div className="flex min-h-0 flex-1">
        {/* Palette */}
        <div className="w-48 shrink-0 space-y-1.5 border-l border-border p-3">
          <p className="mb-1 text-xs font-medium text-muted-foreground">إضافة خطوة</p>
          {([WorkflowStepType.Approval, WorkflowStepType.Action, WorkflowStepType.Condition, WorkflowStepType.End] as const).map((t) => {
            const Icon = TYPE_ICON[t];
            return (
              <button
                key={t}
                onClick={() => addStep(t)}
                disabled={locked}
                className="flex w-full items-center gap-2 rounded-lg border border-border bg-secondary px-2.5 py-2 text-sm hover:bg-muted disabled:opacity-50"
              >
                <Icon className="h-4 w-4 text-primary" /> {TYPE_LABEL[t]} <Plus className="ms-auto h-3.5 w-3.5 text-muted-foreground" />
              </button>
            );
          })}
        </div>

        {/* Timeline */}
        <div className="min-w-0 flex-1 overflow-auto p-6">
          {validation.errors.length > 0 && (
            <Banner tone="error" items={validation.errors} />
          )}
          {validation.warnings.length > 0 && (
            <Banner tone="warn" items={validation.warnings} />
          )}

          {steps.length === 0 ? (
            <div className="mx-auto mt-20 max-w-sm rounded-xl border border-dashed border-border p-8 text-center text-sm text-muted-foreground">
              ابدأ بإضافة خطوة من اللوحة على اليمين. أول خطوة تصبح نقطة البداية.
            </div>
          ) : (
            <div className="mx-auto max-w-2xl">
              <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={onDragEnd}>
                <SortableContext items={optimisticSteps.map((s) => s.id)} strategy={verticalListSortingStrategy}>
                  {optimisticSteps.map((s, i) => (
                    <StepCard
                      key={s.id}
                      step={s}
                      index={i}
                      isRoot={rootStepId === s.id}
                      isSelected={selectedId === s.id}
                      locked={locked}
                      successLabel={stepLabel(s.nextStepIdSuccess)}
                      failureLabel={stepLabel(s.nextStepIdFailure)}
                      onSelect={() => setSelectedId(s.id)}
                      onRemove={() => removeStep(s.id)}
                      onSetRoot={() => setRootStepId(s.id)}
                    />
                  ))}
                </SortableContext>
              </DndContext>
            </div>
          )}
        </div>
      </div>

      {/* Config drawer */}
      <Sheet open={!!selected} onOpenChange={(o) => !o && setSelectedId(null)}>
        <SheetContent className="w-full sm:max-w-md">
          {selected && (
            <>
              <SheetHeader>
                <SheetTitle>إعدادات الخطوة</SheetTitle>
                <SheetDescription>{TYPE_LABEL[selected.type]}</SheetDescription>
              </SheetHeader>
              <div className="space-y-4 overflow-auto px-4 pb-4">
                <Field label="اسم الخطوة">
                  <Input value={selected.name} disabled={locked}
                    onChange={(e) => patchStep(selected.id, { name: e.target.value })} />
                </Field>

                <StepConfigEditor step={selected} locked={locked} onConfig={(k, v) => patchConfig(selected.id, k, v)} />

                {/* Branch wiring */}
                <div className="rounded-lg border border-border p-3">
                  <p className="mb-2 text-xs font-medium text-muted-foreground">الانتقالات</p>
                  <Field label={selected.type === WorkflowStepType.Approval ? "عند الموافقة ←" : "عند النجاح ←"}>
                    <BranchSelect
                      value={selected.nextStepIdSuccess}
                      options={steps.filter((s) => s.id !== selected.id)}
                      disabled={locked}
                      onChange={(v) => patchStep(selected.id, { nextStepIdSuccess: v })}
                    />
                  </Field>
                  {selected.type !== WorkflowStepType.Action && selected.type !== WorkflowStepType.End && (
                    <Field label={selected.type === WorkflowStepType.Approval ? "عند الرفض ←" : "عند الفشل ←"}>
                      <BranchSelect
                        value={selected.nextStepIdFailure}
                        options={steps.filter((s) => s.id !== selected.id)}
                        disabled={locked}
                        onChange={(v) => patchStep(selected.id, { nextStepIdFailure: v })}
                      />
                    </Field>
                  )}
                </div>

                <div className="flex items-center justify-between">
                  <Button variant="outline" size="sm" disabled={locked} onClick={() => { setRootStepId(selected.id); }}>
                    <Flag /> تعيين كبداية
                  </Button>
                  <Button variant="destructive" size="sm" disabled={locked} onClick={() => removeStep(selected.id)}>
                    <Trash2 /> حذف الخطوة
                  </Button>
                </div>
              </div>
            </>
          )}
        </SheetContent>
      </Sheet>
    </div>
  );
}

function Banner({ tone, items }: { tone: "error" | "warn"; items: string[] }) {
  return (
    <div className={`mb-4 rounded-lg border p-3 text-sm ${
      tone === "error" ? "border-destructive/40 bg-destructive/10 text-destructive" : "border-amber-500/40 bg-amber-500/10 text-amber-600"
    }`}>
      <div className="mb-1 flex items-center gap-1.5 font-medium">
        <AlertTriangle className="h-4 w-4" /> {tone === "error" ? "أخطاء يجب إصلاحها قبل الحفظ" : "تنبيهات"}
      </div>
      <ul className="list-inside list-disc space-y-0.5">
        {items.map((m, i) => <li key={i}>{m}</li>)}
      </ul>
    </div>
  );
}

function StepCard({
  step, index, isRoot, isSelected, locked, successLabel, failureLabel, onSelect, onRemove, onSetRoot,
}: {
  step: StepNode; index: number; isRoot: boolean; isSelected: boolean; locked: boolean;
  successLabel: string; failureLabel: string;
  onSelect: () => void; onRemove: () => void; onSetRoot: () => void;
}) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({ id: step.id, disabled: locked });
  const Icon = TYPE_ICON[step.type];
  const style = { transform: CSS.Transform.toString(transform), transition, opacity: isDragging ? 0.6 : 1 };
  const showFailure = step.type !== WorkflowStepType.Action && step.type !== WorkflowStepType.End;

  return (
    <div ref={setNodeRef} style={style} className="relative pb-6">
      <div
        onClick={onSelect}
        className={`flex cursor-pointer items-start gap-3 rounded-xl border bg-card p-3 transition-colors ${
          isSelected ? "border-primary ring-1 ring-primary/30" : "border-border hover:border-input"
        }`}
      >
        <button {...attributes} {...listeners} onClick={(e) => e.stopPropagation()}
          className="mt-0.5 cursor-grab text-muted-foreground hover:text-foreground active:cursor-grabbing" aria-label="إعادة ترتيب">
          <GripVertical className="h-4 w-4" />
        </button>
        <div className="flex h-7 w-7 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
          <Icon className="h-4 w-4" />
        </div>
        <div className="min-w-0 flex-1">
          <div className="flex items-center gap-2">
            <span className="truncate text-sm font-medium">{step.name}</span>
            <Badge variant="secondary">{TYPE_LABEL[step.type]}</Badge>
            {isRoot && <Badge><Flag className="h-3 w-3" /> بداية</Badge>}
          </div>
          <div className="mt-1 flex flex-wrap gap-x-4 gap-y-0.5 text-xs text-muted-foreground">
            <span className="inline-flex items-center gap-1"><ArrowRight className="h-3 w-3 text-emerald-500" /> {successLabel}</span>
            {showFailure && <span className="inline-flex items-center gap-1"><ArrowRight className="h-3 w-3 text-destructive" /> {failureLabel}</span>}
          </div>
        </div>
        <div className="flex shrink-0 items-center gap-1" onClick={(e) => e.stopPropagation()}>
          {!isRoot && !locked && (
            <Button variant="ghost" size="icon-xs" title="تعيين كبداية" onClick={onSetRoot}><Flag /></Button>
          )}
          {!locked && <Button variant="ghost" size="icon-xs" title="حذف" onClick={onRemove}><Trash2 /></Button>}
        </div>
      </div>
      {/* connector */}
      <div className="absolute bottom-0 right-[1.85rem] h-6 w-px bg-border" />
    </div>
  );
}

function StepConfigEditor({
  step, locked, onConfig,
}: { step: StepNode; locked: boolean; onConfig: (key: string, value: unknown) => void }) {
  const c = step.config as Record<string, string>;
  if (step.type === WorkflowStepType.Approval) {
    return (
      <>
        <Field label="دور المعتمد (Role)">
          <Input value={c.approverRole ?? ""} disabled={locked} placeholder="مثال: Manager"
            onChange={(e) => onConfig("approverRole", e.target.value)} />
        </Field>
        <Field label="أو معرّف مستخدم محدّد (اختياري)">
          <Input value={c.approverUserId ?? ""} disabled={locked} placeholder="GUID"
            onChange={(e) => onConfig("approverUserId", e.target.value)} />
        </Field>
      </>
    );
  }
  if (step.type === WorkflowStepType.Action) {
    return (
      <>
        <Field label="البريد المستلم">
          <Input value={c.toEmail ?? ""} disabled={locked} placeholder="hr@company.com"
            onChange={(e) => onConfig("toEmail", e.target.value)} />
        </Field>
        <Field label="الموضوع">
          <Input value={c.subject ?? ""} disabled={locked} placeholder="يدعم {{field}} من البيانات"
            onChange={(e) => onConfig("subject", e.target.value)} />
        </Field>
        <Field label="النص">
          <textarea value={c.body ?? ""} disabled={locked} rows={4}
            onChange={(e) => onConfig("body", e.target.value)}
            className="w-full rounded-lg border border-input bg-transparent px-2.5 py-1.5 text-sm outline-none focus-visible:border-ring" />
        </Field>
      </>
    );
  }
  if (step.type === WorkflowStepType.Condition) {
    return (
      <div className="grid grid-cols-3 gap-2">
        <Field label="الحقل">
          <Input value={c.field ?? ""} disabled={locked} placeholder="amount"
            onChange={(e) => onConfig("field", e.target.value)} />
        </Field>
        <Field label="العامل">
          <select value={c.operator ?? "eq"} disabled={locked}
            onChange={(e) => onConfig("operator", e.target.value)}
            className="h-8 w-full rounded-lg border border-input bg-transparent px-2 text-sm outline-none focus-visible:border-ring">
            {["eq", "neq", "gt", "gte", "lt", "lte", "contains"].map((o) => <option key={o} value={o}>{o}</option>)}
          </select>
        </Field>
        <Field label="القيمة">
          <Input value={c.value ?? ""} disabled={locked} placeholder="1000"
            onChange={(e) => onConfig("value", e.target.value)} />
        </Field>
      </div>
    );
  }
  return <p className="text-xs text-muted-foreground">خطوة النهاية تُنهي الطلب عند الوصول إليها.</p>;
}

function BranchSelect({
  value, options, disabled, onChange,
}: { value: string | null; options: StepNode[]; disabled: boolean; onChange: (v: string | null) => void }) {
  return (
    <select
      value={value ?? ""}
      disabled={disabled}
      onChange={(e) => onChange(e.target.value || null)}
      className="h-8 w-full rounded-lg border border-input bg-transparent px-2 text-sm outline-none focus-visible:border-ring"
    >
      <option value="">إنهاء (نهاية المسار)</option>
      {options.map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
    </select>
  );
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <label className="block space-y-1">
      <span className="text-xs font-medium text-muted-foreground">{label}</span>
      {children}
    </label>
  );
}
