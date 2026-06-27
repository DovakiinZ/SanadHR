"use client";

import { type ElementType, type ReactNode, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import {
  Banknote, Briefcase, Building, CalendarDays, ClipboardList, Download, FileText, GitBranch,
  Loader2, Pencil, Printer, RotateCcw, Scale, Send, TrendingUp, Users, Wallet,
} from "lucide-react";
import { toast } from "sonner";
import { requestRestore } from "@/lib/api/restores";
import { Cell, Pie, PieChart, ResponsiveContainer, Tooltip } from "recharts";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Badge } from "@/components/ui/badge";
import { fileUrl } from "@/lib/api/files";
import { ApiEmployee } from "@/lib/api/employees";
import { usePermissions } from "@/lib/permissions";
import {
  downloadRequestDocument, getEmployeeLeaveBalances, LeaveTypeInfo, RequestInstance,
  requestStatusColor, requestStatusLabel,
} from "@/lib/api/request-center";
import { getEmployeeRequests, getEmployeeTimeline, TimelineEvent } from "@/lib/api/employee-profile";
import { EmployeeDocuments } from "./employee-documents";

interface Props { employee: ApiEmployee }

const DONUT_COLORS = ["#FBBF24", "#60A5FA", "#34D399", "#A78BFA", "#FB923C", "#F472B6", "#22D3EE", "#A3E635"];

function years(hireDate?: string): string {
  if (!hireDate) return "—";
  const d = new Date(hireDate); if (isNaN(d.getTime())) return "—";
  const months = (Date.now() - d.getTime()) / (1000 * 60 * 60 * 24 * 30.44);
  const y = Math.floor(months / 12), m = Math.floor(months % 12);
  return y > 0 ? `${y} سنة${m ? ` و${m} شهر` : ""}` : `${m} شهر`;
}

