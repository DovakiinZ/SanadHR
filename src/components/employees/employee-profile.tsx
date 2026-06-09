"use client";

import { useEffect, useState } from "react";
import {
  Users, Calendar, Banknote, Building, GitBranch, Mail, Phone, MapPin,
  CreditCard, ShieldAlert, FileText, Clock, History, Loader2,
} from "lucide-react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Badge } from "@/components/ui/badge";
import { ApiEmployee } from "@/lib/api/employees";
import { fileUrl } from "@/lib/api/files";
import { getAuditEntries, AuditEntry } from "@/lib/api/audit";
import { usePermissions } from "@/lib/permissions";

interface Props {
  employee: ApiEmployee;
}

function Row({ label, value }: { label: string; value?: string | number | null }) {
  return (
    <div className="flex items-start justify-between gap-4 py-2.5 border-b border-border/60 last:border-0">
      <span className="text-xs font-bold uppercase tracking-wider text-muted-foreground shrink-0">{label}</span>
      <span className="text-sm text-foreground text-left">{value === null || value === undefined || value === "" ? "—" : value}</span>
    </div>
  );
}

function Empty({ text }: { text: string }) {
  return <div className="py-12 text-center text-sm text-muted-foreground">{text}</div>;
}

function yearsOfService(hireDate: string): string {
  const d = new Date(hireDate);
  if (isNaN(d.getTime())) return "—";
  const years = (Date.now() - d.getTime()) / (365.25 * 24 * 3600 * 1000);
  return years < 1 ? `${Math.max(0, Math.round(years * 12))} شهر` : `${years.toFixed(1)} سنة`;
}

