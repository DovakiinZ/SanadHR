"use client";

import { Search, Filter } from "lucide-react";
import { Input } from "@/components/ui/input";
import { TaskStatus, TaskPriority, TaskSource } from "@/types";
import { taskStatuses, taskPriorities, taskSources } from "@/lib/tasks-mock-data";
import { departments } from "@/lib/mock-data";

interface TaskFiltersProps {
  search: string;
  onSearchChange: (value: string) => void;
  statusFilter: string;
  onStatusChange: (value: string) => void;
  priorityFilter: string;
  onPriorityChange: (value: string) => void;
  sourceFilter: string;
  onSourceChange: (value: string) => void;
  departmentFilter: string;
  onDepartmentChange: (value: string) => void;
  resultCount: number;
}

export function TaskFilters({
  search,
  onSearchChange,
  statusFilter,
  onStatusChange,
  priorityFilter,
  onPriorityChange,
  sourceFilter,
  onSourceChange,
  departmentFilter,
  onDepartmentChange,
  resultCount,
}: TaskFiltersProps) {
  return (
    <div className="flex flex-wrap items-center gap-3">
      <div className="relative flex-1 min-w-[200px]">
        <Search className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          placeholder="بحث في المهام..."
          value={search}
          onChange={(e) => onSearchChange(e.target.value)}
          className="pr-10 bg-secondary border-border h-9 text-sm"
        />
      </div>
      <select
        value={statusFilter}
        onChange={(e) => onStatusChange(e.target.value)}
        className="h-9 bg-secondary border border-border px-3 text-sm text-foreground"
      >
        <option value="">كل الحالات</option>
        {taskStatuses.map((s) => (
          <option key={s.value} value={s.value}>{s.label}</option>
        ))}
      </select>
      <select
        value={priorityFilter}
        onChange={(e) => onPriorityChange(e.target.value)}
        className="h-9 bg-secondary border border-border px-3 text-sm text-foreground"
      >
        <option value="">كل الأولويات</option>
        {taskPriorities.map((p) => (
          <option key={p.value} value={p.value}>{p.label}</option>
        ))}
      </select>
      <select
        value={sourceFilter}
        onChange={(e) => onSourceChange(e.target.value)}
        className="h-9 bg-secondary border border-border px-3 text-sm text-foreground"
      >
        <option value="">كل المصادر</option>
        {taskSources.map((s) => (
          <option key={s.value} value={s.value}>{s.label}</option>
        ))}
      </select>
      <select
        value={departmentFilter}
        onChange={(e) => onDepartmentChange(e.target.value)}
        className="h-9 bg-secondary border border-border px-3 text-sm text-foreground"
      >
        <option value="">كل الأقسام</option>
        {departments.map((d) => (
          <option key={d.id} value={d.name}>{d.name}</option>
        ))}
      </select>
      <div className="flex items-center gap-1 text-xs text-muted-foreground">
        <Filter className="h-3.5 w-3.5" />
        <span>{resultCount} مهمة</span>
      </div>
    </div>
  );
}