export function EmployeeProfile({ employee: e }: Props) {
  const { hasAny } = usePermissions();
  const canSeeComp = hasAny("Payroll.View", "Payroll.Edit", "Employees.Edit", "Employees.Create");
  const canEdit = hasAny("Employees.Edit");
  const canTerminate = hasAny("Employees.Terminate");
  const isFormer = e.statusAr === "منتهي" || e.statusAr === "مستقيل";

  const [restoring, setRestoring] = useState(false);
  async function doRestore() {
    if (restoring) return;
    const reason = prompt("سبب استرجاع الموظف (اختياري)") ?? undefined;
    setRestoring(true);
    try {
      await requestRestore(e.id, reason);
      toast.success("تم تقديم طلب الاسترجاع للاعتماد (المدير → الموارد البشرية)");
    } catch (err) {
      toast.error((err as Error)?.message || "تعذر تقديم طلب الاسترجاع");
    } finally {
      setRestoring(false);
    }
  }

  const [balances, setBalances] = useState<LeaveTypeInfo[]>([]);
  const [requests, setRequests] = useState<RequestInstance[]>([]);
  const [timeline, setTimeline] = useState<TimelineEvent[]>([]);

  useEffect(() => {
    getEmployeeLeaveBalances(e.id).then(setBalances).catch(() => setBalances([]));
    getEmployeeRequests(e.id).then(setRequests).catch(() => setRequests([]));
    getEmployeeTimeline(e.id).then(setTimeline).catch(() => setTimeline([]));
  }, [e.id]);

  const photo = fileUrl(e.photoUrl);
  const name = e.fullNameAr || e.fullName;
  const cur = e.currency || "SAR";
  const money = (n: number) => `${Math.round(n).toLocaleString("en-US")} ${cur}`;

  // Salary breakdown is computed on the backend (single source of truth): basic + allowances
  // + additions − deductions − GOSI. The UI just presents it.
  const comp = useMemo(() => {
    const allowances = (e.allowances ?? []).filter((a) => a.isActive).map((a) => ({ name: a.allowanceTypeAr || a.allowanceType || "بدل", amount: a.amount }));
    const additions = (e.additions ?? []).filter((a) => a.isActive).map((a) => ({ name: a.typeAr || a.type || "إضافة", amount: a.amount }));
    const deductions = (e.deductions ?? []).filter((a) => a.isActive).map((a) => ({ name: a.typeAr || a.type || "استقطاع", amount: a.amount }));
    return {
      allowances, additions, deductions,
      totalAllowances: e.totalAllowances ?? 0, totalAdditions: e.totalAdditions ?? 0, totalDeductions: e.totalDeductions ?? 0,
      gosi: e.gosiAmount ?? 0, gosiRate: e.gosiRate ?? 0, gross: e.grossSalary ?? e.basicSalary, net: e.netSalary ?? e.basicSalary,
    };
  }, [e]);

  const leaveTotal = balances.reduce((s, b) => s + b.remainingDays, 0);
  const activeRequests = requests.filter((r) => r.status === "Submitted" || r.status === "InProgress").length;

  // Donut: Basic + Housing + Transport + Other allowances + Additions + Deductions + GOSI.
  const donutData = useMemo(() => {
    let housing = 0, transport = 0, other = 0;
    for (const a of comp.allowances) {
      const n = a.name.toLowerCase();
      if (n.includes("سكن") || n.includes("housing")) housing += a.amount;
      else if (n.includes("نقل") || n.includes("مواصلات") || n.includes("transport")) transport += a.amount;
      else other += a.amount;
    }
    return [
      { name: "أساسي", value: e.basicSalary },
      { name: "بدل سكن", value: housing },
      { name: "بدل نقل", value: transport },
      { name: "بدلات أخرى", value: other },
      { name: "إضافات", value: comp.totalAdditions },
      { name: "استقطاعات", value: comp.totalDeductions },
      { name: "GOSI", value: comp.gosi },
    ].filter((d) => d.value > 0);
  }, [comp, e.basicSalary]);

  return (
    <div className="space-y-5">
      {/* Hero */}
      <div className="border border-border bg-card p-6">
        <div className="flex flex-col gap-5 sm:flex-row sm:items-center">
          <div className="flex h-28 w-28 shrink-0 items-center justify-center overflow-hidden border border-border bg-secondary">
            {photo ? <img src={photo} alt="" className="h-full w-full object-cover" /> : <Users className="h-12 w-12 text-muted-foreground" />}
          </div>
          <div className="flex-1 space-y-2">
            <div className="flex flex-wrap items-center gap-3">
              <h1 className="text-2xl font-bold">{name}</h1>
              <Badge variant="outline" className={`text-xs ${isFormer ? "border-destructive/20 bg-destructive/10 text-destructive" : "border-green-500/20 bg-green-500/10 text-green-500"}`}>{e.statusAr}</Badge>
              <span className="font-mono text-xs text-muted-foreground">{e.employeeNumber}</span>
            </div>
            <div className="flex flex-wrap items-center gap-x-5 gap-y-1 text-sm text-muted-foreground">
              <Meta icon={Briefcase} v={e.jobTitleAr || e.jobTitle} />
              <Meta icon={Building} v={e.departmentName} />
              <Meta icon={GitBranch} v={e.branchName} />
              <Meta icon={Users} v={e.managerName || "بدون مدير"} />
            </div>
          </div>
          <div className="flex flex-wrap items-center gap-2">
            {canEdit && <Link href={`/employees/${e.id}/edit`} className="inline-flex h-9 items-center gap-2 border border-border px-3 text-sm hover:bg-muted"><Pencil className="h-4 w-4" /> تعديل</Link>}
            {canTerminate && !isFormer && <Link href={`/employees/${e.id}/settlement`} className="inline-flex h-9 items-center gap-2 border border-border px-3 text-sm hover:bg-muted"><Scale className="h-4 w-4" /> نهاية الخدمة</Link>}
            {canTerminate && isFormer && (
              <button onClick={doRestore} disabled={restoring} className="inline-flex h-9 items-center gap-2 border border-primary/40 bg-primary/5 px-3 text-sm text-primary hover:bg-primary/10 disabled:opacity-50">
                {restoring ? <Loader2 className="h-4 w-4 animate-spin" /> : <RotateCcw className="h-4 w-4" />} استرجاع الموظف
              </button>
            )}
            <button onClick={() => window.print()} className="inline-flex h-9 items-center gap-2 border border-border px-3 text-sm hover:bg-muted"><Printer className="h-4 w-4" /> طباعة</button>
            <Link href="/requests" className="inline-flex h-9 items-center gap-2 bg-primary px-3 text-sm font-bold text-primary-foreground hover:bg-primary/80"><Send className="h-4 w-4" /> طلب</Link>
          </div>
        </div>
      </div>

      {/* KPI cards */}
      <div className="grid grid-cols-2 gap-3 lg:grid-cols-6">
        <Kpi icon={CalendarDays} label="مدة الخدمة" value={years(e.hireDate)} />
        <Kpi icon={TrendingUp} label="نسبة الحضور" value="—" />
        <Kpi icon={CalendarDays} label="رصيد الإجازات" value={`${leaveTotal} يوم`} />
        <Kpi icon={ClipboardList} label="طلبات نشطة" value={`${activeRequests}`} />
        <Kpi icon={Banknote} label="الراتب الأساسي" value={canSeeComp ? money(e.basicSalary) : "•••"} />
        <Kpi icon={Wallet} label="صافي الراتب" value={canSeeComp ? money(comp.net) : "•••"} />
      </div>

      {/* Tabs */}
      <Tabs defaultValue="overview" className="w-full">
        <TabsList className="flex h-auto flex-wrap justify-start gap-1 bg-transparent p-0">
          {[["overview", "نظرة عامة"], ["personal", "البيانات الشخصية"], ["employment", "التوظيف"], ["salary", "الراتب"], ["attendance", "الحضور"], ["leave", "الإجازات"], ["requests", "الطلبات"], ["documents", "المستندات"], ["timeline", "السجل الزمني"]].map(([v, l]) => (
            <TabsTrigger key={v} value={v} className="border border-border text-xs data-[state=active]:bg-primary data-[state=active]:text-primary-foreground">{l}</TabsTrigger>
          ))}
        </TabsList>

        <TabsContent value="overview" className="mt-4 grid grid-cols-1 gap-4 lg:grid-cols-2">
          <Panel title="ملخص التوظيف">
            <Row k="الحالة" v={e.statusAr} /><Row k="القسم" v={e.departmentName} /><Row k="المسمى الوظيفي" v={e.jobTitleAr || e.jobTitle} />
            <Row k="الفرع" v={e.branchName} /><Row k="المدير المباشر" v={e.managerName} /><Row k="تاريخ الالتحاق" v={e.hireDate?.slice(0, 10)} />
            <Row k="مدة الخدمة" v={years(e.hireDate)} />
          </Panel>
          <Panel title="معلومات سريعة">
            <Row k="البريد الإلكتروني" v={e.email} /><Row k="الهاتف" v={e.phone} /><Row k="الجنسية" v={e.nationalityAr || e.nationality} />
            <Row k="رصيد الإجازات" v={`${leaveTotal} يوم`} /><Row k="طلبات نشطة" v={`${activeRequests}`} />
            {canSeeComp && <Row k="صافي الراتب" v={money(comp.net)} />}
          </Panel>
        </TabsContent>

        <TabsContent value="personal" className="mt-4">
          <Panel title="البيانات الشخصية">
            <Row k="الاسم" v={name} /><Row k="تاريخ الميلاد" v={e.dateOfBirth?.slice(0, 10)} /><Row k="الجنس" v={e.genderAr} />
            <Row k="الجنسية" v={e.nationalityAr || e.nationality} /><Row k="الهوية / الإقامة" v={e.nationalId} />
            <Row k="الهاتف" v={e.phone} /><Row k="البريد الإلكتروني" v={e.email} /><Row k="العنوان" v={e.address} /><Row k="المدينة" v={e.city} />
            <Row k="جهة الطوارئ" v={e.emergencyContactName} /><Row k="هاتف الطوارئ" v={e.emergencyContactPhone} />
          </Panel>
        </TabsContent>

        <TabsContent value="employment" className="mt-4">
          <Panel title="بيانات التوظيف">
            <Row k="الرقم الوظيفي" v={e.employeeNumber} /><Row k="الحالة" v={e.statusAr} /><Row k="تاريخ الالتحاق" v={e.hireDate?.slice(0, 10)} />
            <Row k="نوع العقد" v={e.contractTypeAr || e.contractType} /><Row k="نوع التوظيف" v={e.employmentTypeAr || e.employmentType} />
            <Row k="المسمى الوظيفي" v={e.jobTitleAr || e.jobTitle} /><Row k="القسم" v={e.departmentName} /><Row k="الفرع" v={e.branchName} />
            <Row k="المدير المباشر" v={e.managerName} /><Row k="موقع العمل" v={e.workLocationAr || e.workLocation} />
            <Row k="سياسة الإجازات" v={e.leavePolicyAr || e.leavePolicy} /><Row k="مجموعة الرواتب" v={e.payrollGroupAr || e.payrollGroup} />
          </Panel>
        </TabsContent>

        <TabsContent value="salary" className="mt-4 space-y-4">
          {!canSeeComp ? <Empty text="لا تملك صلاحية عرض بيانات الراتب" /> : (
            <>
              <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
                <div className="border border-border bg-card p-5 lg:col-span-2">
                  <h3 className="mb-4 text-sm font-bold uppercase tracking-wider text-muted-foreground">احتساب صافي الراتب</h3>
                  <Waterfall basic={e.basicSalary} allowances={comp.totalAllowances} additions={comp.totalAdditions} deductions={comp.totalDeductions} gosi={comp.gosi} net={comp.net} money={money} max={comp.gross} />
                </div>
                <div className="border border-border bg-card p-5">
                  <h3 className="mb-2 text-sm font-bold uppercase tracking-wider text-muted-foreground">تكوين الراتب</h3>
                  <div className="h-48">
                    <ResponsiveContainer width="100%" height="100%">
                      <PieChart>
                        <Pie data={donutData} dataKey="value" nameKey="name" innerRadius="55%" outerRadius="85%" paddingAngle={1}>
                          {donutData.map((_, i) => <Cell key={i} fill={DONUT_COLORS[i % DONUT_COLORS.length]} />)}
                        </Pie>
                        <Tooltip contentStyle={{ background: "#212125", border: "1px solid #3F3F46", fontSize: 12 }} formatter={(value) => money(Number(value))} />
                      </PieChart>
                    </ResponsiveContainer>
                  </div>
                </div>
              </div>
              <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
                <Panel title="البدلات">
                  {comp.allowances.length === 0 ? <p className="text-sm text-muted-foreground">لا توجد بدلات</p> : comp.allowances.map((a, i) => <Row key={i} k={a.name} v={money(a.amount)} />)}
                  <div className="mt-2 border-t border-border pt-2"><Row k="إجمالي البدلات" v={money(comp.totalAllowances)} bold /></div>
                </Panel>
                <Panel title="الإضافات">
                  {comp.additions.length === 0 ? <p className="text-sm text-muted-foreground">لا توجد إضافات</p> : comp.additions.map((a, i) => <Row key={i} k={a.name} v={money(a.amount)} />)}
                  <div className="mt-2 border-t border-border pt-2"><Row k="إجمالي الإضافات" v={money(comp.totalAdditions)} bold /></div>
                </Panel>
                <Panel title="الاستقطاعات">
                  {comp.deductions.map((d, i) => <Row key={i} k={d.name} v={money(d.amount)} />)}
                  <Row k={`التأمينات (GOSI ${comp.gosiRate}%)`} v={money(comp.gosi)} />
                  <div className="mt-2 border-t border-border pt-2"><Row k="إجمالي الاستقطاعات" v={money(comp.totalDeductions + comp.gosi)} bold /></div>
                </Panel>
              </div>
              <Panel title="طريقة الدفع">
                <Row k="الطريقة" v={e.paymentMethodAr || e.paymentMethod} />
                {e.paymentMethodCode === "BANK_TRANSFER" && (<><Row k="البنك" v={e.bankAr || e.bank} /><Row k="الآيبان" v={e.iban} /><Row k="رقم الحساب" v={e.bankAccountNumber} /></>)}
                {e.paymentMethodCode === "SALARY_CARD" && (<><Row k="رقم البطاقة" v={e.salaryCardNumber} /><Row k="مزوّد البطاقة" v={e.cardProvider} /></>)}
              </Panel>
            </>
          )}
        </TabsContent>

        <TabsContent value="attendance" className="mt-4"><Empty text="لا توجد سجلات حضور بعد" /></TabsContent>

        <TabsContent value="leave" className="mt-4">
          <Panel title="أرصدة الإجازات">
            {balances.length === 0 ? <p className="text-sm text-muted-foreground">لا توجد بيانات أرصدة</p> : (
              <table className="w-full text-sm">
                <thead><tr className="border-b border-border text-right text-xs text-muted-foreground"><th className="py-2">النوع</th><th>الرصيد</th><th>المستخدم</th><th>المتبقي</th></tr></thead>
                <tbody>{balances.map((b) => (<tr key={b.id} className="border-b border-border/40"><td className="py-1.5">{b.nameAr}</td><td>{b.entitledDays}</td><td>{b.usedDays}</td><td className="font-medium">{b.remainingDays}</td></tr>))}</tbody>
              </table>
            )}
          </Panel>
        </TabsContent>

        <TabsContent value="requests" className="mt-4 space-y-2">
          {requests.length === 0 ? <Empty text="لا توجد طلبات" /> : requests.map((r) => (
            <div key={r.id} className="flex items-center justify-between border border-border bg-card px-4 py-3">
              <div><span className="font-medium">{r.requestTypeNameAr}</span> <span className="font-mono text-xs text-muted-foreground">{r.requestNumber}</span><div className="text-xs text-muted-foreground">{new Date(r.submittedAt).toLocaleDateString("ar")}</div></div>
              <span className={`border px-2 py-1 text-xs ${requestStatusColor(r.status)}`}>{requestStatusLabel(r.status)}</span>
            </div>
          ))}
        </TabsContent>

        <TabsContent value="documents" className="mt-4 space-y-6">
          <EmployeeDocuments employeeId={e.id} canWrite={canEdit} />

          <div className="space-y-2">
            <h3 className="text-sm font-bold">المستندات الرسمية الصادرة</h3>
            {requests.filter((r) => r.generatedDocumentId).length === 0 ? <Empty text="لا توجد مستندات صادرة" /> :
              requests.filter((r) => r.generatedDocumentId).map((r) => (
                <button key={r.id} onClick={() => downloadRequestDocument(r.id, `${r.requestNumber}.pdf`).catch(() => {})} className="flex w-full items-center justify-between border border-border bg-card px-4 py-3 text-right hover:bg-muted/40">
                  <span className="inline-flex items-center gap-2"><FileText className="h-4 w-4 text-primary" /> {r.requestTypeNameAr} — {r.requestNumber}</span>
                  <Download className="h-4 w-4 text-muted-foreground" />
                </button>
              ))}
          </div>
        </TabsContent>

        <TabsContent value="timeline" className="mt-4">
          <Panel title="السجل الزمني">
            {timeline.length === 0 ? <p className="text-sm text-muted-foreground">لا توجد أحداث</p> : (
              <div className="space-y-3">
                {timeline.map((t) => (
                  <div key={t.id} className="flex items-start gap-3">
                    <span className="mt-1.5 h-2 w-2 shrink-0 rounded-full bg-primary" />
                    <div className="text-sm"><span>{t.descriptionAr || t.action}</span>{t.actorName && <span className="text-xs text-muted-foreground"> · {t.actorName}</span>}<div className="text-xs text-muted-foreground">{new Date(t.occurredAt).toLocaleString("ar")}</div></div>
                  </div>
                ))}
              </div>
            )}
          </Panel>
        </TabsContent>
      </Tabs>
    </div>
  );
}

