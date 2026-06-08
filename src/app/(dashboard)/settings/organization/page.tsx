import Link from "next/link";
import { ArrowRight, Network, Building, BriefcaseBusiness, Layers, Coins, ChevronLeft } from "lucide-react";

const modules = [
  { href: "/settings/organization/departments", title: "الأقسام", description: "هيكل الأقسام والتسلسل الإداري والمدراء", icon: Network },
  { href: "/settings/organization/job-titles", title: "المسميات الوظيفية", description: "المسميات ومسؤولياتها ومهاراتها ومستنداتها", icon: BriefcaseBusiness },
  { href: "/settings/organization/branches", title: "الفروع", description: "فروع المؤسسة ومواقعها", icon: Building },
  { href: "/settings/organization/positions", title: "المناصب", description: "المناصب الوظيفية", icon: BriefcaseBusiness },
  { href: "/settings/organization/grades", title: "الدرجات الوظيفية", description: "درجات ومستويات الموظفين", icon: Layers },
  { href: "/settings/organization/cost-centers", title: "مراكز التكلفة", description: "مراكز التكلفة المحاسبية", icon: Coins },
];

export default function OrganizationSettingsPage() {
  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2 text-sm">
        <Link href="/settings" className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1">
          <ArrowRight className="h-4 w-4" /> الإعدادات
        </Link>
        <span className="text-muted-foreground">/</span>
        <span>إعدادات المؤسسة</span>
      </div>

      <div>
        <h1 className="text-2xl font-bold">إعدادات المؤسسة</h1>
        <p className="text-sm text-muted-foreground mt-1">الهيكل التنظيمي والبيانات المرجعية للمؤسسة</p>
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
