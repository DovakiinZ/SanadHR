import { Badge } from "@/components/ui/badge";
import { TransactionStatus, TRANSACTION_STATUS_AR } from "@/lib/api/payroll-transactions";

const STYLES: Record<TransactionStatus, string> = {
  0: "bg-zinc-500/10 text-zinc-400 border-zinc-500/20",     // Draft
  1: "bg-amber-500/10 text-amber-500 border-amber-500/20",  // PendingApproval
  2: "bg-green-500/10 text-green-500 border-green-500/20",   // Approved
  3: "bg-red-500/10 text-red-500 border-red-500/20",         // Rejected
  4: "bg-zinc-500/10 text-zinc-400 border-zinc-500/20",      // Cancelled
  5: "bg-blue-500/10 text-blue-500 border-blue-500/20",      // CarriedForward
  6: "bg-violet-500/10 text-violet-500 border-violet-500/20",// Posted
  7: "bg-orange-500/10 text-orange-500 border-orange-500/20",// Reversed
};

export function TransactionStatusBadge({ status }: { status: TransactionStatus }) {
  return (
    <Badge variant="outline" className={`text-xs ${STYLES[status]}`}>
      {TRANSACTION_STATUS_AR[status]}
    </Badge>
  );
}
