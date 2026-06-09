import Link from "next/link";
import { ArrowRight, Inbox, Tags, ChevronLeft } from "lucide-react";

const modules = [
  {
    href: "/settings/requests/types",
    title: "أنواع الطلبات",
    description: "مُنشئ الطلبات بدون كود — النموذج، مسار الموافقة، اتفاقية الخدمة، المستند، والأثر",
    icon: Inbox,
  },
  {
    href: "/settings/requests/categories",
    title: "فئات الطلبات",
    description: "تصنيف الطلبات (إجازات، مالية، خطابات، تقنية...)",
    icon: Tags,
  },
];

export default function RequestsSettingsPage() {
  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2 text-sm">
        <Link href="/settings" className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1">
          <ArrowRight className="h-4 w-4" /> الإعدادات
        </Link>
        <span className="text-muted-foreground">/</span>
        <span>إعدادات الطلبات</span>
      </div>

      <div>
        <h1 className="text-2xl font-bold">إعدادات الطلبات</h1>
        <p className="text-sm text-muted-foreground mt-1">عرّف أنواع الطلبات التي يقدّمها الموظفون وكيفية معالجتها</p>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {modules.map((m) => {
          const Icon = m.icon;
          return (
            <Link key={m.href} href={m.href} className="group border border-border bg-card p-5 hover:border-primary/50 hover:bg-card/70 transition-colors">
              <div className="flex items-start justify-between">
                <div className="flex h-10 w-10 items-center justify-center bg-primary/10 text-primary"><Icon className="h-5 w-5" /></div>
                <ChevronLeft className="h-4 w-4 text-muted-foreground group-hover:text-primary transition-colors" />
              </div>
              <h2 className="text-base font-bold mt-4">{m.title}</h2>
              <p className="text-xs text-muted-foreground mt-1 leading-relaxed">{m.description}</p>
            </Link>
          );
        })}
      </div>
    </div>
  );
}
