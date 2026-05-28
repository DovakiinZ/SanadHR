"use client";

import { useState } from "react";
import { Task, TaskPriority, TaskSource, TaskStatus, TaskChecklistItem } from "@/types";
import { taskPriorities, taskSources, taskStatuses } from "@/lib/tasks-mock-data";
import { departments } from "@/lib/mock-data";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Plus, Trash2, Check } from "lucide-react";

interface TaskCreateModalProps {
  open: boolean;
  onClose: () => void;
  onCreate: (task: Task) => void;
}

export function TaskCreateModal({ open, onClose, onCreate }: TaskCreateModalProps) {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [priority, setPriority] = useState<TaskPriority>("متوسطة");
  const [source, setSource] = useState<TaskSource>("يدوي");
  const [assignee, setAssignee] = useState("");
  const [department, setDepartment] = useState("");
  const [dueDate, setDueDate] = useState("");
  const [relatedEmployee, setRelatedEmployee] = useState("");
  const [tags, setTags] = useState("");
  const [checklistItems, setChecklistItems] = useState<string[]>([]);
  const [newChecklistItem, setNewChecklistItem] = useState("");

  const addChecklistItem = () => {
    if (!newChecklistItem.trim()) return;
    setChecklistItems([...checklistItems, newChecklistItem.trim()]);
    setNewChecklistItem("");
  };

  const removeChecklistItem = (index: number) => {
    setChecklistItems(checklistItems.filter((_, i) => i !== index));
  };

  const handleSubmit = () => {
    if (!title.trim()) return;

    const taskNum = Math.floor(Math.random() * 900) + 100;
    const newTask: Task = {
      id: `new-${Date.now()}`,
      taskId: `TSK-${taskNum}`,
      title: title.trim(),
      description: description.trim(),
      status: "جديدة" as TaskStatus,
      priority,
      source,
      assignee: assignee || "غير معين",
      createdBy: "المستخدم الحالي",
      department: department || "عام",
      relatedEmployee: relatedEmployee || undefined,
      dueDate: dueDate || new Date().toISOString().split("T")[0],
      tags: tags ? tags.split(",").map((t) => t.trim()).filter(Boolean) : [],
      checklist: checklistItems.map((item, i) => ({
        id: `cl-${i}`,
        title: item,
        completed: false,
      })),
      comments: [],
      attachments: [],
      activityLog: [
        {
          id: "al-1",
          action: "إنشاء المهمة",
          user: "المستخدم الحالي",
          timestamp: new Date().toISOString(),
        },
      ],
      createdAt: new Date().toISOString().split("T")[0],
      updatedAt: new Date().toISOString().split("T")[0],
    };

    onCreate(newTask);
    resetForm();
    onClose();
  };

  const resetForm = () => {
    setTitle("");
    setDescription("");
    setPriority("متوسطة");
    setSource("يدوي");
    setAssignee("");
    setDepartment("");
    setDueDate("");
    setRelatedEmployee("");
    setTags("");
    setChecklistItems([]);
    setNewChecklistItem("");
  };

  return (
    <Dialog open={open} onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="sm:max-w-xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>إنشاء مهمة جديدة</DialogTitle>
        </DialogHeader>

        <div className="space-y-4 py-2">
          {/* Title */}
          <div className="space-y-1.5">
            <Label className="text-xs">عنوان المهمة *</Label>
            <Input
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="أدخل عنوان المهمة..."
              className="bg-secondary border-border h-9 text-sm"
            />
          </div>

          {/* Description */}
          <div className="space-y-1.5">
            <Label className="text-xs">الوصف</Label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="وصف تفصيلي للمهمة..."
              rows={3}
              className="w-full bg-secondary border border-border px-3 py-2 text-sm text-foreground resize-none focus:outline-none focus:ring-1 focus:ring-ring"
            />
          </div>

          {/* Priority + Source */}
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label className="text-xs">الأولوية</Label>
              <select
                value={priority}
                onChange={(e) => setPriority(e.target.value as TaskPriority)}
                className="w-full h-9 bg-secondary border border-border px-3 text-sm text-foreground"
              >
                {taskPriorities.map((p) => (
                  <option key={p.value} value={p.value}>{p.icon} {p.label}</option>
                ))}
              </select>
            </div>
            <div className="space-y-1.5">
              <Label className="text-xs">المصدر</Label>
              <select
                value={source}
                onChange={(e) => setSource(e.target.value as TaskSource)}
                className="w-full h-9 bg-secondary border border-border px-3 text-sm text-foreground"
              >
                {taskSources.map((s) => (
                  <option key={s.value} value={s.value}>{s.label}</option>
                ))}
              </select>
            </div>
          </div>

          {/* Assignee + Department */}
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label className="text-xs">المسؤول</Label>
              <Input
                value={assignee}
                onChange={(e) => setAssignee(e.target.value)}
                placeholder="اسم المسؤول..."
                className="bg-secondary border-border h-9 text-sm"
              />
            </div>
            <div className="space-y-1.5">
              <Label className="text-xs">القسم</Label>
              <select
                value={department}
                onChange={(e) => setDepartment(e.target.value)}
                className="w-full h-9 bg-secondary border border-border px-3 text-sm text-foreground"
              >
                <option value="">اختر القسم</option>
                {departments.map((d) => (
                  <option key={d.id} value={d.name}>{d.name}</option>
                ))}
              </select>
            </div>
          </div>

          {/* Due Date + Related Employee */}
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label className="text-xs">تاريخ الاستحقاق</Label>
              <Input
                type="date"
                value={dueDate}
                onChange={(e) => setDueDate(e.target.value)}
                className="bg-secondary border-border h-9 text-sm"
              />
            </div>
            <div className="space-y-1.5">
              <Label className="text-xs">الموظف المرتبط</Label>
              <Input
                value={relatedEmployee}
                onChange={(e) => setRelatedEmployee(e.target.value)}
                placeholder="اسم الموظف..."
                className="bg-secondary border-border h-9 text-sm"
              />
            </div>
          </div>

          {/* Tags */}
          <div className="space-y-1.5">
            <Label className="text-xs">الوسوم (مفصولة بفاصلة)</Label>
            <Input
              value={tags}
              onChange={(e) => setTags(e.target.value)}
              placeholder="تهيئة, موظف جديد..."
              className="bg-secondary border-border h-9 text-sm"
            />
          </div>

          {/* Checklist */}
          <div className="space-y-2">
            <Label className="text-xs">قائمة المهام الفرعية</Label>
            {checklistItems.map((item, i) => (
              <div key={i} className="flex items-center gap-2 py-1 px-2 bg-secondary/50">
                <Check className="h-3.5 w-3.5 text-muted-foreground" />
                <span className="flex-1 text-sm">{item}</span>
                <button
                  onClick={() => removeChecklistItem(i)}
                  className="text-muted-foreground hover:text-destructive"
                >
                  <Trash2 className="h-3 w-3" />
                </button>
              </div>
            ))}
            <div className="flex items-center gap-2">
              <Plus className="h-4 w-4 text-muted-foreground" />
              <Input
                value={newChecklistItem}
                onChange={(e) => setNewChecklistItem(e.target.value)}
                onKeyDown={(e) => e.key === "Enter" && addChecklistItem()}
                placeholder="إضافة عنصر..."
                className="h-8 bg-transparent border-0 border-b border-border text-sm px-0 focus-visible:ring-0"
              />
            </div>
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={onClose} className="h-9 text-sm">
            إلغاء
          </Button>
          <Button onClick={handleSubmit} disabled={!title.trim()} className="h-9 text-sm">
            إنشاء المهمة
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
