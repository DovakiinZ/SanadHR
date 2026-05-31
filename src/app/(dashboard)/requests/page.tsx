import { FileText } from "lucide-react";

export default function RequestsPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">الطلبات</h1>
        <p className="text-sm text-muted-foreground mt-1">إدارة الطلبات والموافقات</p>
      </div>
      <div className="border border-border bg-card p-12 flex flex-col items-center justify-center text-center">
        <FileText className="h-12 w-12 text-muted-foreground mb-4" />
        <h2 className="text-lg font-semibold mb-2">قريباً</h2>
        <p className="text-sm text-muted-foreground">سيتم إطلاق نظام الطلبات قريباً</p>
      </div>
    </div>
  );
}
