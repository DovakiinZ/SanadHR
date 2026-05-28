"use client";

import { Task } from "@/types";
import { taskPriorities } from "@/lib/tasks-mock-data";
import {
  AlertTriangle,
  CheckCircle2,
  Clock,
  ListTodo,
  TrendingUp,
  Calendar,
  Building2,
  Zap,
} from "lucide-react";

interface WidgetProps {
  tasks: Task[];
}

function WidgetCard({
  icon: Icon,
  title,
  value,
  subtitle,
  color,
}: {
  icon: React.ElementType;
  title: string;
  value: string | number;
  subtitle?: string;
  color: string;
}) {
  return (
    <div className="p-4 bg-card border border-border">
      <div className="flex items-center justify-between mb-3">
        <span className="text-xs font-medium text-muted-foreground">{title}</span>
        <div className={`h-8 w-8 flex items-center justify-center ${color}`}>
          <Icon className="h-4 w-4" />
        </div>
      </div>
      <p className="text-2xl font-bold">{value}</p>
      {subtitle && <p className="text-xs text-muted-foreground mt-1">{subtitle}</p>}
    </div>
  );
}

export function TaskDashboardWidgets({ tasks }: WidgetProps) {
  const pending = tasks.filter((t) => !["مكتملة", "ملغاة"].includes(t.status));
  const overdue = tasks.filter((t) => t.status === "متأخرة");
  const completed = tasks.filter((t) => t.status === "مكتملة");
  const todayStr = new Date().toISOString().split("T")[0];
  const dueToday = tasks.filter((t) => t.dueDate === todayStr && t.status !== "مكتملة");
  const inProgress = tasks.filter((t) => t.status === "قيد التنفيذ");
  const fromWorkflow = tasks.filter((t) => t.source !== "يدوي" && !["مكتملة", "ملغاة"].includes(t.status));

  return (
    <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
      <WidgetCard
        icon={ListTodo}
        title="المهام المعلقة"
        value={pending.length}
        subtitle={`${inProgress.length} قيد التنفيذ`}
        color="bg-blue-500/10 text-blue-400"
      />
      <WidgetCard
        icon={AlertTriangle}
        title="مهام متأخرة"
        value={overdue.length}
        subtitle="تحتاج متابعة"
        color="bg-red-500/10 text-red-400"
      />
      <WidgetCard
        icon={Calendar}
        title="مستحقة اليوم"
        value={dueToday.length}
        color="bg-amber-500/10 text-amber-400"
      />
      <WidgetCard
        icon={CheckCircle2}
        title="مكتملة"
        value={completed.length}
        subtitle={`من ${tasks.length} مهمة`}
        color="bg-green-500/10 text-green-400"
      />
    </div>
  );
}

export function TasksByPriorityWidget({ tasks }: WidgetProps) {
  const pending = tasks.filter((t) => !["مكتملة", "ملغاة"].includes(t.status));

  return (
    <div className="p-4 bg-card border border-border">
      <h3 className="text-sm font-medium mb-4">المهام حسب الأولوية</h3>
      <div className="space-y-3">
        {taskPriorities.map((p) => {
          const count = pending.filter((t) => t.priority === p.value).length;
          const percentage = pending.length > 0 ? (count / pending.length) * 100 : 0;
          return (
            <div key={p.value} className="space-y-1">
              <div className="flex items-center justify-between text-sm">
                <span className="flex items-center gap-1.5">
                  <span>{p.icon}</span>
                  <span className="text-xs">{p.label}</span>
                </span>
                <span className="text-xs text-muted-foreground">{count}</span>
              </div>
              <div className="h-1.5 bg-secondary overflow-hidden">
                <div
                  className="h-full bg-primary transition-all"
                  style={{ width: `${percentage}%` }}
                />
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

export function TasksByDepartmentWidget({ tasks }: WidgetProps) {
  const pending = tasks.filter((t) => !["مكتملة", "ملغاة"].includes(t.status));
  const deptCounts = pending.reduce<Record<string, number>>((acc, t) => {
    acc[t.department] = (acc[t.department] || 0) + 1;
    return acc;
  }, {});

  const sorted = Object.entries(deptCounts).sort((a, b) => b[1] - a[1]);

  return (
    <div className="p-4 bg-card border border-border">
      <h3 className="text-sm font-medium mb-4">المهام حسب القسم</h3>
      <div className="space-y-3">
        {sorted.map(([dept, count]) => {
          const percentage = pending.length > 0 ? (count / pending.length) * 100 : 0;
          return (
            <div key={dept} className="space-y-1">
              <div className="flex items-center justify-between">
                <span className="text-xs">{dept}</span>
                <span className="text-xs text-muted-foreground">{count}</span>
              </div>
              <div className="h-1.5 bg-secondary overflow-hidden">
                <div
                  className="h-full bg-primary transition-all"
                  style={{ width: `${percentage}%` }}
                />
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

export function TasksSourceWidget({ tasks }: WidgetProps) {
  const pending = tasks.filter((t) => !["مكتملة", "ملغاة"].includes(t.status));
  const auto = pending.filter((t) => t.source !== "يدوي").length;
  const manual = pending.filter((t) => t.source === "يدوي").length;

  return (
    <div className="p-4 bg-card border border-border">
      <h3 className="text-sm font-medium mb-4">مصادر المهام</h3>
      <div className="grid grid-cols-2 gap-3">
        <div className="p-3 bg-secondary/50 text-center">
          <Zap className="h-5 w-5 text-primary mx-auto mb-1" />
          <p className="text-lg font-bold">{auto}</p>
          <p className="text-xs text-muted-foreground">تلقائية</p>
        </div>
        <div className="p-3 bg-secondary/50 text-center">
          <TrendingUp className="h-5 w-5 text-muted-foreground mx-auto mb-1" />
          <p className="text-lg font-bold">{manual}</p>
          <p className="text-xs text-muted-foreground">يدوية</p>
        </div>
      </div>
    </div>
  );
}
