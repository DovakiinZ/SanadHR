"use client";

import { Task } from "@/types";
import { TaskStatusBadge } from "./task-status-badge";
import { TaskPriorityBadge } from "./task-priority-badge";
import { TaskSourceBadge } from "./task-source-badge";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Calendar, CheckSquare, MessageSquare, Paperclip, User } from "lucide-react";

interface TaskCardProps {
  task: Task;
  onClick?: () => void;
  compact?: boolean;
}

export function TaskCard({ task, onClick, compact = false }: TaskCardProps) {
  const completedChecklist = task.checklist.filter((c) => c.completed).length;
  const totalChecklist = task.checklist.length;

  if (compact) {
    return (
      <div
        onClick={onClick}
        className="p-3 bg-card border border-border hover:border-primary/30 transition-colors cursor-pointer group"
      >
        <div className="flex items-start justify-between gap-2 mb-2">
          <h4 className="text-sm font-medium group-hover:text-primary transition-colors line-clamp-2">
            {task.title}
          </h4>
        </div>
        <div className="flex items-center gap-2 flex-wrap">
          <TaskPriorityBadge priority={task.priority} />
          {task.relatedEmployee && (
            <span className="text-xs text-muted-foreground flex items-center gap-1">
              <User className="h-3 w-3" />
              {task.relatedEmployee}
            </span>
          )}
        </div>
        <div className="flex items-center justify-between mt-2 text-xs text-muted-foreground">
          <div className="flex items-center gap-3">
            {totalChecklist > 0 && (
              <span className="flex items-center gap-1">
                <CheckSquare className="h-3 w-3" />
                {completedChecklist}/{totalChecklist}
              </span>
            )}
            {task.comments.length > 0 && (
              <span className="flex items-center gap-1">
                <MessageSquare className="h-3 w-3" />
                {task.comments.length}
              </span>
            )}
            {task.attachments.length > 0 && (
              <span className="flex items-center gap-1">
                <Paperclip className="h-3 w-3" />
                {task.attachments.length}
              </span>
            )}
          </div>
          <Avatar className="h-5 w-5">
            <AvatarFallback className="bg-secondary text-[10px] font-bold border border-border">
              {task.assignee.charAt(0)}
            </AvatarFallback>
          </Avatar>
        </div>
      </div>
    );
  }

  return (
    <div
      onClick={onClick}
      className="p-4 bg-card border border-border hover:border-primary/30 transition-colors cursor-pointer group"
    >
      <div className="flex items-start justify-between gap-3">
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2 mb-1">
            <span className="text-xs font-mono text-muted-foreground">{task.taskId}</span>
            <TaskSourceBadge source={task.source} />
          </div>
          <h3 className="text-sm font-medium group-hover:text-primary transition-colors mb-2">
            {task.title}
          </h3>
          <p className="text-xs text-muted-foreground line-clamp-1 mb-3">{task.description}</p>
          <div className="flex items-center gap-2 flex-wrap">
            <TaskStatusBadge status={task.status} />
            <TaskPriorityBadge priority={task.priority} />
            {task.tags.map((tag) => (
              <span key={tag} className="text-xs bg-secondary px-2 py-0.5 text-muted-foreground">
                {tag}
              </span>
            ))}
          </div>
        </div>
        <div className="flex flex-col items-end gap-2 flex-shrink-0">
          <Avatar className="h-7 w-7">
            <AvatarFallback className="bg-secondary text-xs font-bold border border-border">
              {task.assignee.charAt(0)}
            </AvatarFallback>
          </Avatar>
          <span className="text-xs text-muted-foreground flex items-center gap-1">
            <Calendar className="h-3 w-3" />
            {task.dueDate}
          </span>
        </div>
      </div>

      {/* Footer meta */}
      <div className="flex items-center justify-between mt-3 pt-3 border-t border-border/50">
        <div className="flex items-center gap-2 text-xs text-muted-foreground">
          <span>{task.assignee}</span>
          {task.department && (
            <>
              <span className="text-border">·</span>
              <span>{task.department}</span>
            </>
          )}
        </div>
        <div className="flex items-center gap-3 text-xs text-muted-foreground">
          {totalChecklist > 0 && (
            <span className="flex items-center gap-1">
              <CheckSquare className="h-3 w-3" />
              {completedChecklist}/{totalChecklist}
            </span>
          )}
          {task.comments.length > 0 && (
            <span className="flex items-center gap-1">
              <MessageSquare className="h-3 w-3" />
              {task.comments.length}
            </span>
          )}
          {task.attachments.length > 0 && (
            <span className="flex items-center gap-1">
              <Paperclip className="h-3 w-3" />
              {task.attachments.length}
            </span>
          )}
        </div>
      </div>
    </div>
  );
}
