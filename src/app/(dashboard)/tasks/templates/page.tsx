"use client";

import Link from "next/link";
import { ArrowRight, Plus } from "lucide-react";
import { mockTemplates } from "@/lib/tasks-mock-data";
import { TaskTemplateList } from "@/components/tasks/task-template-list";

export default function TemplatesPage() {
  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <div className="flex items-center gap-2 text-sm text-muted-foreground mb-2">
            <Link href="/tasks" className="hover:text-primary transition-colors">المهام</Link>
            <ArrowRight className="h-3 w-3 rotate-180" />
            <span>القوالب</span>
          </div>
          <h1 className="text-2xl font-bold">قوالب المهام</h1>
          <p className="text-sm text-muted-foreground mt-1">إدارة قوالب المهام المتكررة والأتمتة</p>
        </div>
        <button className="inline-flex items-center gap-2 h-10 px-4 bg-primary text-primary-foreground font-bold uppercase tracking-wider text-sm hover:bg-primary/80 transition-colors">
          <Plus className="h-4 w-4" />
          قالب جديد
        </button>
      </div>

      <TaskTemplateList templates={mockTemplates} />
    </div>
  );
}
