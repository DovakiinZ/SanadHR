"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { AlertTriangle, ArrowRight, CheckCircle2, Loader2 } from "lucide-react";
import { toast } from "sonner";
import { ApiError } from "@/lib/api-client";
import { Badge } from "@/components/ui/badge";
import { Combobox, type ComboboxOption } from "@/components/ui/combobox";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import {
  getRequestTypesAdmin, listApprovalWorkflows, setRequestTypeActive,
  setRequestTypePrintTemplate, setRequestTypeWorkflow, type RequestTypeAdmin,
} from "@/lib/api/approval-workflows";
import { getDocumentTemplates, documentLabel } from "@/lib/api/documents";
import { getLookup, lookupLabel } from "@/lib/api/lookups";
import { DesignerCredit } from "@/components/workflows/designer-credit";

export default function RequestActivationPage() {
  const [rows, setRows] = useState<RequestTypeAdmin[]>([]);
  const [workflows, setWorkflows] = useState<ComboboxOption[]>([]);
  const [templates, setTemplates] = useState<ComboboxOption[]>([]);
  const [categories, setCategories] = useState<Map<string, string>>(new Map());
  const [loading, setLoading] = useState(true);
  const [busyId, setBusyId] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    const [types, wfs, tpls, cats] = await Promise.allSettled([
      getRequestTypesAdmin(), listApprovalWorkflows(), getDocumentTemplates(), getLookup("request-categories"),
    ]);
    if (types.status === "fulfilled") setRows(types.value ?? []);
    if (wfs.status === "fulfilled") setWorkflows((wfs.value ?? []).filter((w) => w.isActive).map((w) => ({ value: w.id, label: w.name })));
    if (tpls.status === "fulfilled") setTemplates((tpls.value ?? []).map((t) => ({ value: t.id, label: documentLabel(t) })));
    if (cats.status === "fulfilled") setCategories(new Map((cats.value ?? []).map((c) => [c.id, lookupLabel(c)])));
    setLoading(false);
  }, []);

  useEffect(() => { load(); }, [load]);

  const patch = (id: string, p: Partial<RequestTypeAdmin>) => setRows((rs) => rs.map((r) => (r.id === id ? { ...r, ...p } : r)));

  async function assignWorkflow(r: RequestTypeAdmin, workflowId: string | null) {
    setBusyId(r.id);
    try {
      await setRequestTypeWorkflow(r.id, workflowId);
      const name = workflows.find((w) => w.value === workflowId)?.label ?? null;
      patch(r.id, { workflowDefinitionId: workflowId, workflowName: name, activationReady: r.formDefinitionId !== "00000000-0000-0000-0000-000000000000" && !!workflowId });
      toast.success("تم تحديث المسار");
    } catch (e) { toast.error(e instanceof ApiError ? e.message : "تعذر التحديث"); }
    finally { setBusyId(null); }
  }
  async function assignTemplate(r: RequestTypeAdmin, templateId: string | null) {
    setBusyId(r.id);
    try {
      await setRequestTypePrintTemplate(r.id, templateId);
      patch(r.id, { printTemplateId: templateId, printTemplateName: templates.find((t) => t.value === templateId)?.label ?? null });
      toast.success("تم تحديث القالب");
    } catch (e) { toast.error(e instanceof ApiError ? e.message : "تعذر التحديث"); }
    finally { setBusyId(null); }
  }
  async function toggleActive(r: RequestTypeAdmin) {
    setBusyId(r.id);
    try {
      await setRequestTypeActive(r.id, !r.isActive);
      patch(r.id, { isActive: !r.isActive });
      toast.success(!r.isActive ? "تم التفعيل" : "تم التعطيل");
    } catch (e) { toast.error(e instanceof ApiError ? e.message : "تعذر تغيير الحالة"); }
    finally { setBusyId(null); }
  }

  const sorted = useMemo(() => [...rows].sort((a, b) => a.nameAr.localeCompare(b.nameAr, "ar")), [rows]);

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2 text-sm">
        <Link href="/settings/requests" className="flex items-center gap-1 text-muted-foreground transition-colors hover:text-foreground">
          <ArrowRight className="h-4 w-4" /> إعدادات الطلبات
        </Link>
        <span className="text-muted-foreground">/</span>
        <span>ربط المسارات والتفعيل</span>
      </div>

      <div>
        <h1 className="text-2xl font-bold">أنواع الطلبات — الربط والتفعيل</h1>
        <p className="mt-1 text-sm text-muted-foreground">اربط كل نوع طلب بمسار موافقة وقالب مستند، ثم فعّله. لا يمكن التفعيل قبل تعيين مسار موافقة.</p>
      </div>

      {loading ? (
        <div className="flex justify-center p-12"><Loader2 className="animate-spin text-muted-foreground" /></div>
      ) : (
        <div className="overflow-x-auto rounded-xl border border-border">
          <Table>
            <TableHeader>
              <TableRow className="hover:bg-transparent">
                <TableHead className="text-right text-xs text-muted-foreground">نوع الطلب</TableHead>
                <TableHead className="text-right text-xs text-muted-foreground">الفئة</TableHead>
                <TableHead className="text-right text-xs text-muted-foreground w-56">مسار الموافقة</TableHead>
                <TableHead className="text-right text-xs text-muted-foreground w-56">قالب المستند الافتراضي</TableHead>
                <TableHead className="text-right text-xs text-muted-foreground">الحالة</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {sorted.map((r) => (
                <TableRow key={r.id} className="border-border align-middle">
                  <TableCell>
                    <div className="font-medium">{r.nameAr}</div>
                    <div className="font-mono text-[10px] text-muted-foreground">{r.code}</div>
                  </TableCell>
                  <TableCell className="text-sm text-muted-foreground">{r.categoryId ? categories.get(r.categoryId) ?? "—" : "—"}</TableCell>
                  <TableCell>
                    <Combobox value={r.workflowDefinitionId} onChange={(v) => assignWorkflow(r, v)} options={workflows} placeholder="بدون مسار" />
                  </TableCell>
                  <TableCell>
                    <Combobox value={r.printTemplateId} onChange={(v) => assignTemplate(r, v)} options={templates} placeholder="بدون قالب" />
                  </TableCell>
                  <TableCell>
                    <div className="flex items-center gap-2">
                      <button disabled={busyId === r.id} onClick={() => toggleActive(r)}
                        className={`inline-flex h-6 items-center rounded-full px-2 text-[11px] font-medium transition-colors ${r.isActive ? "bg-emerald-500/15 text-emerald-600" : "bg-zinc-500/15 text-zinc-400"}`}>
                        {busyId === r.id ? <Loader2 className="h-3 w-3 animate-spin" /> : r.isActive ? "نشط" : "معطّل"}
                      </button>
                      {!r.workflowDefinitionId ? (
                        <span title="يلزم تعيين مسار موافقة للتفعيل"><AlertTriangle className="h-4 w-4 text-amber-500" /></span>
                      ) : r.activationReady ? (
                        <span title="جاهز"><CheckCircle2 className="h-4 w-4 text-emerald-500" /></span>
                      ) : null}
                    </div>
                  </TableCell>
                </TableRow>
              ))}
              {sorted.length === 0 && (
                <TableRow className="hover:bg-transparent"><TableCell colSpan={5} className="py-12 text-center text-sm text-muted-foreground">لا توجد أنواع طلبات</TableCell></TableRow>
              )}
            </TableBody>
          </Table>
        </div>
      )}

      <DesignerCredit />
    </div>
  );
}
