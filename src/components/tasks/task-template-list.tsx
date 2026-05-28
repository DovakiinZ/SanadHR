"use client";

import { TaskTemplate } from "@/types";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  FileText,
  Play,
  ChevronDown,
  ChevronUp,
  ListChecks,
  Calendar,
  User,
} from "lucide-react";
import { useState } from "react";

interface TaskTemplateListProps {
  templates: TaskTemplate[];
}

export function TaskTemplateList({ templates }: TaskTemplateListProps) {
  const [expanded, setExpanded] = useState<string | null>(null);

  return (
    <div className="space-y-3">
      {templates.map((template) => (
        <div key={template.id} className="bg-card border border-border">
          {/* Template header */}
          <div
            className="p-4 flex items-center justify-between cursor-pointer hover:bg-secondary/30 transition-colors"
            onClick={() => setExpanded(expanded === template.id ? null : template.id)}
          >
            <div className="flex items-center gap-3">
              <div className="h-10 w-10 bg-primary/10 flex items-center justify-center">
                <FileText className="h-5 w-5 text-primary" />
              </div>
              <div>
                <h3 className="text-sm font-medium">{template.name}</h3>
                <p className="text-xs text-muted-foreground">{template.description}</p>
              </div>
            </div>
            <div className="flex items-center gap-3">
              <Badge variant="outline" className="text-xs border-border text-muted-foreground">
                {template.relatedModule}
              </Badge>
              <span className="text-xs text-muted-foreground">{template.items.length} مهام</span>
              {expanded === template.id ? (
                <ChevronUp className="h-4 w-4 text-muted-foreground" />
              ) : (
                <ChevronDown className="h-4 w-4 text-muted-foreground" />
              )}
            </div>
          </div>

          {/* Expanded details */}
          {expanded === template.id && (
            <div className="px-4 pb-4 border-t border-border pt-3">
              {template.automationTrigger && (
                <div className="mb-3 flex items-center gap-2 text-xs text-muted-foreground">
                  <Play className="h-3 w-3" />
                  <span>التشغيل التلقائي: {template.automationTrigger}</span>
                </div>
              )}

              <div className="space-y-2">
                {template.items.map((item, index) => (
                  <div key={item.id} className="p-3 bg-secondary/40 border border-border/50">
                    <div className="flex items-center justify-between mb-2">
                      <div className="flex items-center gap-2">
                        <span className="text-xs text-muted-foreground font-mono">
                          {String(index + 1).padStart(2, "0")}
                        </span>
                        <span className="text-sm font-medium">{item.title}</span>
                      </div>
                      <Badge variant="outline" className="text-xs">
                        {item.priority}
                      </Badge>
                    </div>
                    <div className="flex items-center gap-4 text-xs text-muted-foreground">
                      <span className="flex items-center gap-1">
                        <User className="h-3 w-3" />
                        {item.defaultAssignee}
                      </span>
                      <span className="flex items-center gap-1">
                        <Calendar className="h-3 w-3" />
                        {item.relativeDueDays > 0 ? `+${item.relativeDueDays}` : item.relativeDueDays} يوم
                      </span>
                      {item.checklist.length > 0 && (
                        <span className="flex items-center gap-1">
                          <ListChecks className="h-3 w-3" />
                          {item.checklist.length} عنصر
                        </span>
                      )}
                    </div>
                    {item.checklist.length > 0 && (
                      <div className="mt-2 flex flex-wrap gap-1">
                        {item.checklist.map((cl, ci) => (
                          <span key={ci} className="text-xs bg-secondary px-2 py-0.5 text-muted-foreground">
                            {cl}
                          </span>
                        ))}
                      </div>
                    )}
                  </div>
                ))}
              </div>

              <div className="mt-3 flex justify-end">
                <Button size="sm" className="h-8 text-xs gap-1">
                  <Play className="h-3 w-3" />
                  تشغيل القالب
                </Button>
              </div>
            </div>
          )}
        </div>
      ))}
    </div>
  );
}
