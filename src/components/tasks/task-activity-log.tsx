"use client";

import { TaskActivityLog as ActivityLog } from "@/types";
import { Activity } from "lucide-react";

export function TaskActivityLog({ logs }: { logs: ActivityLog[] }) {
  const formatTime = (dateStr: string) => {
    const d = new Date(dateStr);
    return `${d.toLocaleDateString("ar-SA")} ${d.toLocaleTimeString("ar-SA", { hour: "2-digit", minute: "2-digit" })}`;
  };

  return (
    <div className="space-y-1">
      {logs.length === 0 && (
        <p className="text-sm text-muted-foreground text-center py-4">لا يوجد سجل</p>
      )}
      {logs.map((log, i) => (
        <div key={log.id} className="flex gap-3 py-2 relative">
          {/* Timeline line */}
          {i < logs.length - 1 && (
            <div className="absolute right-[13px] top-8 bottom-0 w-px bg-border" />
          )}
          <div className="flex-shrink-0 h-[26px] w-[26px] bg-secondary border border-border flex items-center justify-center z-10">
            <Activity className="h-3 w-3 text-muted-foreground" />
          </div>
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2">
              <span className="text-sm font-medium">{log.action}</span>
              <span className="text-xs text-muted-foreground">بواسطة {log.user}</span>
            </div>
            {log.details && (
              <p className="text-xs text-muted-foreground mt-0.5">{log.details}</p>
            )}
            <p className="text-xs text-muted-foreground/60 mt-0.5">{formatTime(log.timestamp)}</p>
          </div>
        </div>
      ))}
    </div>
  );
}
