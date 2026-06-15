"use client";

import { useEffect, useState } from "react";
import { Loader2, LogIn, LogOut, FileText } from "lucide-react";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetDescription,
} from "@/components/ui/sheet";
import {
  ATTENDANCE_STATUS_AR,
  ATTENDANCE_STATUS_STYLE,
  ATTENDANCE_SOURCE_AR,
  fmtMinutes,
  fmtTime,
  getAttendanceDetail,
  type AttendanceDay,
  type AttendanceDetail,
} from "@/lib/api/attendance";

interface Props {
  row: AttendanceDay | null;
  open: boolean;
  onClose: () => void;
}

function StatusChip({ status }: { status: string }) {
  return (
    <span className={`inline-flex items-center rounded-md px-2 py-0.5 text-xs font-medium ${ATTENDANCE_STATUS_STYLE[status] ?? "bg-muted text-muted-foreground"}`}>
      {ATTENDANCE_STATUS_AR[status] ?? status}
    </span>
  );
}

function Metric({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border border-border bg-background p-2.5">
      <p className="text-[0.7rem] text-muted-foreground">{label}</p>
      <p className="mt-0.5 text-sm font-bold tabular-nums">{value}</p>
    </div>
  );
}

export function AttendanceDetailDrawer({ row, open, onClose }: Props) {
  const [detail, setDetail] = useState<AttendanceDetail | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    setDetail(null);
    if (open && row?.recordId) {
      setLoading(true);
      getAttendanceDetail(row.recordId)
        .then(setDetail)
        .catch(() => setDetail(null))
        .finally(() => setLoading(false));
    }
  }, [open, row?.recordId]);

  if (!row) return null;
  const day = detail?.day ?? row;

  return (
    <Sheet open={open} onOpenChange={(o) => !o && onClose()}>
      <SheetContent side="left" className="w-full overflow-y-auto p-0 sm:max-w-lg">
        <SheetHeader className="border-b p-5">
          <SheetTitle>{day.employeeName ?? "—"}</SheetTitle>
          <SheetDescription>
            {day.employeeNumber} · {day.departmentName ?? "—"} · {day.branchName ?? "—"}
          </SheetDescription>
          <div className="mt-2 flex items-center gap-2">
            <StatusChip status={day.status} />
            <span className="text-xs text-muted-foreground">{day.date.slice(0, 10)}</span>
          </div>
        </SheetHeader>

        <div className="space-y-5 p-5">
          {/* Shift */}
          <section>
            <h3 className="mb-2 text-xs font-bold text-muted-foreground">الوردية</h3>
            <div className="rounded-lg border border-border bg-background p-3 text-sm">
              {day.shiftName ? (
                <div className="flex items-center justify-between">
                  <span>{day.shiftName}</span>
                  <span className="text-xs text-muted-foreground">
                    {day.isFlexible ? "مرنة" : "ثابتة"} · مطلوب {fmtMinutes(day.requiredMinutes)}
                  </span>
                </div>
              ) : (
                <span className="text-muted-foreground">لا توجد وردية معيّنة</span>
              )}
            </div>
          </section>

          {/* Calculation breakdown */}
          <section>
            <h3 className="mb-2 text-xs font-bold text-muted-foreground">تفصيل الاحتساب</h3>
            <div className="grid grid-cols-3 gap-2">
              <Metric label="الحضور" value={fmtTime(day.checkIn)} />
              <Metric label="الانصراف" value={fmtTime(day.checkOut)} />
              <Metric label="ساعات العمل" value={fmtMinutes(day.workedMinutes)} />
              <Metric label="المطلوبة" value={fmtMinutes(day.requiredMinutes)} />
              <Metric label="المتبقي" value={fmtMinutes(day.shortageMinutes)} />
              <Metric label="الاستراحة" value={fmtMinutes(day.breakMinutes)} />
              <Metric label="التأخير" value={fmtMinutes(day.lateMinutes)} />
              <Metric label="النقص" value={fmtMinutes(day.shortageMinutes)} />
              <Metric label="الإضافي" value={fmtMinutes(day.overtimeMinutes)} />
            </div>
          </section>

          {/* Punch timeline */}
          <section>
            <h3 className="mb-2 text-xs font-bold text-muted-foreground">سجل البصمات</h3>
            {loading ? (
              <div className="flex justify-center py-4"><Loader2 className="h-5 w-5 animate-spin text-muted-foreground" /></div>
            ) : detail && detail.punches.length > 0 ? (
              <div className="space-y-1.5">
                {detail.punches.map((p) => (
                  <div key={p.id} className="flex items-center gap-2 rounded-lg border border-border bg-background p-2 text-sm">
                    {p.direction === "In" ? <LogIn className="h-4 w-4 text-green-600" /> : <LogOut className="h-4 w-4 text-red-600" />}
                    <span className="tabular-nums">{fmtTime(p.punchTime)}</span>
                    <span className="text-xs text-muted-foreground">{p.direction === "In" ? "حضور" : "انصراف"}</span>
                    <span className="ms-auto text-xs text-muted-foreground">{ATTENDANCE_SOURCE_AR[p.source] ?? p.source}</span>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-xs text-muted-foreground">لا توجد بصمات مسجلة لهذا اليوم</p>
            )}
          </section>

          {/* Related request */}
          {detail?.relatedRequestNumber && (
            <section>
              <h3 className="mb-2 text-xs font-bold text-muted-foreground">الطلب المرتبط</h3>
              <div className="flex items-center gap-2 rounded-lg border border-border bg-background p-2.5 text-sm">
                <FileText className="h-4 w-4 text-muted-foreground" />
                {detail.relatedRequestNumber}
                {day.source && <span className="ms-auto text-xs text-muted-foreground">{ATTENDANCE_SOURCE_AR[day.source] ?? day.source}</span>}
              </div>
            </section>
          )}

          {day.notes && (
            <section>
              <h3 className="mb-2 text-xs font-bold text-muted-foreground">ملاحظات</h3>
              <p className="rounded-lg border border-border bg-background p-2.5 text-sm">{day.notes}</p>
            </section>
          )}

          {/* Audit log */}
          {detail && detail.audit.length > 0 && (
            <section>
              <h3 className="mb-2 text-xs font-bold text-muted-foreground">سجل التغييرات</h3>
              <div className="space-y-1.5">
                {detail.audit.map((a, i) => (
                  <div key={i} className="flex items-start gap-2 text-xs">
                    <span className="mt-1 h-1.5 w-1.5 shrink-0 rounded-full bg-muted-foreground/40" />
                    <div>
                      <span className="font-medium">{a.action}</span>
                      {a.details && <span className="text-muted-foreground"> — {a.details}</span>}
                      <span className="ms-1 text-muted-foreground">({new Date(a.at).toLocaleString("ar")})</span>
                    </div>
                  </div>
                ))}
              </div>
            </section>
          )}
        </div>
      </SheetContent>
    </Sheet>
  );
}
