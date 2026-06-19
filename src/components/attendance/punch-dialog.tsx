"use client";

import { useState } from "react";
import { Loader2 } from "lucide-react";
import { toast } from "sonner";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { addManualPunch, correctAttendance, fmtTime, type AttendanceDay } from "@/lib/api/attendance";

interface Props {
  mode: "manual" | "correct";
  row: AttendanceDay;
  open: boolean;
  onClose: () => void;
  onSaved: () => void;
}

function toHm(s?: string | null): string {
  if (!s) return "";
  const d = new Date(s);
  return isNaN(d.getTime()) ? "" : d.toLocaleTimeString("en-GB", { hour: "2-digit", minute: "2-digit" });
}

export function PunchDialog({ mode, row, open, onClose, onSaved }: Props) {
  const [checkIn, setCheckIn] = useState(toHm(row.checkIn));
  const [checkOut, setCheckOut] = useState(toHm(row.checkOut));
  const [reason, setReason] = useState("");
  const [saving, setSaving] = useState(false);

  const isCorrect = mode === "correct";

  async function save() {
    if (!checkIn && !checkOut) {
      toast.error("أدخل وقت الحضور أو الانصراف");
      return;
    }
    setSaving(true);
    try {
      if (isCorrect && row.recordId) {
        await correctAttendance(row.recordId, { checkIn, checkOut, reason });
        toast.success("تم تصحيح الحضور");
      } else {
        await addManualPunch({
          employeeId: row.employeeId,
          date: row.date.slice(0, 10),
          checkIn,
          checkOut,
          notes: reason,
        });
        toast.success("تم تسجيل البصمة");
      }
      onSaved();
      onClose();
    } catch (e) {
      toast.error((e as Error)?.message || "تعذر الحفظ");
    } finally {
      setSaving(false);
    }
  }

  return (
    <Dialog open={open} onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{isCorrect ? "تصحيح الحضور" : "تسجيل بصمة يدوية"}</DialogTitle>
          <DialogDescription>
            {row.employeeName} — {row.date.slice(0, 10)}
            {row.shiftName ? ` · ${row.shiftName}` : ""}
          </DialogDescription>
        </DialogHeader>

        <div className="grid grid-cols-2 gap-3">
          <div className="space-y-1">
            <label className="text-xs font-bold text-muted-foreground">وقت الحضور</label>
            <input
              type="time"
              value={checkIn}
              onChange={(e) => setCheckIn(e.target.value)}
              className="h-9 w-full rounded-lg border border-border bg-background px-2 text-sm tabular-nums"
            />
          </div>
          <div className="space-y-1">
            <label className="text-xs font-bold text-muted-foreground">وقت الانصراف</label>
            <input
              type="time"
              value={checkOut}
              onChange={(e) => setCheckOut(e.target.value)}
              className="h-9 w-full rounded-lg border border-border bg-background px-2 text-sm tabular-nums"
            />
          </div>
        </div>

        {isCorrect && (
          <p className="text-xs text-muted-foreground">
            الحالي: حضور {fmtTime(row.checkIn)} · انصراف {fmtTime(row.checkOut)}
          </p>
        )}

        <div className="space-y-1">
          <label className="text-xs font-bold text-muted-foreground">{isCorrect ? "سبب التصحيح" : "ملاحظات"}</label>
          <textarea
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            rows={2}
            className="w-full rounded-lg border border-border bg-background px-2 py-1.5 text-sm"
          />
        </div>

        <div className="mt-1 flex items-center justify-end gap-2">
          <Button variant="outline" onClick={onClose} disabled={saving}>
            إلغاء
          </Button>
          <Button onClick={save} disabled={saving}>
            {saving && <Loader2 className="animate-spin" />} حفظ
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
