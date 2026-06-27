"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { ArrowRight, Loader2, RefreshCw } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { ApiError } from "@/lib/api-client";
import { AccessGuard } from "@/components/access/access-guard";
import { getAccessAudit, type AccessAuditDto } from "@/lib/api/access";

function notifyError(err: unknown, fallback: string) {
  if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
    toast.error(err instanceof ApiError ? err.message : fallback);
  }
}

const ACTION_AR: Record<string, string> = {
  UserCreated: "إنشاء مستخدم", UserUpdated: "تعديل مستخدم", UserDisabled: "تعطيل مستخدم", UserEnabled: "تفعيل مستخدم",
  UserForceLogout: "إنهاء الجلسات", PasswordResetSent: "إرسال إعادة تعيين", EmailChanged: "تغيير البريد",
  RolesChanged: "تغيير الأدوار", EmployeeLinked: "ربط موظف", EmployeeUnlinked: "فك ربط موظف",
  UserCreatedFromEmployee: "إنشاء حساب لموظف", RoleCreated: "إنشاء دور", RoleUpdated: "تعديل دور",
  RolePermissionsChanged: "تغيير صلاحيات دور", RoleDeleted: "حذف دور", TemplateCreated: "إنشاء قالب",
  TemplateUpdated: "تعديل قالب", TemplatePermissionsChanged: "تغيير صلاحيات قالب", TemplateDuplicated: "نسخ قالب",
  TemplateDeleted: "حذف قالب", TemplateAssigned: "إسناد قالب", TemplateRevoked: "سحب قالب",
  OverridesChanged: "تغيير الاستثناءات", TemplatesSeeded: "إنشاء قوالب جاهزة",
};

export default function AuditPage() {
  return <AccessGuard anyOf={["Settings.ViewAudit"]}><Inner /></AccessGuard>;
}

function Inner() {
  const [rows, setRows] = useState<AccessAuditDto[]>([]);
  const [loading, setLoading] = useState(true);

  async function load() {
    setLoading(true);
    try { setRows(await getAccessAudit(200)); } catch (err) { notifyError(err, "تعذر تحميل السجل"); } finally { setLoading(false); }
  }
  useEffect(() => {
    (async () => {
      setLoading(true);
      try { setRows(await getAccessAudit(200)); } catch (err) { notifyError(err, "تعذر تحميل السجل"); } finally { setLoading(false); }
    })();
  }, []);

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2 text-sm">
        <Link href="/settings/access" className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1">
          <ArrowRight className="h-4 w-4" /> المستخدمون والصلاحيات
        </Link>
        <span className="text-muted-foreground">/</span><span>سجل التدقيق</span>
      </div>

      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">سجل تدقيق الوصول</h1>
          <p className="text-sm text-muted-foreground mt-1">كل إجراءات إدارة المستخدمين والأدوار والصلاحيات</p>
        </div>
        <Button onClick={load} variant="outline" className="h-9 gap-2 text-sm"><RefreshCw className="h-4 w-4" /> تحديث</Button>
      </div>

      <div className="border border-border">
        <Table>
          <TableHeader>
            <TableRow className="border-border hover:bg-transparent">
              {["الإجراء", "النوع", "المنفّذ", "الوقت"].map((h, i) => (
                <TableHead key={i} className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">{h}</TableHead>
              ))}
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              <TableRow className="hover:bg-transparent"><TableCell colSpan={4} className="py-12 text-center text-sm text-muted-foreground"><Loader2 className="h-4 w-4 animate-spin inline" /> جاري التحميل…</TableCell></TableRow>
            ) : rows.length === 0 ? (
              <TableRow className="hover:bg-transparent"><TableCell colSpan={4} className="py-12 text-center text-sm text-muted-foreground">لا توجد سجلات</TableCell></TableRow>
            ) : rows.map((r) => (
              <TableRow key={r.id} className="border-border hover:bg-card/50">
                <TableCell className="font-medium">{ACTION_AR[r.action] ?? r.action}</TableCell>
                <TableCell className="text-sm text-muted-foreground">{r.entityType.replace("Access.", "")}</TableCell>
                <TableCell className="text-sm text-muted-foreground" dir="ltr">{r.userEmail ?? "—"}</TableCell>
                <TableCell className="text-sm text-muted-foreground" dir="ltr">{new Date(r.timestamp).toLocaleString("ar-SA")}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}
