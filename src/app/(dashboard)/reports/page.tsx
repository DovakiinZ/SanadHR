import { BarChart3 } from "lucide-react";

export default function ReportsPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">التقارير</h1>
        <p className="text-sm text-muted-foreground mt-1">التقارير والإحصائيات</p>
      </div>
      <div className="border border-border bg-card p-12 flex flex-col items-center justify-center text-center">
        <BarChart3 className="h-12 w-12 text-muted-foreground mb-4" />
        <h2 className="text-lg font-semibold mb-2">قريباً</h2>
        <p className="text-sm text-muted-foreground">سيتم إطلاق نظام التقارير قريباً</p>
      </div>
    </div>
  );
}
