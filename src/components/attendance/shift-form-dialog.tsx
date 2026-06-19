"use client";

import { useState } from "react";
import { Loader2 } from "lucide-react";
import { toast } from "sonner";
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import {
  createShift, updateShift, WEEKDAYS_AR,
  type Shift, type ShiftInput,
} from "@/lib/api/shifts";

interface Props {
  shift: Shift | null;
  open: boolean;
  onClose: () => void;
  onSaved: () => void;
}

function defaults(s: Shift | null): ShiftInput {
  return {
    nameAr: s?.nameAr ?? "",
    nameEn: s?.nameEn ?? "",
    startTime: s?.startTime ?? "08:00",
    endTime: s?.endTime ?? "17:00",
    requiredMinutes: s?.requiredMinutes ?? 480,
    breakMinutes: s?.breakMinutes ?? 0,
    graceBeforeStartMinutes: s?.graceBeforeStartMinutes ?? 0,
    graceAfterStartMinutes: s?.graceAfterStartMinutes ?? 0,
    graceBeforeEndMinutes: s?.graceBeforeEndMinutes ?? 0,
    graceAfterEndMinutes: s?.graceAfterEndMinutes ?? 0,
    overtimeAllowed: s?.overtimeAllowed ?? false,
    lateDeductionEnabled: s?.lateDeductionEnabled ?? false,
    isFlexible: s?.isFlexible ?? false,
    weekendDays: s?.weekendDays ?? "5,6",
    isActive: s?.isActive ?? true,
  };
}

const inputCls = "h-9 w-full rounded-lg border border-border bg-background px-2 text-sm";
const labelCls = "text-xs font-bold text-muted-foreground";

