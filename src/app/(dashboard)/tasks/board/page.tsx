"use client";

import { useState } from "react";
import Link from "next/link";
import { ArrowRight, Plus } from "lucide-react";
import { mockTasks } from "@/lib/tasks-mock-data";
import { Task } from "@/types";
import { TaskBoardView } from "@/components/tasks/task-board-view";
import { TaskCreateModal } from "@/components/tasks/task-create-modal";

export default function BoardPage() {
  const [tasks, setTasks] = useState<Task[]>(mockTasks);
  const [showCreate, setShowCreate] = useState(false);

  const handleUpdate = (updated: Task) => {
    setTasks(tasks.map((t) => (t.id === updated.id ? updated : t)));
  };

  const handleCreate = (task: Task) => {
    setTasks([task, ...tasks]);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <div className="flex items-center gap-2 text-sm text-muted-foreground mb-2">
            <Link href="/tasks" className="hover:text-primary transition-colors">المهام</Link>
            <ArrowRight className="h-3 w-3 rotate-180" />
            <span>لوحة كانبان</span>
          </div>
          <h1 className="text-2xl font-bold">لوحة كانبان</h1>
          <p className="text-sm text-muted-foreground mt-1">إدارة المهام بالسحب والإفلات</p>
        </div>
        <button
          onClick={() => setShowCreate(true)}
          className="inline-flex items-center gap-2 h-10 px-4 bg-primary text-primary-foreground font-bold uppercase tracking-wider text-sm hover:bg-primary/80 transition-colors"
        >
          <Plus className="h-4 w-4" />
          مهمة جديدة
        </button>
      </div>

      <TaskBoardView tasks={tasks} onUpdate={handleUpdate} />

      <TaskCreateModal
        open={showCreate}
        onClose={() => setShowCreate(false)}
        onCreate={handleCreate}
      />
    </div>
  );
}
