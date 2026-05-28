"use client";

import { useState, useMemo } from "react";
import Link from "next/link";
import { ArrowRight } from "lucide-react";
import { mockTasks } from "@/lib/tasks-mock-data";
import { Task } from "@/types";
import { TaskCard } from "@/components/tasks/task-card";
import { TaskDetailsDrawer } from "@/components/tasks/task-details-drawer";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { TaskStatusBadge } from "@/components/tasks/task-status-badge";

export default function TeamTasksPage() {
  const [tasks, setTasks] = useState<Task[]>(mockTasks);
  const [selectedTask, setSelectedTask] = useState<Task | null>(null);

  const handleUpdate = (updated: Task) => {
    setTasks(tasks.map((t) => (t.id === updated.id ? updated : t)));
    setSelectedTask(updated);
  };

  // Group by assignee
  const byAssignee = useMemo(() => {
    const groups: Record<string, Task[]> = {};
    tasks
      .filter((t) => !["مكتملة", "ملغاة"].includes(t.status))
      .forEach((t) => {
        if (!groups[t.assignee]) groups[t.assignee] = [];
        groups[t.assignee].push(t);
      });
    return Object.entries(groups).sort((a, b) => b[1].length - a[1].length);
  }, [tasks]);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <div className="flex items-center gap-2 text-sm text-muted-foreground mb-2">
          <Link href="/tasks" className="hover:text-primary transition-colors">المهام</Link>
          <ArrowRight className="h-3 w-3 rotate-180" />
          <span>مهام الفريق</span>
        </div>
        <h1 className="text-2xl font-bold">مهام الفريق</h1>
        <p className="text-sm text-muted-foreground mt-1">عرض المهام حسب أعضاء الفريق</p>
      </div>

      {/* Team view */}
      <div className="space-y-6">
        {byAssignee.map(([assignee, assigneeTasks]) => (
          <div key={assignee} className="space-y-2">
            <div className="flex items-center gap-3 py-2 border-b border-border">
              <Avatar className="h-8 w-8">
                <AvatarFallback className="bg-card text-xs font-bold border border-border">
                  {assignee.charAt(0)}
                </AvatarFallback>
              </Avatar>
              <div>
                <h3 className="text-sm font-medium">{assignee}</h3>
                <p className="text-xs text-muted-foreground">{assigneeTasks.length} مهام نشطة</p>
              </div>
              <div className="flex items-center gap-1 mr-auto">
                {["قيد التنفيذ", "جديدة", "متأخرة"].map((status) => {
                  const count = assigneeTasks.filter((t) => t.status === status).length;
                  if (count === 0) return null;
                  return (
                    <TaskStatusBadge key={status} status={status as Task["status"]} />
                  );
                })}
              </div>
            </div>
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-2">
              {assigneeTasks.map((task) => (
                <TaskCard
                  key={task.id}
                  task={task}
                  onClick={() => setSelectedTask(task)}
                />
              ))}
            </div>
          </div>
        ))}
      </div>

      <TaskDetailsDrawer
        task={selectedTask}
        open={!!selectedTask}
        onClose={() => setSelectedTask(null)}
        onUpdate={handleUpdate}
      />
    </div>
  );
}
