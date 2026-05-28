"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  LayoutDashboard,
  Users,
  Clock,
  Banknote,
  Receipt,
  HandCoins,
  FileText,
  BarChart3,
  FolderOpen,
  Settings,
  LogOut,
} from "lucide-react";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";

const navItems = [
  { label: "لوحة التحكم", href: "/dashboard", icon: LayoutDashboard },
  { label: "الموظفين", href: "/employees", icon: Users },
  { label: "الحضور", href: "/attendance", icon: Clock },
  { label: "الرواتب", href: "/payroll", icon: Banknote },
  { label: "المصروفات", href: "/expenses", icon: Receipt },
  { label: "السلف", href: "/loans", icon: HandCoins },
  { label: "الطلبات", href: "/requests", icon: FileText },
  { label: "التقارير", href: "/reports", icon: BarChart3 },
  { label: "المستندات", href: "/documents", icon: FolderOpen },
  { label: "الإعدادات", href: "/settings", icon: Settings },
];

export function Sidebar() {
  const pathname = usePathname();

  const handleLogout = () => {
    localStorage.removeItem("hr_auth");
    window.location.href = "/login";
  };

  return (
    <aside className="fixed top-0 right-0 z-40 h-screen w-16 border-l border-border bg-secondary flex flex-col items-center py-4">
      {/* Logo */}
      <div className="mb-6 flex h-10 w-10 items-center justify-center bg-primary">
        <span className="text-lg font-bold text-primary-foreground">HR</span>
      </div>

      {/* Nav */}
      <nav className="flex flex-1 flex-col items-center gap-1">
        {navItems.map((item) => {
          const isActive =
            pathname === item.href || pathname?.startsWith(item.href + "/");
          const Icon = item.icon;
          return (
            <Tooltip key={item.href}>
              <TooltipTrigger
                render={<Link href={item.href} />}
                className={`flex h-10 w-10 items-center justify-center transition-colors ${
                  isActive
                    ? "bg-primary text-primary-foreground"
                    : "text-muted-foreground hover:bg-card hover:text-foreground"
                }`}
              >
                <Icon className="h-5 w-5" />
              </TooltipTrigger>
              <TooltipContent side="left" className="font-sans">
                {item.label}
              </TooltipContent>
            </Tooltip>
          );
        })}
      </nav>

      {/* Logout */}
      <Tooltip>
        <TooltipTrigger
          onClick={handleLogout}
          className="flex h-10 w-10 items-center justify-center text-muted-foreground hover:bg-card hover:text-foreground transition-colors"
        >
          <LogOut className="h-5 w-5" />
        </TooltipTrigger>
        <TooltipContent side="left" className="font-sans">
          تسجيل الخروج
        </TooltipContent>
      </Tooltip>
    </aside>
  );
}