export function EmployeeProfile({ employee: e }: Props) {
  const { hasAny } = usePermissions();
  const canSeeComp = hasAny("Payroll.View", "Payroll.Edit", "Employees.Edit", "Employees.Create");
  const canSeeAudit = hasAny("Platform.Admin.View", "Audit.View");

  const [audit, setAudit] = useState<AuditEntry[]>([]);
  const [auditLoading, setAuditLoading] = useState(false);

  useEffect(() => {
    if (!canSeeAudit) return;
    setAuditLoading(true);
    getAuditEntries({ entityType: "Employee", entityId: e.id, pageSize: 50 })
      .then(setAudit).catch(() => setAudit([])).finally(() => setAuditLoading(false));
  }, [e.id, canSeeAudit]);

  const photo = fileUrl(e.photoUrl);
  const name = e.fullNameAr || e.fullName;
  const money = (n?: number | null) => (n == null ? "—" : `${n.toLocaleString("en-US")} ${e.currency || "SAR"}`);

  return (
    <div className="space-y-5">
      {/* Header */}
      <div className="border border-border bg-card p-5 flex flex-col sm:flex-row sm:items-center gap-5">
        <div className="h-24 w-24 border border-border bg-secondary overflow-hidden flex items-center justify-center shrink-0">
          {photo ? <img src={photo} alt="" className="h-full w-full object-cover" /> : <Users className="h-10 w-10 text-muted-foreground" />}
        </div>
        <div className="flex-1 space-y-2">
          <div className="flex items-center gap-3 flex-wrap">
            <h2 className="text-xl font-bold">{name}</h2>
            {e.status && <Badge variant="outline" className="text-xs bg-green-500/10 text-green-500 border-green-500/20">{e.statusAr}</Badge>}
            <span className="font-mono text-xs text-muted-foreground">{e.employeeNumber}</span>
          </div>
          <div className="flex items-center gap-x-5 gap-y-1 flex-wrap text-sm text-muted-foreground">
            <span className="inline-flex items-center gap-1"><FileText className="h-3.5 w-3.5" />{e.jobTitleAr || e.jobTitle || "—"}</span>
            <span className="inline-flex items-center gap-1"><Building className="h-3.5 w-3.5" />{e.departmentName || "—"}</span>
            <span className="inline-flex items-center gap-1"><GitBranch className="h-3.5 w-3.5" />{e.branchName || "—"}</span>
            <span className="inline-flex items-center gap-1"><Users className="h-3.5 w-3.5" />{e.managerName || "بدون مدير"}</span>
          </div>
        </div>
      </div>

      {/* Quick stats */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="border border-border bg-card p-4"><div className="text-xs text-muted-foreground flex items-center gap-1"><Calendar className="h-3.5 w-3.5" /> مدة الخدمة</div><div className="text-lg font-bold mt-1">{yearsOfService(e.hireDate)}</div></div>
        <div className="border border-border bg-card p-4"><div className="text-xs text-muted-foreground flex items-center gap-1"><Calendar className="h-3.5 w-3.5" /> تاريخ الالتحاق</div><div className="text-lg font-bold mt-1">{e.hireDate?.slice(0, 10) || "—"}</div></div>
        <div className="border border-border bg-card p-4"><div className="text-xs text-muted-foreground flex items-center gap-1"><Banknote className="h-3.5 w-3.5" /> الراتب الأساسي</div><div className="text-lg font-bold mt-1">{canSeeComp ? money(e.basicSalary) : "•••"}</div></div>
        <div className="border border-border bg-card p-4"><div className="text-xs text-muted-foreground flex items-center gap-1"><CreditCard className="h-3.5 w-3.5" /> طريقة الدفع</div><div className="text-lg font-bold mt-1">{e.paymentMethodAr || "—"}</div></div>
      </div>

      {/* Tabs */}
      <Tabs defaultValue="overview" className="w-full">
        <TabsList className="flex flex-wrap h-auto justify-start gap-1 bg-transparent p-0">
          {[
            ["overview", "نظرة عامة"], ["personal", "شخصي"], ["job", "الوظيفة"],
            ...(canSeeComp ? [["comp", "التعويضات"]] : []),
            ["attendance", "الحضور"], ["leaves", "الإجازات"], ["documents", "المستندات"],
            ["requests", "الطلبات"], ["timeline", "السجل الزمني"],
            ...(canSeeAudit ? [["audit", "سجل التدقيق"]] : []),
          ].map(([v, l]) => (
            <TabsTrigger key={v} value={v} className="text-xs data-[state=active]:bg-primary data-[state=active]:text-primary-foreground border border-border">{l}</TabsTrigger>
          ))}
        </TabsList>

        <div className="border border-border bg-card p-5 mt-3">
          <TabsContent value="overview" className="mt-0 grid grid-cols-1 md:grid-cols-2 gap-x-8">
            <div><Row label="الاسم (عربي)" value={e.fullNameAr} /><Row label="الاسم (إنجليزي)" value={e.fullName} /><Row label="البريد" value={e.email} /><Row label="الجوال" value={e.phone} /><Row label="الجنسية" value={e.nationalityAr || e.nationality} /></div>
            <div><Row label="القسم" value={e.departmentName} /><Row label="الفرع" value={e.branchName} /><Row label="المسمى" value={e.jobTitleAr || e.jobTitle} /><Row label="المدير المباشر" value={e.managerName} /><Row label="الحالة" value={e.statusAr} /></div>
          </TabsContent>

          <TabsContent value="personal" className="mt-0 grid grid-cols-1 md:grid-cols-2 gap-x-8">
            <div><Row label="الاسم الأول (عربي)" value={e.firstNameAr} /><Row label="اسم العائلة (عربي)" value={e.lastNameAr} /><Row label="الجنس" value={e.genderAr} /><Row label="تاريخ الميلاد" value={e.dateOfBirth?.slice(0, 10)} /><Row label="رقم الهوية/الإقامة" value={e.nationalId} /></div>
            <div><Row label="الجنسية" value={e.nationalityAr || e.nationality} /><Row label="المدينة" value={e.city} /><Row label="العنوان" value={e.address} /><Row label="جهة اتصال الطوارئ" value={e.emergencyContactName} /><Row label="هاتف الطوارئ" value={e.emergencyContactPhone} /></div>
          </TabsContent>

          <TabsContent value="job" className="mt-0 grid grid-cols-1 md:grid-cols-2 gap-x-8">
            <div><Row label="الرقم الوظيفي" value={e.employeeNumber} /><Row label="المسمى الوظيفي" value={e.jobTitleAr || e.jobTitle} /><Row label="نوع العقد" value={e.contractTypeAr || e.contractType} /><Row label="نوع التوظيف" value={e.employmentTypeAr || e.employmentType} /></div>
            <div><Row label="تاريخ الالتحاق" value={e.hireDate?.slice(0, 10)} /><Row label="القسم" value={e.departmentName} /><Row label="الفرع" value={e.branchName} /><Row label="المدير المباشر" value={e.managerName} /></div>
          </TabsContent>

          {canSeeComp && (
            <TabsContent value="comp" className="mt-0">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-x-8">
                <div><Row label="الراتب الأساسي" value={money(e.basicSalary)} /><Row label="طريقة الدفع" value={e.paymentMethodAr} /><Row label="البنك" value={e.bankAr || e.bank} /><Row label="رقم الآيبان" value={e.iban} /></div>
                <div><Row label="رقم الحساب" value={e.bankAccountNumber} /><Row label="رقم بطاقة الراتب" value={e.salaryCardNumber} /><Row label="مزود البطاقة" value={e.cardProvider} /><Row label="مجموعة الرواتب" value={e.payrollGroupAr || e.payrollGroup} /></div>
              </div>
              <div className="mt-4">
                <div className="text-xs font-bold uppercase tracking-wider text-primary mb-2">البدلات</div>
                {(!e.allowances || e.allowances.length === 0) ? <Empty text="لا توجد بدلات" /> : (
                  <div className="border border-border divide-y divide-border">
                    {e.allowances.map((a) => (
                      <div key={a.id} className="flex items-center justify-between px-3 py-2 text-sm">
                        <span>{a.allowanceTypeAr || a.allowanceType}</span>
                        <span className="font-mono">{a.amount.toLocaleString("en-US")}</span>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </TabsContent>
          )}

          <TabsContent value="attendance" className="mt-0"><Empty text="لا توجد سجلات حضور بعد" /></TabsContent>
          <TabsContent value="leaves" className="mt-0"><Empty text="لا توجد إجازات مسجلة بعد" /></TabsContent>
          <TabsContent value="documents" className="mt-0"><Empty text="لا توجد مستندات مرفوعة بعد" /></TabsContent>
          <TabsContent value="requests" className="mt-0"><Empty text="لا توجد طلبات لهذا الموظف بعد" /></TabsContent>

          <TabsContent value="timeline" className="mt-0">
            {auditLoading ? <div className="py-8 text-center"><Loader2 className="h-4 w-4 animate-spin inline" /></div>
              : !canSeeAudit ? <Empty text="غير متاح" />
              : audit.length === 0 ? <Empty text="لا توجد أحداث بعد" /> : (
                <div className="relative pr-4">
                  {audit.map((a) => (
                    <div key={a.id} className="relative pr-6 pb-5 border-r border-border last:border-0">
                      <span className="absolute right-[-5px] top-1 h-2.5 w-2.5 rounded-full bg-primary" />
                      <div className="text-sm font-medium">{actionLabel(a.action)}</div>
                      <div className="text-xs text-muted-foreground">{a.userEmail || "النظام"} · {new Date(a.timestamp).toLocaleString("ar")}</div>
                    </div>
                  ))}
                </div>
              )}
          </TabsContent>

          {canSeeAudit && (
            <TabsContent value="audit" className="mt-0">
              {auditLoading ? <div className="py-8 text-center"><Loader2 className="h-4 w-4 animate-spin inline" /></div>
                : audit.length === 0 ? <Empty text="لا توجد سجلات تدقيق بعد" /> : (
                  <div className="space-y-2">
                    {audit.map((a) => (
                      <div key={a.id} className="border border-border p-3 text-sm">
                        <div className="flex items-center justify-between">
                          <span className="font-medium inline-flex items-center gap-2"><History className="h-3.5 w-3.5 text-primary" />{actionLabel(a.action)}</span>
                          <span className="text-xs text-muted-foreground">{new Date(a.timestamp).toLocaleString("ar")}</span>
                        </div>
                        <div className="text-xs text-muted-foreground mt-1">بواسطة: {a.userEmail || "النظام"}{a.ipAddress ? ` · ${a.ipAddress}` : ""}</div>
                        {(a.oldValues || a.newValues) && (
                          <div className="grid grid-cols-1 md:grid-cols-2 gap-2 mt-2">
                            {a.oldValues && <pre className="text-[10px] bg-secondary border border-border p-2 overflow-x-auto" dir="ltr">{prettyJson(a.oldValues)}</pre>}
                            {a.newValues && <pre className="text-[10px] bg-secondary border border-border p-2 overflow-x-auto" dir="ltr">{prettyJson(a.newValues)}</pre>}
                          </div>
                        )}
                      </div>
                    ))}
                  </div>
                )}
            </TabsContent>
          )}
        </div>
      </Tabs>
    </div>
  );
}

function actionLabel(action: string): string {
  const m: Record<string, string> = { Created: "تم الإنشاء", Updated: "تم التحديث", Deleted: "تم الحذف", Reparented: "تم تغيير القسم الأب" };
  return m[action] ?? action;
}
function prettyJson(s: string): string {
  try { return JSON.stringify(JSON.parse(s), null, 1); } catch { return s; }
}
