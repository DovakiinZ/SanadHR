"use client";

import { useState, useMemo } from "react";
import { Task } from "@/types";
import { TaskCard } from "./task-card";
import { TaskFilters } from "./task-filters";
import { TaskDetailsDrawer } from "./task-details-drawer";
import { Button } from "@/components/ui/button";

interface TaskListViewProps {
  tasks: Task[];
  onUpdate?: (task: Task) => void;
  showFilters?: boolean;
}

export function TaskListView({ tasks: allTasks, onUpdate, showFilters = true }: TaskListViewProps) {
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [priorityFilter, setPriorityFilter] = useState("");
  const [sourceFilter, setSourceFilter] = useState("");
  const [departmentFilter, setDepartmentFilter] = useState("");
  const [selectedTask, setSelectedTask] = useState<Task | null>(null);
  const [page, setPage] = useState(1);
  const perPage = 10;

  const filtered = useMemo(() => {
    let result = allTasks;
    if (search) {
      result = result.filter(
        (t) =>
          t.title.includes(search) ||
          t.taskId.toLowerCase().includes(search.toLowerCase()) ||
          t.description.includes(search) ||
          t.assignee.includes(search)
      );
    }
    if (statusFilter) result = result.filter((t) => t.status === statusFilter);
    if (priorityFilter) result = result.filter((t) => t.priority === priorityFilter);
    if (sourceFilter) result = result.filter((t) => t.source === sourceFilter);
    if (departmentFilter) result = result.filter((t) => t.department === departmentFilter);
    return result;
  }, [allTasks, search, statusFilter, priorityFilter, sourceFilter, departmentFilter]);

  const displayed = filtered.slice((page - 1) * perPage, page * perPage);
  const totalPages = Math.ceil(filtered.length / perPage);

  return (
    <div>
      {showFilters && (
        <div className="mb-4">
          <TaskFilters
            search={search}
            onSearchChange={(v) => { setSearch(v); setPage(1); }}
            statusFilter={statusFilter}
            onStatusChange={(v) => { setStatusFilter(v); setPage(1); }}
            priorityFilter={priorityFilter}
            onPriorityChange={(v) => { setPriorityFilter(v); setPage(1); }}
            sourceFilter={sourceFilter}
            onSourceChange={(v) => { setSourceFilter(v); setPage(1); }}
            departmentFilter={departmentFilter}
            onDepartmentChange={(v) => { setDepartmentFilter(v); setPage(1); }}
            resultCount={filtered.length}
          />
        </div>
      )}

      <div className="space-y-2">
        {displayed.length === 0 && (
          <div className="text-center py-12 text-muted-foreground">
            <p className="text-sm">لا توجد مهام</p>
          </div>
        )}
        {displayed.map((task) => (
          <TaskCard key={task.id} task={task} onClick={() => setSelectedTask(task)} />
        ))}
      </div>

      {totalPages > 1 && (
        <div className="mt-4 flex items-center justify-between">
          <p className="text-xs text-muted-foreground">
            عرض {(page - 1) * perPage + 1} - {Math.min(page * perPage, filtered.length)} من {filtered.length}
          </p>
          <div className="flex items-center gap-1">
            <Button variant="outline" size="sm" onClick={() => setPage(page - 1)} disabled={page === 1} className="h-8 text-xs">
              السابق
            </Button>
            {Array.from({ length: totalPages }, (_, i) => (
              <Button key={i + 1} variant={page === i + 1 ? "default" : "outline"} size="sm" onClick={() => setPage(i + 1)} className="h-8 w-8 text-xs">
                {i + 1}
              </Button>
            ))}
            <Button variant="outline" size="sm" onClick={() => setPage(page + 1)} disabled={page === totalPages} className="h-8 text-xs">
              التالي
            </Button>
          </div>
        </div>
      )}

      <TaskDetailsDrawer
        task={selectedTask}
        open={!!selectedTask}
        onClose={() => setSelectedTask(null)}
        onUpdate={(updated) => {
          setSelectedTask(updated);
          onUpdate?.(updated);
        }}
      />
    </div>
  );
}
