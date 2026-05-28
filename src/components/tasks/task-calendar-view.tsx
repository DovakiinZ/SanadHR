"use client";

import { useState, useMemo } from "react";
import { Task } from "@/types";
import { TaskDetailsDrawer } from "./task-details-drawer";
import { TaskPriorityBadge } from "./task-priority-badge";
import { ChevronRight, ChevronLeft } from "lucide-react";
import { Button } from "@/components/ui/button";

interface TaskCalendarViewProps {
  tasks: Task[];
  onUpdate?: (task: Task) => void;
}

const arabicMonths = [
  "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
  "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر",
];

const arabicDays = ["الأحد", "الإثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة", "السبت"];

export function TaskCalendarView({ tasks, onUpdate }: TaskCalendarViewProps) {
  const [currentDate, setCurrentDate] = useState(new Date());
  const [selectedTask, setSelectedTask] = useState<Task | null>(null);

  const year = currentDate.getFullYear();
  const month = currentDate.getMonth();

  const firstDay = new Date(year, month, 1).getDay();
  const daysInMonth = new Date(year, month + 1, 0).getDate();
  const today = new Date();

  const calendarDays = useMemo(() => {
    const days: (number | null)[] = [];
    for (let i = 0; i < firstDay; i++) days.push(null);
    for (let i = 1; i <= daysInMonth; i++) days.push(i);
    return days;
  }, [firstDay, daysInMonth]);

  const getTasksForDay = (day: number) => {
    const dateStr = `${year}-${String(month + 1).padStart(2, "0")}-${String(day).padStart(2, "0")}`;
    return tasks.filter((t) => t.dueDate === dateStr);
  };

  const isToday = (day: number) => {
    return day === today.getDate() && month === today.getMonth() && year === today.getFullYear();
  };

  const prevMonth = () => setCurrentDate(new Date(year, month - 1, 1));
  const nextMonth = () => setCurrentDate(new Date(year, month + 1, 1));

  return (
    <>
      {/* Calendar Header */}
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-lg font-bold">
          {arabicMonths[month]} {year}
        </h3>
        <div className="flex items-center gap-1">
          <Button variant="outline" size="sm" onClick={nextMonth} className="h-8 w-8 p-0">
            <ChevronRight className="h-4 w-4" />
          </Button>
          <Button variant="outline" size="sm" onClick={prevMonth} className="h-8 w-8 p-0">
            <ChevronLeft className="h-4 w-4" />
          </Button>
        </div>
      </div>

      {/* Day headers */}
      <div className="grid grid-cols-7 border border-border">
        {arabicDays.map((day) => (
          <div key={day} className="p-2 text-center text-xs font-bold text-muted-foreground border-b border-border bg-secondary/50">
            {day}
          </div>
        ))}

        {/* Calendar cells */}
        {calendarDays.map((day, i) => {
          const dayTasks = day ? getTasksForDay(day) : [];
          return (
            <div
              key={i}
              className={`min-h-[100px] p-1.5 border-b border-l border-border ${
                day ? "bg-card/30" : "bg-secondary/20"
              }`}
            >
              {day && (
                <>
                  <span
                    className={`inline-flex h-6 w-6 items-center justify-center text-xs ${
                      isToday(day)
                        ? "bg-primary text-primary-foreground font-bold"
                        : "text-muted-foreground"
                    }`}
                  >
                    {day}
                  </span>
                  <div className="mt-1 space-y-0.5">
                    {dayTasks.slice(0, 3).map((task) => (
                      <button
                        key={task.id}
                        onClick={() => setSelectedTask(task)}
                        className="w-full text-right px-1.5 py-0.5 text-xs truncate bg-secondary hover:bg-primary/10 hover:text-primary transition-colors"
                      >
                        {task.title}
                      </button>
                    ))}
                    {dayTasks.length > 3 && (
                      <span className="text-xs text-muted-foreground px-1.5">
                        +{dayTasks.length - 3} المزيد
                      </span>
                    )}
                  </div>
                </>
              )}
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
