"use client";

import { useState } from "react";
import Link from "next/link";
import { Plus, List, LayoutGrid, Calendar, BarChart3 } from "lucide-react";
import { mockTasks } from "@/lib/tasks-mock-data";
import { Task } from "@/types";
import { TaskListView } from "@/components/tasks/task-list-view";
import { TaskBoardView } from "@/components/tasks/task-board-view";
import { TaskCalendarView } from "@/components/tasks/task-calendar-view";
import { TaskCreateModal } from "@/components/tasks/task-create-modal";
import {
  TaskDashboardWidgets,
  TasksByPriorityWidget,
  TasksByDepartmentWidget,
  TasksSourceWidget,
} from "@/components/tasks/task-dashboard-widgets";
import { Button } from "@/components/ui/button";

type ViewMode = "list" | "board" | "calendar" | "stats";

const viewOptions: { value: ViewMode; label: string; icon: React.ElementType }[] = [
  { value: "list", label: "قائمة", icon: List },
  { value: "board", label: "لوحة", icon: LayoutGrid },
  { value: "calendar", label: "تقويم", icon: Calendar },
  { value: "stats", label: "إحصائيات", icon: BarChart3 },
];

export default function TasksPage() {
  const [tasks, setTasks] = useState<Task[]>(mockTasks);
  const [view, setView] = useState<ViewMode>("list");
  const [showCreate, setShowCreate] = useState(false);

  const handleCreate = (task: Task) => {
    setTasks([task, ...tasks]);
  };

  const handleUpdate = (updated: Task) => {
    setTasks(tasks.map((t) => (t.id === updated.id ? updated : t)));
  };

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">المهام</h1>
          <p className="text-sm text-muted-foreground mt-1">إدارة ومتابعة مهام العمليات</p>
        </div>
        <div className="flex items-center gap-2">
          {/* Sub-navigation */}
          <div className="flex items-center gap-1 ml-4">
            <Link href="/tasks/my-tasks">
              <Button variant="ghost" size="sm" className="h-8 text-xs text-muted-foreground">مهامي</Button>
            </Link>
            <Link href="/tasks/team">
              <Button variant="ghost" size="sm" className="h-8 text-xs text-muted-foreground">الفريق</Button>
            </Link>
            <Link href="/tasks/templates">
              <Button variant="ghost" size="sm" className="h-8 text-xs text-muted-foreground">القوالب</Button>
            </Link>
            <Link href="/tasks/settings">
              <Button variant="ghost" size="sm" className="h-8 text-xs text-muted-foreground">الإعدادات</Button>
            </Link>
          </div>

          {/* View toggles */}
          <div className="flex items-center border border-border">
            {viewOptions.map((opt) => {
              const Icon = opt.icon;
              return (
                <button
                  key={opt.value}
                  onClick={() => setView(opt.value)}
                  className={`h-9 w-9 flex items-center justify-center transition-colors ${
                    view === opt.value
                      ? "bg-primary text-primary-foreground"
                      : "text-muted-foreground hover:text-foreground hover:bg-secondary"
                  }`}
                  title={opt.label}
                >
                  <Icon className="h-4 w-4" />
                </button>
              );
            })}
          </div>

          {/* Create button */}
          <button
            onClick={() => setShowCreate(true)}
            className="inline-flex items-center gap-2 h-10 px-4 bg-primary text-primary-foreground font-bold uppercase tracking-wider text-sm hover:bg-primary/80 transition-colors"
          >
            <Plus className="h-4 w-4" />
            مهمة جديدة
          </button>
        </div>
      </div>

      {/* Dashboard Widgets */}
      <TaskDashboardWidgets tasks={tasks} />

      {/* View content */}
      {view === "list" && <TaskListView tasks={tasks} onUpdate={handleUpdate} />}
      {view === "board" && <TaskBoardView tasks={tasks} onUpdate={handleUpdate} />}
      {view === "calendar" && <TaskCalendarView tasks={tasks} onUpdate={handleUpdate} />}
      {view === "stats" && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
          <TasksByPriorityWidget tasks={tasks} />
          <TasksByDepartmentWidget tasks={tasks} />
          <TasksSourceWidget tasks={tasks} />
        </div>
      )}

      {/* Create Modal */}
      <TaskCreateModal
        open={showCreate}
        onClose={() => setShowCreate(false)}
        onCreate={handleCreate}
      />
    </div>
  );
}
