"use client";

import { Bell, Search } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";

export function Topbar() {
  return (
    <header className="sticky top-0 z-30 h-14 border-b border-border bg-background flex items-center justify-between px-6">
      {/* Search */}
      <div className="relative w-72">
        <Search className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          placeholder="بحث..."
          className="pr-10 bg-secondary border-border h-9 text-sm placeholder:text-muted-foreground"
        />
      </div>

      {/* Actions */}
      <div className="flex items-center gap-4">
        <button className="relative flex h-9 w-9 items-center justify-center text-muted-foreground hover:text-foreground transition-colors">
          <Bell className="h-5 w-5" />
          <span className="absolute top-1 left-1 h-2 w-2 bg-primary" />
        </button>

        <div className="flex items-center gap-3">
          <div className="text-left">
            <p className="text-sm font-medium leading-none">عبدالله المدير</p>
            <p className="text-xs text-muted-foreground">مدير النظام</p>
          </div>
          <Avatar className="h-8 w-8">
            <AvatarFallback className="bg-primary text-primary-foreground text-xs font-bold">
              عم
            </AvatarFallback>
          </Avatar>
        </div>
      </div>
    </header>
  );
}
