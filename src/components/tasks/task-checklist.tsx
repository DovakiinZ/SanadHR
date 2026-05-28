"use client";

import { useState } from "react";
import { Check, Plus, Trash2 } from "lucide-react";
import { TaskChecklistItem } from "@/types";
import { Input } from "@/components/ui/input";

interface TaskChecklistProps {
  items: TaskChecklistItem[];
  onChange?: (items: TaskChecklistItem[]) => void;
  readOnly?: boolean;
}

export function TaskChecklist({ items, onChange, readOnly = false }: TaskChecklistProps) {
  const [newItem, setNewItem] = useState("");

  const completed = items.filter((i) => i.completed).length;
  const total = items.length;
  const percentage = total > 0 ? Math.round((completed / total) * 100) : 0;

  const toggleItem = (id: string) => {
    if (readOnly || !onChange) return;
    const updated = items.map((item) =>
      item.id === id
        ? {
            ...item,
            completed: !item.completed,
            completedAt: !item.completed ? new Date().toISOString().split("T")[0] : undefined,
            completedBy: !item.completed ? "المستخدم الحالي" : undefined,
          }
        : item
    );
    onChange(updated);
  };

  const addItem = () => {
    if (!newItem.trim() || !onChange) return;
    const item: TaskChecklistItem = {
      id: `new-${Date.now()}`,
      title: newItem.trim(),
      completed: false,
    };
    onChange([...items, item]);
    setNewItem("");
  };

  const removeItem = (id: string) => {
    if (readOnly || !onChange) return;
    onChange(items.filter((i) => i.id !== id));
  };

  return (
    <div className="space-y-3">
      {/* Progress */}
      {total > 0 && (
        <div className="flex items-center gap-3">
          <div className="flex-1 h-1.5 bg-secondary overflow-hidden">
            <div
              className="h-full bg-primary transition-all duration-300"
              style={{ width: `${percentage}%` }}
            />
          </div>
          <span className="text-xs text-muted-foreground whitespace-nowrap">
            {completed}/{total}
          </span>
        </div>
      )}

      {/* Items */}
      <div className="space-y-1">
        {items.map((item) => (
          <div
            key={item.id}
            className="flex items-center gap-2 group py-1.5 px-2 hover:bg-secondary/50 transition-colors"
          >
            <button
              onClick={() => toggleItem(item.id)}
              disabled={readOnly}
              className={`flex-shrink-0 h-4 w-4 border flex items-center justify-center transition-colors ${
                item.completed
                  ? "bg-primary border-primary text-primary-foreground"
                  : "border-border hover:border-primary/50"
              }`}
            >
              {item.completed && <Check className="h-3 w-3" />}
            </button>
            <span
              className={`flex-1 text-sm ${
                item.completed ? "line-through text-muted-foreground" : "text-foreground"
              }`}
            >
              {item.title}
            </span>
            {item.completedBy && (
              <span className="text-xs text-muted-foreground">{item.completedBy}</span>
            )}
            {!readOnly && (
              <button
                onClick={() => removeItem(item.id)}
                className="opacity-0 group-hover:opacity-100 text-muted-foreground hover:text-destructive transition-all"
              >
                <Trash2 className="h-3 w-3" />
              </button>
            )}
          </div>
        ))}
      </div>

      {/* Add item */}
      {!readOnly && (
        <div className="flex items-center gap-2">
          <Plus className="h-4 w-4 text-muted-foreground flex-shrink-0" />
          <Input
            value={newItem}
            onChange={(e) => setNewItem(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && addItem()}
            placeholder="إضافة عنصر جديد..."
            className="h-8 bg-transparent border-0 border-b border-border text-sm px-0 focus-visible:ring-0"
          />
        </div>
      )}
    </div>
  );
}
