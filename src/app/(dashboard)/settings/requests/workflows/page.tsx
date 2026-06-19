"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { ArrowRight, Copy, GitBranch, Layers, Loader2, Pencil, Plus, Power, Trash2, X } from "lucide-react";
import { toast } from "sonner";
import { ApiError } from "@/lib/api-client";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  deleteApprovalWorkflow, duplicateApprovalWorkflow, listApprovalWorkflows,
  type ApprovalWorkflowListItem, type ApprovalWorkflowStep,
} from "@/lib/api/approval-workflows";
import { ApprovalWorkflowWizard } from "@/components/workflows/wizard/ApprovalWorkflowWizard";
import { WORKFLOW_TEMPLATES } from "@/components/workflows/wizard/templates";
import { DesignerCredit } from "@/components/workflows/designer-credit";

type WizardState =
  | { mode: "edit"; id: string }
  | { mode: "new"; name?: string; steps?: ApprovalWorkflowStep[] }
  | null;

export default function ApprovalWorkflowsPage() {
  const [items, setItems] = useState<ApprovalWorkflowListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [wizard, setWizard] = useState<WizardState>(null);
  const [templatePicker, setTemplatePicker] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try { setItems((await listApprovalWorkflows()) ?? []); }
    catch (e) { toast.error(e instanceof ApiError ? e.message : "تعذر تحميل المسارات"); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => { load(); }, [load]);

  async function duplicate(id: string) {
    try { await duplicateApprovalWorkflow(id); toast.success("تم النسخ"); await load(); }
    catch (e) { toast.error(e instanceof ApiError ? e.message : "تعذر النسخ"); }
  }
  async function remove(w: ApprovalWorkflowListItem) {
    if (!confirm(`حذف المسار "${w.name}"؟ سيتم إلغاء ربطه بأنواع الطلبات.`)) return;
    try { await deleteApprovalWorkflow(w.id); toast.success("تم الحذف"); await load(); }
    catch (e) { toast.error(e instanceof ApiError ? e.message : "تعذر الحذف"); }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2 text-sm">
        <Link href="/settings/requests" className="flex items-center gap-1 text-muted-foreground transition-colors hover:text-foreground">
          <ArrowRight className="h-4 w-4" /> إعدادات الطلبات
        </Link>
        <span className="text-muted-foreground">/</span>
        <span>مسارات الموافقات</span>
      </div>

      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold">مسارات الموافقات</h1>
          <p className="mt-1 text-sm text-muted-foreground">صمّم سلاسل الاعتماد باختيارات جاهزة — دون أي قيمة تقنية.</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => setTemplatePicker(true)}><Layers className="h-4 w-4" /> من قالب جاهز</Button>
          <Button onClick={() => setWizard({ mode: "new" })}><Plus className="h-4 w-4" /> مسار جديد</Button>
        </div>
      </div>

      {loading ? (
        <div className="flex justify-center p-12"><Loader2 className="animate-spin text-muted-foreground" /></div>
      ) : items.length === 0 ? (
        <div className="rounded-xl border border-dashed border-border p-10 text-center text-sm text-muted-foreground">
          لا توجد مسارات بعد. ابدأ من قالب جاهز أو أنشئ مساراً جديداً.
        </div>
      ) : (
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {items.map((w) => (
            <div key={w.id} className="flex flex-col rounded-xl border border-border bg-card p-4">
              <div className="flex items-start gap-2">
                <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary/10 text-primary"><GitBranch className="h-4 w-4" /></div>
                <div className="min-w-0 flex-1">
                  <div className="flex items-center gap-1.5">
                    <span className="truncate text-sm font-medium">{w.name}</span>
                    {w.isActive ? <Badge variant="secondary">نشط</Badge> : <Badge variant="outline">معطّل</Badge>}
                  </div>
                  <span className="text-xs text-muted-foreground">{w.stepCount} خطوة</span>
                </div>
              </div>
              {w.assignedRequestTypes.length > 0 && (
                <div className="mt-2 flex flex-wrap gap-1">
                  {w.assignedRequestTypes.slice(0, 3).map((t, i) => <Badge key={i} variant="outline" className="text-[10px]">{t}</Badge>)}
                  {w.assignedRequestTypes.length > 3 && <Badge variant="outline" className="text-[10px]">+{w.assignedRequestTypes.length - 3}</Badge>}
                </div>
              )}
              <div className="mt-3 flex gap-1.5">
                <Button variant="outline" size="sm" onClick={() => setWizard({ mode: "edit", id: w.id })}><Pencil /> تعديل</Button>
                <Button variant="ghost" size="icon-sm" title="نسخ" onClick={() => duplicate(w.id)}><Copy /></Button>
                <Button variant="ghost" size="icon-sm" className="ms-auto text-destructive" title="حذف" onClick={() => remove(w)}><Trash2 /></Button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Template picker */}
      {templatePicker && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/30 p-4" onClick={() => setTemplatePicker(false)}>
          <div className="max-h-[80vh] w-full max-w-lg overflow-auto rounded-xl border border-border bg-popover p-4 shadow-lg" onClick={(e) => e.stopPropagation()}>
            <div className="mb-3 flex items-center justify-between">
              <h3 className="text-sm font-medium">اختر قالباً جاهزاً</h3>
              <Button variant="ghost" size="icon-sm" onClick={() => setTemplatePicker(false)}><X /></Button>
            </div>
            <div className="grid gap-2">
              {WORKFLOW_TEMPLATES.map((t) => (
                <button key={t.key}
                  onClick={() => { setTemplatePicker(false); setWizard({ mode: "new", name: t.nameAr, steps: t.steps.map((s) => ({ ...s })) }); }}
                  className="rounded-lg border border-border p-3 text-start transition-colors hover:border-primary hover:bg-primary/5">
                  <div className="text-sm font-medium">{t.nameAr}</div>
                  <div className="text-xs text-muted-foreground">{t.descriptionAr}</div>
                </button>
              ))}
            </div>
          </div>
        </div>
      )}

      {/* Wizard overlay */}
      {wizard && (
        <ApprovalWorkflowWizard
          workflowId={wizard.mode === "edit" ? wizard.id : null}
          initialName={wizard.mode === "new" ? wizard.name : undefined}
          initialSteps={wizard.mode === "new" ? wizard.steps : undefined}
          onClose={() => setWizard(null)}
          onSaved={() => { setWizard(null); load(); }}
        />
      )}

      <DesignerCredit />
    </div>
  );
}
