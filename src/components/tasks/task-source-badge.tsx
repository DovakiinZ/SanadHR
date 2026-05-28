"use client";

import { Badge } from "@/components/ui/badge";
import { TaskSource } from "@/types";
import {
  Workflow,
  FileText,
  FileWarning,
  Banknote,
  Clock,
  UserPlus,
  UserMinus,
  Users,
  Bot,
  PenLine,
} from "lucide-react";

const sourceIcons: Record<TaskSource, React.ElementType> = {
  "يدوي": PenLine,
  "سير العمل": Workflow,
  "طلب": FileText,
  "انتهاء مستند": FileWarning,
  "الرواتب": Banknote,
  "الحضور": Clock,
  "تهيئة موظف": UserPlus,
  "إنهاء خدمات": UserMinus,
  "التوظيف": Users,
  "أتمتة النظام": Bot,
};

export function TaskSourceBadge({ source }: { source: TaskSource }) {
  const Icon = sourceIcons[source] || FileText;
  return (
    <Badge variant="outline" className="text-xs font-medium border-border text-muted-foreground gap-1">
      <Icon className="h-3 w-3" />
      {source}
    </Badge>
  );
}
