"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import {
  ReactFlow, Background, Controls, MiniMap, Handle, Position,
  type Node, type Edge, type NodeProps,
} from "@xyflow/react";
import "@xyflow/react/dist/style.css";
import { ArrowRight, Search, Loader2, Pencil, ChevronDown, ChevronUp, Users, Building2 } from "lucide-react";
import { toast } from "sonner";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { ApiError } from "@/lib/api-client";
import {
  Department, DepartmentInput, listDepartments, updateDepartment, reparentDepartment,
  getBranches, getDepartments, OrgOption, orgLabel,
} from "@/lib/api/org";
import { getEmployees } from "@/lib/api/employees";
import { getLookup, lookupLabel, LookupItem } from "@/lib/api/lookups";
import { Employee } from "@/types";

function notifyError(err: unknown, fallback: string) {
  if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
    toast.error(err instanceof ApiError ? err.message : fallback);
  }
}

const NODE_W = 210, NODE_H = 74, H_GAP = 36, V_GAP = 96;

type DeptData = {
  label: string; code?: string | null; manager?: string | null;
  childCount: number; collapsed: boolean; highlight: boolean;
  onToggle: (id: string) => void; onEdit: (id: string) => void; id: string;
};

function DeptNode({ data }: NodeProps) {
  const d = data as DeptData;
  return (
    <div
      className={`bg-card border ${d.highlight ? "border-primary ring-2 ring-primary/40" : "border-border"} px-3 py-2 shadow-sm`}
      style={{ width: NODE_W, height: NODE_H }}
    >
      <Handle type="target" position={Position.Top} className="!bg-primary !w-2 !h-2" />
      <div className="flex items-start justify-between gap-1 h-full">
        <div className="min-w-0 flex-1">
          <div className="font-bold text-sm truncate">{d.label}</div>
          {d.code && <div className="font-mono text-[10px] text-muted-foreground">{d.code}</div>}
          <div className="text-[11px] text-muted-foreground truncate flex items-center gap-1"><Users className="h-3 w-3" />{d.manager || "بدون مدير"}</div>
        </div>
        <div className="flex flex-col items-center gap-1">
          <button onClick={(e) => { e.stopPropagation(); d.onEdit(d.id); }} className="text-muted-foreground hover:text-foreground" title="تعديل"><Pencil className="h-3.5 w-3.5" /></button>
          {d.childCount > 0 && (
            <button onClick={(e) => { e.stopPropagation(); d.onToggle(d.id); }} className="text-muted-foreground hover:text-foreground" title={d.collapsed ? "توسيع" : "طي"}>
              {d.collapsed ? <ChevronDown className="h-3.5 w-3.5" /> : <ChevronUp className="h-3.5 w-3.5" />}
            </button>
          )}
        </div>
      </div>
      {d.childCount > 0 && <Handle type="source" position={Position.Bottom} className="!bg-primary !w-2 !h-2" />}
      {d.collapsed && d.childCount > 0 && (
        <span className="absolute -bottom-2 left-1/2 -translate-x-1/2 text-[9px] bg-primary text-primary-foreground px-1">{d.childCount}</span>
      )}
    </div>
  );
}

const nodeTypes = { dept: DeptNode };

