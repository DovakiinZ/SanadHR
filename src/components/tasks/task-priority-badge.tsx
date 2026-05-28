"use client";

import { Badge } from "@/components/ui/badge";
import { TaskPriority } from "@/types";
import { taskPriorities } from "@/lib/tasks-mock-data";

export function TaskPriorityBadge({ priority }: { priority: TaskPriority }) {
  const config = taskPriorities.find((p) => p.value === priority);
  return (
    <Badge variant="outline" className={`text-xs font-medium ${config?.color || ""}`}>
      <span className="ml-1">{config?.icon}</span>
      {priority}
    </Badge>
  );
}