function Meta({ icon: Icon, v }: { icon: ElementType; v?: string | null }) {
  return <span className="inline-flex items-center gap-1"><Icon className="h-3.5 w-3.5" /> {v || "—"}</span>;
}
function Kpi({ icon: Icon, label, value }: { icon: ElementType; label: string; value: string }) {
  return (
    <div className="border border-border bg-card p-4">
      <div className="flex items-center gap-1.5 text-xs text-muted-foreground"><Icon className="h-3.5 w-3.5" /> {label}</div>
      <div className="mt-1 text-lg font-bold tabular-nums">{value}</div>
    </div>
  );
}
function Panel({ title, children }: { title: string; children: ReactNode }) {
  return <div className="border border-border bg-card p-5"><h3 className="mb-3 text-sm font-bold uppercase tracking-wider text-muted-foreground">{title}</h3><div className="space-y-1.5">{children}</div></div>;
}
function Row({ k, v, bold }: { k: string; v?: string | number | null; bold?: boolean }) {
  return <div className="flex items-center justify-between gap-3 text-sm"><span className="text-muted-foreground">{k}</span><span className={bold ? "font-bold" : "font-medium"}>{v ?? "—"}</span></div>;
}
function Empty({ text }: { text: string }) {
  return <div className="border border-dashed border-border py-12 text-center text-sm text-muted-foreground">{text}</div>;
}
function Waterfall({ basic, allowances, additions, deductions, gosi, net, money, max }: { basic: number; allowances: number; additions: number; deductions: number; gosi: number; net: number; money: (n: number) => string; max: number }) {
  const w = (n: number) => `${Math.max(2, (n / (max || 1)) * 100)}%`;
  const rows = [
    { label: "الراتب الأساسي", val: basic, color: "#60A5FA", sign: "" },
    { label: "البدلات", val: allowances, color: "#34D399", sign: "+" },
    { label: "الإضافات", val: additions, color: "#A3E635", sign: "+" },
    { label: "الاستقطاعات", val: deductions, color: "#F87171", sign: "−" },
    { label: "التأمينات (GOSI)", val: gosi, color: "#FB923C", sign: "−" },
    { label: "صافي الراتب", val: net, color: "#FBBF24", sign: "=" },
  ].filter((r) => r.val > 0 || r.sign === "=" || r.sign === "");
  return (
    <div className="space-y-3">
      {rows.map((r, i) => (
        <div key={i} className="space-y-1">
          <div className="flex items-center justify-between text-sm"><span className="text-muted-foreground">{r.sign} {r.label}</span><span className="font-medium tabular-nums">{money(r.val)}</span></div>
          <div className="h-2 w-full overflow-hidden bg-secondary"><div className="h-full" style={{ width: w(r.val), background: r.color }} /></div>
        </div>
      ))}
    </div>
  );
}
