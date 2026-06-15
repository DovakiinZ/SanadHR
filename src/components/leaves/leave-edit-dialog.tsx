"use client";

import { useEffect, useState } from "react";
import { Loader2 } from "lucide-react";
import { toast } from "sonner";
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { getMasterDataItems, type MasterDataItem } from "@/lib/api/master-data";
import { editLeave, type LeaveRecordRow } from "@/lib/api/leaves";

interface Props {
  row: LeaveRecordRow;
  open: boolean;
  onClose: () => void;
  onSaved: () => void;
}

const inputCls = "h-9 w-full rounded-lg border border-border bg-background px-2 text-sm";
const labelCls = "text-xs font-bold text-muted-foreground";

export function LeaveEditDialog({ row, open, onClose, onSaved }: Props) {
  const [types, setTypes] = useState<MasterDataItem[]>([]);
  const [leaveTypeId, setLeaveTypeId] = useState(row.leaveTypeId);
  const [startDate, setStartDate] = useState(row.startDate.slice(0, 10));
  const [endDate, setEndDate] = useState(row.endDate.slice(0, 10));
  const [notes, setNotes] = useState(row.notes ?? "");
  const [saving, setSaving] = useState(false);

  useEffect(() => { getMasterDataItems("LeaveType").then(setTypes).catch(() => {}); }, []);

  async function save() {
    if (!startDate || !endDate) { toast.error("حدّد تاريخ البداية والنهاية"); return; }
    if (endDate < startDate) { toast.error("تاريخ النهاية قبل البداية"); return; }
    setSaving(true);
    try {
      await editLeave(row.id, { leaveTypeId, startDate, endDate, notes });
      toast.success("تم تعديل الإجازة وإعادة احتساب الرصيد");
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
          <DialogTitle>تعديل الإجازة</DialogTitle>
          <DialogDescription>{row.recordNumber} — {row.employeeName}</DialogDescription>
        </DialogHeader>

        <div className="space-y-3">
          <div className="space-y-1">
            <label className={labelCls}>نوع الإجازة</label>
            <select value={leaveTypeId} onChange={(e) => setLeaveTypeId(e.target.value)} className={inputCls}>
              {types.map((t) => <option key={t.id} value={t.id}>{t.nameAr}</option>)}
            </select>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1">
              <label className={labelCls}>من تاريخ</label>
              <input type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} className={`${inputCls} tabular-nums`} />
            </div>
            <div className="space-y-1">
              <label className={labelCls}>إلى تاريخ</label>
              <input type="date" value={endDate} onChange={(e) => setEndDate(e.target.value)} className={`${inputCls} tabular-nums`} />
            </div>
          </div>
          <div className="space-y-1">
            <label className={labelCls}>ملاحظات</label>
            <textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={2}
              className="w-full rounded-lg border border-border bg-background px-2 py-1.5 text-sm" />
          </div>
          <p className="text-xs text-muted-foreground">سيُعاد احتساب الأيام والرصيد وتحديث سجلات الحضور تلقائياً.</p>
        </div>

        <div className="mt-2 flex items-center justify-end gap-2">
          <Button variant="outline" onClick={onClose} disabled={saving}>إلغاء</Button>
          <Button onClick={save} disabled={saving}>{saving && <Loader2 className="animate-spin" />} حفظ</Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
