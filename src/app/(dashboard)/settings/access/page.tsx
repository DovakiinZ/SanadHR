"use client";

import Link from "next/link";
import { ArrowRight, ChevronLeft, Users, UserCog, Shield, LayoutGrid, ScrollText } from "lucide-react";
import { AccessGuard } from "@/components/access/access-guard";

const cards = [
  { href: "/settings/access/users", title: "المستخدمون والوصول", description: "كل حسابات النظام: الحالة، الموظف المرتبط، الأدوار، إعادة التعيين، التعطيل", icon: Users },
  { href: "/settings/access/employees", title: "حسابات الموظفين", description: "إنشاء حساب دخول لموظف، ربط/فك ربط حساب موجود، حالة وصول الموظف", icon: UserCog },
  { href: "/settings/access/roles", title: "الأدوار", description: "إنشاء وتعديل الأدوار وتحديد صلاحياتها عبر مصفوفة الصلاحيات", icon: Shield },
  { href: "/settings/access/templates", title: "قوالب الصلاحيات", description: "قوالب جاهزة (مدير الموارد البشرية، المالية…) ونسخها وتعديلها وإسنادها", icon: LayoutGrid },
  { href: "/settings/access/audit", title: "سجل التدقيق", description: "كل إجراءات إدارة الوصول: إنشاء/تعطيل المستخدمين، تغيير الأدوار والصلاحيات", icon: ScrollText },
];

export default function AccessSettingsPage() {
  return (
    <AccessGuard anyOf={["Settings.ManageUsers", "Settings.ManageRoles", "Settings.ManageTemplates", "Settings.ViewAudit", "Identity.ViewUsers"]}>
      <div className="space-y-8">
        <div className="flex items-center gap-2 text-sm">
          <Link href="/settings" className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1">
            <ArrowRight className="h-4 w-4" /> الإعدادات
          </Link>
          <span className="text-muted-foreground">/</span>
          <span>المستخدمون والصلاحيات</span>
        </div>

        <div>
          <h1 className="text-2xl font-bold">المستخدمون والصلاحيات</h1>
          <p className="text-sm text-muted-foreground mt-1">إدارة حسابات الدخول والأدوار والصلاحيات وقوالبها وسجل التدقيق من مكان واحد</p>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {cards.map((c) => {
            const Icon = c.icon;
            return (
              <Link key={c.href} href={c.href} className="group border border-border bg-card p-5 hover:border-primary/50 hover:bg-card/70 transition-colors">
                <div className="flex items-start justify-between">
                  <div className="flex h-10 w-10 items-center justify-center bg-primary/10 text-primary"><Icon className="h-5 w-5" /></div>
                  <ChevronLeft className="h-4 w-4 text-muted-foreground group-hover:text-primary transition-colors" />
                </div>
                <h2 className="text-base font-bold mt-4">{c.title}</h2>
                <p className="text-xs text-muted-foreground mt-1 leading-relaxed">{c.description}</p>
              </Link>
            );
          })}
        </div>
      </div>
    </AccessGuard>
  );
}
