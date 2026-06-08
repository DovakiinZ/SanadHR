import { Badge } from "@/components/ui/badge";

const statusConfig: Record<string, { className: string }> = {
  // Backend statuses (EmployeeMappingProfile)
  "نشط": { className: "bg-green-500/10 text-green-500 border-green-500/20" },
  "في إجازة": { className: "bg-yellow-500/10 text-yellow-500 border-yellow-500/20" },
  "موقوف": { className: "bg-orange-500/10 text-orange-500 border-orange-500/20" },
  "منتهي": { className: "bg-red-500/10 text-red-500 border-red-500/20" },
  "مستقيل": { className: "bg-zinc-500/10 text-zinc-400 border-zinc-500/20" },
  // Legacy mock values (kept for backward compatibility)
  "إجازة": { className: "bg-yellow-500/10 text-yellow-500 border-yellow-500/20" },
  "منتهي العقد": { className: "bg-red-500/10 text-red-500 border-red-500/20" },
};

export function StatusBadge({ status }: { status: string }) {
  const config = statusConfig[status as keyof typeof statusConfig] || statusConfig["نشط"];
  return (
    <Badge variant="outline" className={`text-xs font-medium ${config.className}`}>
      {status}
    </Badge>
  );
}
