import Link from "next/link";
import { Database, ChevronLeft } from "lucide-react";

const settingsAreas = [
  {
    href: "/settings/master-data",
    title: "البيانات الرئيسية",
    description: "إدارة القوائم المرجعية (المسميات الوظيفية، الجنسيات، أنواع العقود، الإجازات، الطلبات…) المستخدمة في كل النظام",
    icon: Database,
  },
];

export default function SettingsPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">الإعدادات</h1>
        <p className="text-sm text-muted-foreground mt-1">إعدادات النظام والتكوين</p>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {settingsAreas.map((area) => {
          const Icon = area.icon;
          return (
            <Link
              key={area.href}
              href={area.href}
              className="group border border-border bg-card p-5 hover:border-primary/50 hover:bg-card/70 transition-colors"
            >
              <div className="flex items-start justify-between">
                <div className="flex h-10 w-10 items-center justify-center bg-primary/10 text-primary">
                  <Icon className="h-5 w-5" />
                </div>
                <ChevronLeft className="h-4 w-4 text-muted-foreground group-hover:text-primary transition-colors" />
              </div>
              <h2 className="text-base font-bold mt-4">{area.title}</h2>
              <p className="text-xs text-muted-foreground mt-1 leading-relaxed">{area.description}</p>
            </Link>
          );
        })}
      </div>
    </div>
  );
}
