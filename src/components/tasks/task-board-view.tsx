"use client";

import { useState, useMemo } from "react";
import { Task, TaskStatus } from "@/types";
import { TaskCard } from "./task-card";
import { TaskDetailsDrawer } from "./task-details-drawer";
import { getStatusForBoard, taskStatuses } from "@/lib/tasks-mock-data";

interface BoardColumn {
  id: string;
  title: string;
  color: string;
  statuses: TaskStatus[];
}

const boardColumns: BoardColumn[] = [
  {
    id: "new",
    title: "جديدة",
    color: "border-t-blue-500",
    statuses: ["جديدة"],
  },
  {
    id: "in-progress",
    title: "قيد التنفيذ",
    color: "border-t-amber-500",
    statuses: ["قيد التنفيذ"],
  },
  {
    id: "waiting",
    title: "بانتظار",
    color: "border-t-purple-500",
    statuses: ["بانتظار الموافقة", "بانتظار الموظف", "بانتظار الموارد البشرية", "بانتظار المالية"],
  },
  {
    id: "completed",
    title: "مكتملة",
    color: "border-t-green-500",
    statuses: ["مكتملة"],
  },
  {
    id: "overdue",
    title: "متأخرة",
    color: "border-t-red-500",
    statuses: ["متأخرة"],
  },
];

interface TaskBoardViewProps {
  tasks: Task[];
  onUpdate?: (task: Task) => void;
}

export function TaskBoardView({ tasks, onUpdate }: TaskBoardViewProps) {
  const [selectedTask, setSelectedTask] = useState<Task | null>(null);
  const [draggedTask, setDraggedTask] = useState<Task | null>(null);

  const getColumnTasks = (column: BoardColumn) => {
    return tasks.filter((t) => column.statuses.includes(t.status) && t.status !== "ملغاة");
  };

  const handleDragStart = (task: Task) => {
    setDraggedTask(task);
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
  };

  const handleDrop = (column: BoardColumn) => {
    if (draggedTask && onUpdate) {
      const newStatus = column.statuses[0];
      onUpdate({ ...draggedTask, status: newStatus });
    }
    setDraggedTask(null);
  };

  return (
    <>
      <div className="flex gap-4 overflow-x-auto pb-4 min-h-[calc(100vh-250px)]">
        {boardColumns.map((column) => {
          const columnTasks = getColumnTasks(column);
          return (
            <div
              key={column.id}
              className={`flex-1 min-w-[280px] max-w-[350px] bg-secondary/30 border-t-2 ${column.color}`}
              onDragOver={handleDragOver}
              onDrop={() => handleDrop(column)}
            >
              {/* Column Header */}
              <div className="p-3 flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <h3 className="text-sm font-medium">{column.title}</h3>
                  <span className="text-xs bg-secondary px-1.5 py-0.5 text-muted-foreground">
                    {columnTasks.length}
                  </span>
                </div>
              </div>

              {/* Column Body */}
              <div className="p-2 space-y-2">
                {columnTasks.map((task) => (
                  <div
                    key={task.id}
                    draggable
                    onDragStart={() => handleDragStart(task)}
                    className="cursor-grab active:cursor-grabbing"
                  >
                    <TaskCard
                      task={task}
                      compact
                      onClick={() => setSelectedTask(task)}
                    />
                  </div>
                ))}
                {columnTasks.length === 0 && (
                  <div className="text-center py-8 text-xs text-muted-foreground">
                    لا توجد مهام
                  </div>
                )}
              </div>
            </div>
          );
        })}
      </div>

      <TaskDetailsDrawer
        task={selectedTask}
        open={!!selectedTask}
        onClose={() => setSelectedTask(null)}
        onUpdate={(updated) => {
          setSelectedTask(updated);
          onUpdate?.(updated);
        }}
      />
    </>
  );
}
