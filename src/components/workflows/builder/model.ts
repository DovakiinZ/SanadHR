import {
  WorkflowStepType,
  type WorkflowDefinitionDto,
  type WorkflowStepInput,
} from "@/lib/api/workflow-builder";

// Client-side editable representation of a step. `id` is a real UUID generated up-front so the
// success/failure pointers are valid before the graph is ever saved (the backend reuses these ids).
export interface StepNode {
  id: string;
  type: WorkflowStepType;
  name: string;
  config: Record<string, unknown>;
  nextStepIdSuccess: string | null;
  nextStepIdFailure: string | null;
}

export function newStepId(): string {
  // Browser crypto is available in all target environments; fall back just in case.
  return typeof crypto !== "undefined" && "randomUUID" in crypto
    ? crypto.randomUUID()
    : "00000000-0000-4000-8000-" + Date.now().toString(16).padStart(12, "0");
}

const DEFAULT_NAME: Record<WorkflowStepType, string> = {
  [WorkflowStepType.Approval]: "موافقة",
  [WorkflowStepType.Action]: "إرسال بريد",
  [WorkflowStepType.Condition]: "شرط",
  [WorkflowStepType.End]: "نهاية",
};

export function newStep(type: WorkflowStepType): StepNode {
  const config: Record<string, unknown> =
    type === WorkflowStepType.Action
      ? { actionType: "email", toEmail: "", subject: "", body: "" }
      : type === WorkflowStepType.Condition
      ? { field: "", operator: "eq", value: "" }
      : type === WorkflowStepType.Approval
      ? { approverRole: "" }
      : {};
  return {
    id: newStepId(),
    type,
    name: DEFAULT_NAME[type],
    config,
    nextStepIdSuccess: null,
    nextStepIdFailure: null,
  };
}

export function stepsFromDto(def: WorkflowDefinitionDto): StepNode[] {
  return [...def.steps]
    .sort((a, b) => a.sortOrder - b.sortOrder)
    .map((s) => {
      let config: Record<string, unknown> = {};
      try {
        config = s.config ? (JSON.parse(s.config) as Record<string, unknown>) : {};
      } catch {
        config = {};
      }
      return {
        id: s.id,
        type: s.type,
        name: s.name,
        config,
        nextStepIdSuccess: s.nextStepIdSuccess,
        nextStepIdFailure: s.nextStepIdFailure,
      };
    });
}

export function stepsToInput(steps: StepNode[]): WorkflowStepInput[] {
  return steps.map((s, i) => ({
    id: s.id,
    type: s.type,
    name: s.name.trim() || "خطوة",
    config: JSON.stringify(s.config ?? {}),
    nextStepIdSuccess: s.nextStepIdSuccess,
    nextStepIdFailure: s.nextStepIdFailure,
    sortOrder: i,
  }));
}

export interface ValidationResult {
  errors: string[];
  warnings: string[];
}

/**
 * Live mirror of the server-side WorkflowGraphValidator: dangling pointers, a valid root, no
 * circular references, and reachability/end-state checks. Runs on every edit so the builder can
 * block an invalid save before the request is ever made.
 */
export function validateGraph(rootStepId: string | null, steps: StepNode[]): ValidationResult {
  const result: ValidationResult = { errors: [], warnings: [] };
  if (steps.length === 0) return result;

  const byId = new Map(steps.map((s) => [s.id, s]));

  if (!rootStepId) result.errors.push("لم يتم تحديد خطوة البداية.");
  else if (!byId.has(rootStepId)) result.errors.push("خطوة البداية تشير إلى خطوة غير موجودة.");

  for (const s of steps) {
    for (const [next, branch] of [
      [s.nextStepIdSuccess, "النجاح"],
      [s.nextStepIdFailure, "الفشل"],
    ] as const) {
      if (!next) continue;
      if (next === s.id) result.errors.push(`الخطوة "${s.name}" تشير إلى نفسها في مسار ${branch}.`);
      else if (!byId.has(next)) result.errors.push(`الخطوة "${s.name}" لها مسار ${branch} إلى خطوة غير موجودة.`);
    }
  }

  if (detectsCycle(steps, byId)) result.errors.push("تم اكتشاف مرجع دائري — يمكن الوصول إلى خطوة من نفسها.");

  if (result.errors.length === 0 && rootStepId) {
    const reachable = reachableFrom(rootStepId, byId);
    for (const s of steps)
      if (!reachable.has(s.id)) result.warnings.push(`الخطوة "${s.name}" غير قابلة للوصول من البداية.`);
    for (const s of steps)
      if (s.type === WorkflowStepType.End && (s.nextStepIdSuccess || s.nextStepIdFailure))
        result.warnings.push(`خطوة النهاية "${s.name}" يجب ألا يكون لها مسارات خارجة.`);
  }

  return result;
}

function detectsCycle(steps: StepNode[], byId: Map<string, StepNode>): boolean {
  const state = new Map<string, number>(); // 0 white, 1 gray, 2 black
  const visit = (id: string): boolean => {
    state.set(id, 1);
    const node = byId.get(id);
    if (node) {
      for (const next of [node.nextStepIdSuccess, node.nextStepIdFailure]) {
        if (!next || !byId.has(next)) continue;
        const s = state.get(next) ?? 0;
        if (s === 1) return true;
        if (s === 0 && visit(next)) return true;
      }
    }
    state.set(id, 2);
    return false;
  };
  for (const s of steps) if ((state.get(s.id) ?? 0) === 0 && visit(s.id)) return true;
  return false;
}

function reachableFrom(root: string, byId: Map<string, StepNode>): Set<string> {
  const seen = new Set<string>();
  const stack = [root];
  while (stack.length) {
    const id = stack.pop()!;
    if (seen.has(id)) continue;
    seen.add(id);
    const node = byId.get(id);
    if (!node) continue;
    if (node.nextStepIdSuccess) stack.push(node.nextStepIdSuccess);
    if (node.nextStepIdFailure) stack.push(node.nextStepIdFailure);
  }
  return seen;
}
