"use client";

import { useState } from "react";
import Link from "next/link";
import { ArrowRight, Plus, Trash2, GripVertical } from "lucide-react";
import { taskStatuses, taskPriorities } from "@/lib/tasks-mock-data";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";

export default function TaskSettingsPage() {
  const [statuses] = useState(taskStatuses);
  const [priorities] = useState(taskPriorities);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <div className="flex items-center gap-2 text-sm text-muted-foreground mb-2">
          <Link href="/tasks" className="hover:text-primary transition-colors">المهام</Link>
          <ArrowRight className="h-3 w-3 rotate-180" />
          <span>الإعدادات</span>
        </div>
        <h1 className="text-2xl font-bold">إعدادات المهام</h1>
        <p className="text-sm text-muted-foreground mt-1">تخصيص الحالات والأولويات والإعدادات العامة</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Statuses */}
        <div className="bg-card border border-border p-5">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-base font-medium">حالات المهام</h2>
            <Button variant="outline" size="sm" className="h-8 text-xs gap-1">
              <Plus className="h-3 w-3" />
              إضافة حالة
            </Button>
          </div>
          <div className="space-y-2">
            {statuses.map((status) => (
              <div key={status.value} className="flex items-center gap-3 p-2 bg-secondary/30 border border-border/50">
                <GripVertical className="h-4 w-4 text-muted-foreground cursor-grab" />
                <Badge variant="outline" className={`text-xs ${status.color}`}>
                  {status.label}
                </Badge>
                <span className="flex-1 text-sm text-muted-foreground">{status.value}</span>
                <button className="text-muted-foreground hover:text-destructive transition-colors">
                  <Trash2 className="h-3.5 w-3.5" />
                </button>
              </div>
            ))}
          </div>
        </div>

        {/* Priorities */}
        <div className="bg-card border border-border p-5">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-base font-medium">أولويات المهام</h2>
          </div>
          <div className="space-y-2">
            {priorities.map((priority) => (
              <div key={priority.value} className="flex items-center gap-3 p-2 bg-secondary/30 border border-border/50">
                <span className="text-sm">{priority.icon}</span>
                <Badge variant="outline" className={`text-xs ${priority.color}`}>
                  {priority.label}
                </Badge>
                <span className="flex-1 text-sm text-muted-foreground">{priority.value}</span>
              </div>
            ))}
          </div>
        </div>

        {/* Notifications */}
        <div className="bg-card border border-border p-5">
          <h2 className="text-base font-medium mb-4">إعدادات الإشعارات</h2>
          <div className="space-y-3">
            {[
              { label: "عند تعيين مهمة", key: "assigned", checked: true },
              { label: "قبل الاستحقاق بيوم", key: "due_soon", checked: true },
              { label: "عند التأخر", key: "overdue", checked: true },
              { label: "عند إكمال المهمة", key: "completed", checked: false },
              { label: "عند إضافة تعليق", key: "commented", checked: true },
              { label: "عند إعادة التعيين", key: "reassigned", checked: true },
              { label: "عند إكمال قائمة المهام", key: "checklist_done", checked: false },
            ].map((item) => (
              <label key={item.key} className="flex items-center gap-3 cursor-pointer">
                <input
                  type="checkbox"
                  defaultChecked={item.checked}
                  className="h-4 w-4 accent-primary"
                />
                <span className="text-sm">{item.label}</span>
              </label>
            ))}
          </div>
        </div>

        {/* Permissions */}
        <div className="bg-card border border-border p-5">
          <h2 className="text-base font-medium mb-4">الصلاحيات</h2>
          <div className="space-y-2">
            {[
              { code: "tasks.view", label: "عرض المهام" },
              { code: "tasks.create", label: "إنشاء مهام" },
              { code: "tasks.edit", label: "تعديل المهام" },
              { code: "tasks.delete", label: "حذف المهام" },
              { code: "tasks.assign", label: "تعيين المهام" },
              { code: "tasks.complete", label: "إكمال المهام" },
              { code: "tasks.view_all", label: "عرض جميع المهام" },
              { code: "tasks.view_team", label: "عرض مهام الفريق" },
              { code: "tasks.manage_templates", label: "إدارة القوالب" },
              { code: "tasks.export", label: "تصدير المهام" },
            ].map((perm) => (
              <div key={perm.code} className="flex items-center justify-between p-2 bg-secondary/30 border border-border/50">
                <span className="text-sm">{perm.label}</span>
                <code className="text-xs text-muted-foreground font-mono">{perm.code}</code>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