export function ShiftFormDialog({ shift, open, onClose, onSaved }: Props) {
  const [form, setForm] = useState<ShiftInput>(() => defaults(shift));
  const [saving, setSaving] = useState(false);

  function set<K extends keyof ShiftInput>(key: K, value: ShiftInput[K]) {
    setForm((p) => ({ ...p, [key]: value }));
  }

  const weekendSet = new Set(form.weekendDays.split(",").map((s) => s.trim()).filter(Boolean));
  function toggleWeekend(day: number) {
    const k = String(day);
    const next = new Set(weekendSet);
    if (next.has(k)) next.delete(k); else next.add(k);
    set("weekendDays", Array.from(next).sort().join(","));
  }

  // Required hours shown as a friendly decimal; stored as minutes.
  const requiredHours = (form.requiredMinutes / 60).toString();

  async function save() {
    if (!form.nameAr.trim()) { toast.error("أدخل اسم الوردية بالعربية"); return; }
    const payload: ShiftInput = { ...form, nameEn: form.nameEn.trim() || form.nameAr.trim() };
    setSaving(true);
    try {
      if (shift) await updateShift(shift.id, payload);
      else await createShift(payload);
      toast.success(shift ? "تم تحديث الوردية" : "تم إنشاء الوردية");
      onSaved();
    } catch (e) { toast.error((e as Error)?.message || "تعذر الحفظ"); }
    finally { setSaving(false); }
  }

  return (
    <Dialog open={open} onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-2xl">
        <DialogHeader>
          <DialogTitle>{shift ? "تعديل الوردية" : "وردية جديدة"}</DialogTitle>
          <DialogDescription>حدّد أوقات الدوام وقواعد التأخير والإضافي ونهاية الأسبوع</DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1">
              <label className={labelCls}>اسم الوردية (عربي)</label>
              <input value={form.nameAr} onChange={(e) => set("nameAr", e.target.value)} className={inputCls} />
            </div>
            <div className="space-y-1">
              <label className={labelCls}>اسم الوردية (إنجليزي)</label>
              <input value={form.nameEn} onChange={(e) => set("nameEn", e.target.value)} className={inputCls} dir="ltr" />
            </div>
          </div>

          {/* Flexible toggle */}
          <label className="flex items-center justify-between rounded-lg border border-border p-3">
            <div>
              <span className="text-sm font-medium">وردية مرنة</span>
              <p className="text-xs text-muted-foreground">يمكن للموظف الحضور في أي وقت — المهم إكمال الساعات المطلوبة فقط</p>
            </div>
            <input type="checkbox" checked={form.isFlexible} onChange={(e) => set("isFlexible", e.target.checked)} className="h-4 w-4" />
          </label>

          {!form.isFlexible && (
            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-1">
                <label className={labelCls}>وقت البداية</label>
                <input type="time" value={form.startTime} onChange={(e) => set("startTime", e.target.value)} className={`${inputCls} tabular-nums`} />
              </div>
              <div className="space-y-1">
                <label className={labelCls}>وقت النهاية</label>
                <input type="time" value={form.endTime} onChange={(e) => set("endTime", e.target.value)} className={`${inputCls} tabular-nums`} />
              </div>
            </div>
          )}

          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1">
              <label className={labelCls}>الساعات المطلوبة</label>
              <input
                type="number" step="0.5" min="0" value={requiredHours}
                onChange={(e) => set("requiredMinutes", Math.round((Number(e.target.value) || 0) * 60))}
                className={inputCls}
              />
            </div>
            <div className="space-y-1">
              <label className={labelCls}>مدة الاستراحة (دقائق)</label>
              <input type="number" min="0" value={form.breakMinutes} onChange={(e) => set("breakMinutes", Number(e.target.value) || 0)} className={inputCls} />
            </div>
          </div>

          {!form.isFlexible && (
            <div>
              <p className="mb-2 text-xs font-bold text-muted-foreground">فترات السماح (دقائق)</p>
              <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
                <div className="space-y-1">
                  <label className="text-[0.7rem] text-muted-foreground">قبل البداية</label>
                  <input type="number" min="0" value={form.graceBeforeStartMinutes} onChange={(e) => set("graceBeforeStartMinutes", Number(e.target.value) || 0)} className={inputCls} />
                </div>
                <div className="space-y-1">
                  <label className="text-[0.7rem] text-muted-foreground">بعد البداية</label>
                  <input type="number" min="0" value={form.graceAfterStartMinutes} onChange={(e) => set("graceAfterStartMinutes", Number(e.target.value) || 0)} className={inputCls} />
                </div>
                <div className="space-y-1">
                  <label className="text-[0.7rem] text-muted-foreground">قبل النهاية</label>
                  <input type="number" min="0" value={form.graceBeforeEndMinutes} onChange={(e) => set("graceBeforeEndMinutes", Number(e.target.value) || 0)} className={inputCls} />
                </div>
                <div className="space-y-1">
                  <label className="text-[0.7rem] text-muted-foreground">بعد النهاية</label>
                  <input type="number" min="0" value={form.graceAfterEndMinutes} onChange={(e) => set("graceAfterEndMinutes", Number(e.target.value) || 0)} className={inputCls} />
                </div>
              </div>
            </div>
          )}

          <div>
            <p className="mb-2 text-xs font-bold text-muted-foreground">أيام نهاية الأسبوع</p>
            <div className="flex flex-wrap gap-1.5">
              {WEEKDAYS_AR.map((name, i) => (
                <button
                  key={i}
                  onClick={() => toggleWeekend(i)}
                  className={`rounded-md border px-2.5 py-1 text-xs ${weekendSet.has(String(i)) ? "border-primary bg-primary/10 text-primary" : "border-border text-muted-foreground"}`}
                >
                  {name}
                </button>
              ))}
            </div>
          </div>

          <div className="grid grid-cols-1 gap-2 sm:grid-cols-3">
            <label className="flex items-center gap-2 rounded-lg border border-border p-2.5 text-sm">
              <input type="checkbox" checked={form.overtimeAllowed} onChange={(e) => set("overtimeAllowed", e.target.checked)} className="h-4 w-4" />
              السماح بالوقت الإضافي
            </label>
            <label className="flex items-center gap-2 rounded-lg border border-border p-2.5 text-sm">
              <input type="checkbox" checked={form.lateDeductionEnabled} onChange={(e) => set("lateDeductionEnabled", e.target.checked)} className="h-4 w-4" />
              خصم التأخير
            </label>
            <label className="flex items-center gap-2 rounded-lg border border-border p-2.5 text-sm">
              <input type="checkbox" checked={form.isActive} onChange={(e) => set("isActive", e.target.checked)} className="h-4 w-4" />
              نشطة
            </label>
          </div>
        </div>

        <div className="mt-2 flex items-center justify-end gap-2">
          <Button variant="outline" onClick={onClose} disabled={saving}>إلغاء</Button>
          <Button onClick={save} disabled={saving}>{saving && <Loader2 className="animate-spin" />} حفظ</Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
