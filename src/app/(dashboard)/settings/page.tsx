import Link from "next/link";
import { Building2, Database, ChevronLeft, Inbox, Wallet, CalendarDays } from "lucide-react";

const categories = [
  {
    href: "/settings/leave",
    title: "إعدادات الإجازات",
    description: "أنواع الإجازات وقواعدها (مدفوعة/رصيد/مرفقات) وأرصدة الموظفين",
    icon: CalendarDays,
  },
  {
    href: "/settings/organization",
    title: "إعدادات المؤسسة",
    description: "الأقسام، الفروع، المسميات الوظيفية، الجنسيات، الدرجات، مراكز التكلفة",
    icon: Building2,
  },
  {
    href: "/settings/payroll",
    title: "إعدادات الرواتب",
    description: "أنواع البدلات وقواعد احتسابها، مجموعات الرواتب، طرق الدفع",
    icon: Wallet,
  },
  {
    href: "/settings/requests",
    title: "إعدادات الطلبات",
    description: "أنواع الطلبات، الفئات، النماذج، مسارات الموافقة، اتفاقيات مستوى الخدمة",
    icon: Inbox,
  },
];

export default function SettingsPage() {
  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-bold">الإعدادات</h1>
        <p className="text-sm text-muted-foreground mt-1">تكوين وحدات النظام — كل وحدة بإعداداتها وقواعدها</p>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {categories.map((c) => {
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

      {/* Advanced raw catalog editor (covers catalog types without a dedicated page yet) */}
      <div className="border-t border-border pt-5">
        <Link href="/settings/master-data" className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
          <Database className="h-4 w-4" />
          محرّر البيانات الرئيسية المتقدم (كل القوائم)
        </Link>
      </div>
    </div>
  );
}
