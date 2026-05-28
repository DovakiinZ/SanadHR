"use client";

import { useState } from "react";
import Link from "next/link";
import { ArrowRight } from "lucide-react";
import { mockTasks } from "@/lib/tasks-mock-data";
import { Task } from "@/types";
import { TaskCalendarView } from "@/components/tasks/task-calendar-view";

export default function CalendarPage() {
  const [tasks, setTasks] = useState<Task[]>(mockTasks);

  const handleUpdate = (updated: Task) => {
    setTasks(tasks.map((t) => (t.id === updated.id ? updated : t)));
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <div className="flex items-center gap-2 text-sm text-muted-foreground mb-2">
          <Link href="/tasks" className="hover:text-primary transition-colors">المهام</Link>
          <ArrowRight className="h-3 w-3 rotate-180" />
          <span>التقويم</span>
        </div>
        <h1 className="text-2xl font-bold">تقويم المهام</h1>
        <p className="text-sm text-muted-foreground mt-1">عرض المهام حسب تواريخ الاستحقاق</p>
      </div>

      <TaskCalendarView tasks={tasks} onUpdate={handleUpdate} />
    </div>
  );
}
