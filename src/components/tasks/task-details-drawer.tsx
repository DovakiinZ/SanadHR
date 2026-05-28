"use client";

import { useState } from "react";
import { Task, TaskChecklistItem, TaskComment } from "@/types";
import { TaskStatusBadge } from "./task-status-badge";
import { TaskPriorityBadge } from "./task-priority-badge";
import { TaskSourceBadge } from "./task-source-badge";
import { TaskChecklist } from "./task-checklist";
import { TaskComments } from "./task-comments";
import { TaskActivityLog } from "./task-activity-log";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import {
  Calendar,
  User,
  Building2,
  FileText,
  Banknote,
  Users,
  Link2,
  Play,
  CheckCircle2,
  UserPlus,
  Paperclip,
  Tag,
} from "lucide-react";

interface TaskDetailsDrawerProps {
  task: Task | null;
  open: boolean;
  onClose: () => void;
  onUpdate?: (task: Task) => void;
}

export function TaskDetailsDrawer({ task, open, onClose, onUpdate }: TaskDetailsDrawerProps) {
  if (!task) return null;

  const handleChecklistChange = (items: TaskChecklistItem[]) => {
    if (onUpdate) {
      onUpdate({ ...task, checklist: items });
    }
  };

  const handleAddComment = (comment: TaskComment) => {
    if (onUpdate) {
      onUpdate({ ...task, comments: [...task.comments, comment] });
    }
  };

  const handleStatusChange = (newStatus: Task["status"]) => {
    if (onUpdate) {
      onUpdate({ ...task, status: newStatus });
    }
  };

  return (
    <Sheet open={open} onOpenChange={(o) => !o && onClose()}>
      <SheetContent side="left" className="w-full sm:max-w-2xl overflow-y-auto p-0">
        {/* Header */}
        <SheetHeader className="p-5 pb-0">
          <div className="flex items-center gap-2 mb-2">
            <span className="text-xs font-mono text-muted-foreground">{task.taskId}</span>
            <TaskSourceBadge source={task.source} />
          </div>
          <SheetTitle className="text-lg leading-relaxed">{task.title}</SheetTitle>
          <p className="text-sm text-muted-foreground mt-1">{task.description}</p>
        </SheetHeader>

        {/* Actions */}
        <div className="px-5 py-3 flex items-center gap-2 border-b border-border">
          {task.status !== "مكتملة" && task.status !== "ملغاة" && (
            <>
              {task.status === "جديدة" && (
                <Button
                  size="sm"
                  className="h-8 text-xs gap-1"
                  onClick={() => handleStatusChange("قيد التنفيذ")}
                >
                  <Play className="h-3 w-3" />
                  بدء المهمة
                </Button>
              )}
              <Button
                size="sm"
                variant="outline"
                className="h-8 text-xs gap-1 border-green-500/30 text-green-400 hover:bg-green-500/10"
                onClick={() => handleStatusChange("مكتملة")}
              >
                <CheckCircle2 className="h-3 w-3" />
                إكمال
              </Button>
              <Button
                size="sm"
                variant="outline"
                className="h-8 text-xs gap-1"
                onClick={() => {}}
              >
                <UserPlus className="h-3 w-3" />
                إعادة تعيين
              </Button>
            </>
          )}
          <div className="flex items-center gap-2 mr-auto">
            <TaskStatusBadge status={task.status} />
            <TaskPriorityBadge priority={task.priority} />
          </div>
        </div>

        {/* Meta info */}
        <div className="px-5 py-4 grid grid-cols-2 gap-3 border-b border-border">
          <MetaItem icon={User} label="المسؤول" value={task.assignee} />
          <MetaItem icon={Calendar} label="تاريخ الاستحقاق" value={task.dueDate} />
          <MetaItem icon={Building2} label="القسم" value={task.department} />
          <MetaItem icon={User} label="أنشئ بواسطة" value={task.createdBy} />
          {task.relatedEmployee && (
            <MetaItem icon={Users} label="الموظف المرتبط" value={task.relatedEmployee} />
          )}
          {task.relatedRequest && (
            <MetaItem icon={FileText} label="الطلب المرتبط" value={task.relatedRequest} />
          )}
          {task.relatedDocument && (
            <MetaItem icon={Link2} label="المستند المرتبط" value={task.relatedDocument} />
          )}
          {task.relatedPayrollRun && (
            <MetaItem icon={Banknote} label="كشف الرواتب" value={task.relatedPayrollRun} />
          )}
          {task.relatedCandidate && (
            <MetaItem icon={Users} label="المرشح" value={task.relatedCandidate} />
          )}
        </div>

        {/* Tags */}
        {task.tags.length > 0 && (
          <div className="px-5 py-3 border-b border-border flex items-center gap-2">
            <Tag className="h-3.5 w-3.5 text-muted-foreground" />
            <div className="flex items-center gap-1.5 flex-wrap">
              {task.tags.map((tag) => (
                <span key={tag} className="text-xs bg-secondary px-2 py-0.5 text-muted-foreground">
                  {tag}
                </span>
              ))}
            </div>
          </div>
        )}

        {/* Attachments */}
        {task.attachments.length > 0 && (
          <div className="px-5 py-3 border-b border-border">
            <div className="flex items-center gap-2 mb-2">
              <Paperclip className="h-3.5 w-3.5 text-muted-foreground" />
              <span className="text-xs font-medium text-muted-foreground">المرفقات ({task.attachments.length})</span>
            </div>
            <div className="space-y-1">
              {task.attachments.map((att) => (
                <div key={att.id} className="flex items-center gap-2 py-1.5 px-2 bg-secondary/50 text-sm">
                  <FileText className="h-3.5 w-3.5 text-muted-foreground" />
                  <span className="flex-1">{att.name}</span>
                  <span className="text-xs text-muted-foreground">{att.size}</span>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Tabs: Checklist / Comments / Activity */}
        <div className="px-5 py-4">
          <Tabs defaultValue="checklist">
            <TabsList variant="line" className="mb-4">
              <TabsTrigger value="checklist">
                قائمة المهام ({task.checklist.length})
              </TabsTrigger>
              <TabsTrigger value="comments">
                التعليقات ({task.comments.length})
              </TabsTrigger>
              <TabsTrigger value="activity">
                السجل ({task.activityLog.length})
              </TabsTrigger>
            </TabsList>
            <TabsContent value="checklist">
              <TaskChecklist
                items={task.checklist}
                onChange={handleChecklistChange}
              />
            </TabsContent>
            <TabsContent value="comments">
              <TaskComments
                comments={task.comments}
                onAdd={handleAddComment}
              />
            </TabsContent>
            <TabsContent value="activity">
              <TaskActivityLog logs={task.activityLog} />
            </TabsContent>
          </Tabs>
        </div>
      </SheetContent>
    </Sheet>
  );
}

function MetaItem({ icon: Icon, label, value }: { icon: React.ElementType; label: string; value: string }) {
  return (
    <div className="flex items-center gap-2">
      <Icon className="h-3.5 w-3.5 text-muted-foreground flex-shrink-0" />
      <div>
        <p className="text-xs text-muted-foreground">{label}</p>
        <p className="text-sm font-medium">{value}</p>
      </div>
    </div>
  );
}
