import { Badge } from "@/components/ui/badge";

const statusConfig = {
  "نشط": { className: "bg-green-500/10 text-green-500 border-green-500/20" },
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
