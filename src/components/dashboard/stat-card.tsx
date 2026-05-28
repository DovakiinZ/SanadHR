import { type LucideIcon } from "lucide-react";

interface StatCardProps {
  title: string;
  value: string | number;
  icon: LucideIcon;
  change?: string;
  changeType?: "positive" | "negative" | "neutral";
}

export function StatCard({
  title,
  value,
  icon: Icon,
  change,
  changeType = "neutral",
}: StatCardProps) {
  return (
    <div className="border border-border bg-card p-5">
      <div className="flex items-center justify-between">
        <p className="text-xs font-bold uppercase tracking-wider text-muted-foreground">
          {title}
        </p>
        <div className="flex h-9 w-9 items-center justify-center bg-primary/10">
          <Icon className="h-5 w-5 text-primary" />
        </div>
      </div>
      <p className="mt-3 text-3xl font-bold">{value}</p>
      {change && (
        <p
          className={`mt-1 text-xs ${
            changeType === "positive"
              ? "text-green-500"
              : changeType === "negative"
              ? "text-red-500"
              : "text-muted-foreground"
          }`}
        >
          {change}
        </p>
      )}
    </div>
  );
}