export default function DepartmentChartPage() {
  const [depts, setDepts] = useState<Department[]>([]);
  const [loading, setLoading] = useState(true);
  const [collapsed, setCollapsed] = useState<Set<string>>(new Set());
  const [search, setSearch] = useState("");

  // edit dialog
  const [editing, setEditing] = useState<Department | null>(null);
  const [form, setForm] = useState<DepartmentInput | null>(null);
  const [saving, setSaving] = useState(false);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [branches, setBranches] = useState<OrgOption[]>([]);
  const [costCenters, setCostCenters] = useState<LookupItem[]>([]);
  const [allDepts, setAllDepts] = useState<OrgOption[]>([]);

  const load = useCallback(async () => {
    setLoading(true);
    try { setDepts(await listDepartments()); }
    catch (err) { notifyError(err, "تعذر تحميل الأقسام"); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => { load(); }, [load]);
  useEffect(() => {
    (async () => {
      try {
        const [emps, brs, cc, deps] = await Promise.all([
          getEmployees({ pageSize: 200 }), getBranches(), getLookup("cost-centers"), getDepartments(),
        ]);
        setEmployees(emps); setBranches(brs); setCostCenters(cc); setAllDepts(deps);
      } catch { /* selects degrade gracefully */ }
    })();
  }, []);

  const toggle = useCallback((id: string) => {
    setCollapsed((c) => { const n = new Set(c); n.has(id) ? n.delete(id) : n.add(id); return n; });
  }, []);
  const openEdit = useCallback((id: string) => {
    setDepts((cur) => {
      const d = cur.find((x) => x.id === id);
      if (d) {
        setEditing(d);
        setForm({
          name: d.name, nameAr: d.nameAr ?? "", code: d.code ?? "", description: d.description ?? "",
          parentDepartmentId: d.parentDepartmentId ?? null, managerId: d.managerId ?? null,
          deputyManagerId: d.deputyManagerId ?? null, branchId: d.branchId ?? null,
          costCenterId: d.costCenterId ?? null, isActive: d.isActive,
        });
      }
      return cur;
    });
  }, []);

  // Build hierarchy → tidy tree layout.
  const { nodes, edges } = useMemo(() => {
    const byId = new Map(depts.map((d) => [d.id, d]));
    const childrenMap = new Map<string, Department[]>();
    const roots: Department[] = [];
    for (const d of depts) {
      const pid = d.parentDepartmentId && byId.has(d.parentDepartmentId) ? d.parentDepartmentId : null;
      if (pid) { const arr = childrenMap.get(pid) ?? []; arr.push(d); childrenMap.set(pid, arr); }
      else roots.push(d);
    }
    const term = search.trim().toLowerCase();
    const pos = new Map<string, { x: number; y: number }>();
    let cursor = 0;
    const place = (id: string, depth: number): number => {
      const kids = (childrenMap.get(id) ?? []);
      const isCollapsed = collapsed.has(id);
      if (kids.length === 0 || isCollapsed) {
        const x = cursor; cursor += NODE_W + H_GAP;
        pos.set(id, { x, y: depth * (NODE_H + V_GAP) });
        return x;
      }
      const xs = kids.map((k) => place(k.id, depth + 1));
      const center = (xs[0] + xs[xs.length - 1]) / 2;
      pos.set(id, { x: center, y: depth * (NODE_H + V_GAP) });
      return center;
    };
    roots.forEach((r) => place(r.id, 0));

    // Only nodes that got a position are visible (descendants of collapsed are hidden).
    const ns: Node[] = [];
    const es: Edge[] = [];
    for (const d of depts) {
      const p = pos.get(d.id);
      if (!p) continue;
      const childCount = (childrenMap.get(d.id) ?? []).length;
      const highlight = !!term && ((d.nameAr ?? "").toLowerCase().includes(term) || d.name.toLowerCase().includes(term) || (d.code ?? "").toLowerCase().includes(term));
      ns.push({
        id: d.id, type: "dept", position: p, draggable: true,
        data: { id: d.id, label: d.nameAr || d.name, code: d.code, manager: d.managerName, childCount, collapsed: collapsed.has(d.id), highlight, onToggle: toggle, onEdit: openEdit },
      });
      const pid = d.parentDepartmentId && byId.has(d.parentDepartmentId) ? d.parentDepartmentId : null;
      if (pid && pos.has(pid)) es.push({ id: `${pid}-${d.id}`, source: pid, target: d.id, type: "smoothstep" });
    }
    return { nodes: ns, edges: es };
  }, [depts, collapsed, search, toggle, openEdit]);

  // Drag a node onto another → reparent (backend guards cycles).
  const onNodeDragStop = useCallback((_e: unknown, node: Node) => {
    const cx = node.position.x + NODE_W / 2;
    const cy = node.position.y + NODE_H / 2;
    let target: Node | null = null;
    for (const n of nodes) {
      if (n.id === node.id) continue;
      const within = cx >= n.position.x && cx <= n.position.x + NODE_W && cy >= n.position.y && cy <= n.position.y + NODE_H;
      if (within) { target = n; break; }
    }
    const current = depts.find((d) => d.id === node.id)?.parentDepartmentId ?? null;
    const newParent = target ? target.id : null;
    if (newParent === current) { setDepts((d) => [...d]); return; } // snap back via relayout
    (async () => {
      try {
        await reparentDepartment(node.id, newParent);
        toast.success(newParent ? "تم نقل القسم" : "تم جعل القسم جذراً");
        await load();
      } catch (err) { notifyError(err, "تعذر نقل القسم"); await load(); }
    })();
  }, [nodes, depts, load]);

  async function saveEdit() {
    if (!editing || !form) return;
    if (!form.name.trim() && !form.nameAr?.trim()) { toast.error("اسم القسم مطلوب"); return; }
    setSaving(true);
    try {
      await updateDepartment(editing.id, { ...form, name: form.name || form.nameAr || "" });
      toast.success("تم تحديث القسم");
      setEditing(null); setForm(null);
      await load();
    } catch (err) { notifyError(err, "تعذر حفظ القسم"); } finally { setSaving(false); }
  }

  const selectClass = "w-full h-9 bg-secondary border border-border px-3 text-sm text-foreground";

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-2 text-sm">
        <Link href="/settings/organization/departments" className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1">
          <ArrowRight className="h-4 w-4" /> الأقسام
        </Link>
        <span className="text-muted-foreground">/</span>
        <span>المخطط التنظيمي</span>
      </div>

      <div className="flex items-center justify-between gap-3 flex-wrap">
        <div>
          <h1 className="text-2xl font-bold flex items-center gap-2"><Building2 className="h-6 w-6" /> المخطط التنظيمي</h1>
          <p className="text-sm text-muted-foreground mt-1">اسحب قسماً فوق آخر لتغيير التبعية · انقر القلم للتعديل · استخدم الأزرار للطي والتكبير</p>
        </div>
        <div className="relative w-64">
          <Search className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input placeholder="بحث عن قسم..." value={search} onChange={(e) => setSearch(e.target.value)} className="pr-10 bg-secondary border-border h-9 text-sm" />
        </div>
      </div>

      <div className="border border-border bg-card" style={{ height: "calc(100vh - 220px)", minHeight: 480 }}>
        {loading ? (
          <div className="h-full flex items-center justify-center text-muted-foreground"><Loader2 className="h-5 w-5 animate-spin" /></div>
        ) : depts.length === 0 ? (
          <div className="h-full flex items-center justify-center text-sm text-muted-foreground">لا توجد أقسام — أضفها من صفحة الأقسام</div>
        ) : (
          <ReactFlow
            nodes={nodes}
            edges={edges}
            nodeTypes={nodeTypes}
            onNodeDragStop={onNodeDragStop}
            fitView
            minZoom={0.2}
            proOptions={{ hideAttribution: true }}
          >
            <Background gap={20} color="#27272a" />
            <Controls showInteractive={false} />
            <MiniMap pannable zoomable nodeColor="#3f3f46" maskColor="rgba(0,0,0,0.6)" />
          </ReactFlow>
        )}
      </div>

      <Dialog open={!!editing} onOpenChange={(o) => { if (!o && !saving) { setEditing(null); setForm(null); } }}>
        <DialogContent className="sm:max-w-2xl">
          <DialogHeader><DialogTitle>تعديل قسم</DialogTitle></DialogHeader>
          {form && (
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 py-2 max-h-[65vh] overflow-y-auto pl-1">
              <div className="space-y-2"><Label className="text-xs font-bold uppercase tracking-wider">الاسم (عربي)</Label><Input value={form.nameAr ?? ""} onChange={(e) => setForm({ ...form, nameAr: e.target.value })} className="bg-secondary border-border" /></div>
              <div className="space-y-2"><Label className="text-xs font-bold uppercase tracking-wider">الاسم (إنجليزي)</Label><Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} className="bg-secondary border-border" /></div>
              <div className="space-y-2"><Label className="text-xs font-bold uppercase tracking-wider">الرمز</Label><Input value={form.code ?? ""} onChange={(e) => setForm({ ...form, code: e.target.value })} className="bg-secondary border-border font-mono" /></div>
              <div className="space-y-2">
                <Label className="text-xs font-bold uppercase tracking-wider">القسم الأب</Label>
                <select value={form.parentDepartmentId ?? ""} onChange={(e) => setForm({ ...form, parentDepartmentId: e.target.value || null })} className={selectClass}>
                  <option value="">— جذر —</option>
                  {allDepts.filter((d) => d.id !== editing?.id).map((d) => <option key={d.id} value={d.id}>{orgLabel(d)}</option>)}
                </select>
              </div>
              <div className="space-y-2">
                <Label className="text-xs font-bold uppercase tracking-wider">المدير</Label>
                <select value={form.managerId ?? ""} onChange={(e) => setForm({ ...form, managerId: e.target.value || null })} className={selectClass}>
                  <option value="">— لا يوجد —</option>
                  {employees.map((m) => <option key={m.id} value={m.id}>{m.name}</option>)}
                </select>
              </div>
              <div className="space-y-2">
                <Label className="text-xs font-bold uppercase tracking-wider">نائب المدير</Label>
                <select value={form.deputyManagerId ?? ""} onChange={(e) => setForm({ ...form, deputyManagerId: e.target.value || null })} className={selectClass}>
                  <option value="">— لا يوجد —</option>
                  {employees.map((m) => <option key={m.id} value={m.id}>{m.name}</option>)}
                </select>
              </div>
              <div className="space-y-2">
                <Label className="text-xs font-bold uppercase tracking-wider">الفرع</Label>
                <select value={form.branchId ?? ""} onChange={(e) => setForm({ ...form, branchId: e.target.value || null })} className={selectClass}>
                  <option value="">— لا يوجد —</option>
                  {branches.map((b) => <option key={b.id} value={b.id}>{orgLabel(b)}</option>)}
                </select>
              </div>
              <div className="space-y-2">
                <Label className="text-xs font-bold uppercase tracking-wider">مركز التكلفة</Label>
                <select value={form.costCenterId ?? ""} onChange={(e) => setForm({ ...form, costCenterId: e.target.value || null })} className={selectClass}>
                  <option value="">— لا يوجد —</option>
                  {costCenters.map((c) => <option key={c.id} value={c.id}>{lookupLabel(c)}</option>)}
                </select>
              </div>
              <label className="flex items-center gap-2 text-sm cursor-pointer pt-2"><input type="checkbox" checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} /> نشط</label>
            </div>
          )}
          <DialogFooter>
            <Button variant="outline" onClick={() => { setEditing(null); setForm(null); }} disabled={saving}>إلغاء</Button>
            <Button onClick={saveEdit} disabled={saving} className="font-bold">{saving ? "جاري الحفظ..." : "حفظ"}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
