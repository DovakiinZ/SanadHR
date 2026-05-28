"use client";

import { Badge } from "@/components/ui/badge";
import { TaskStatus } from "@/types";
import { taskStatuses } from "@/lib/tasks-mock-data";

export function TaskStatusBadge({ status }: { status: TaskStatus }) {
  const config = taskStatuses.find((s) => s.value === status);
  return (
    <Badge variant="outline" className={`text-xs font-medium ${config?.color || ""}`}>
      {config?.label || status}
    </Badge>
  );
}
