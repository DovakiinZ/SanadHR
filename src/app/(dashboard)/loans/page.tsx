import { HandCoins } from "lucide-react";

export default function LoansPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">السلف</h1>
        <p className="text-sm text-muted-foreground mt-1">إدارة طلبات السلف والقروض</p>
      </div>
      <div className="border border-border bg-card p-12 flex flex-col items-center justify-center text-center">
        <HandCoins className="h-12 w-12 text-muted-foreground mb-4" />
        <h2 className="text-lg font-semibold mb-2">قريباً</h2>
        <p className="text-sm text-muted-foreground">سيتم إطلاق نظام السلف قريباً</p>
      </div>
    </div>
  );
}
