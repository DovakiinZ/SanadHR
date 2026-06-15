"use client";

import { useEffect, useState } from "react";
import { Loader2, Paperclip, FileText, CalendarDays } from "lucide-react";
import {
  Sheet, SheetContent, SheetHeader, SheetTitle, SheetDescription,
} from "@/components/ui/sheet";
import { fileUrl } from "@/lib/api/files";
import {
  getLeaveDetail, LEAVE_STATUS_AR, LEAVE_STATUS_STYLE, LEAVE_SOURCE_AR,
  type LeaveRecordRow, type LeaveDetail,
} from "@/lib/api/leaves";

interface Props {
  row: LeaveRecordRow | null;
  open: boolean;
  onClose: () => void;
}

function Field({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div>
      <p className="text-[0.7rem] text-muted-foreground">{label}</p>
      <p className="mt-0.5 text-sm font-medium">{value ?? "—"}</p>
    </div>
  );
}

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <section>
      <h3 className="mb-2 text-xs font-bold text-muted-foreground">{title}</h3>
      {children}
    </section>
  );
}

export function LeaveDetailDrawer({ row, open, onClose }: Props) {
  const [detail, setDetail] = useState<LeaveDetail | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    setDetail(null);
    if (open && row) {
      setLoading(true);
      getLeaveDetail(row.id).then(setDetail).catch(() => setDetail(null)).finally(() => setLoading(false));
    }
  }, [open, row]);

  if (!row) return null;
  const r = detail?.record ?? row;
  const d = (s?: string | null) => (s ? s.slice(0, 10) : "—");

  return (
    <Sheet open={open} onOpenChange={(o) => !o && onClose()}>
      <SheetContent side="left" className="w-full overflow-y-auto p-0 sm:max-w-lg">
        <SheetHeader className="border-b p-5">
          <SheetTitle>{r.recordNumber} — {r.employeeName}</SheetTitle>
          <SheetDescription>{r.leaveTypeName} · {d(r.startDate)} ← {d(r.endDate)}</SheetDescription>
          <div className="mt-2">
            <span className={`inline-flex items-center rounded-md px-2 py-0.5 text-xs font-medium ${LEAVE_STATUS_STYLE[r.status] ?? "bg-muted"}`}>
              {LEAVE_STATUS_AR[r.status] ?? r.status}
            </span>
          </div>
        </SheetHeader>

        {loading ? (
          <div className="flex h-40 items-center justify-center"><Loader2 className="h-6 w-6 animate-spin text-muted-foreground" /></div>
        ) : (
          <div className="space-y-5 p-5">
            <Section title="معلومات الموظف">
              <div className="grid grid-cols-2 gap-3 rounded-lg border border-border bg-background p-3">
                <Field label="الموظف" value={r.employeeName} />
                <Field label="الرقم الوظيفي" value={r.employeeNumber} />
                <Field label="الإدارة" value={r.departmentName} />
                <Field label="المسمى الوظيفي" value={r.jobTitleName} />
                <Field label="الفرع" value={r.branchName} />
                <Field label="الجنسية" value={detail?.nationality} />
              </div>
            </Section>

            <Section title="معلومات الإجازة">
              <div className="grid grid-cols-2 gap-3 rounded-lg border border-border bg-background p-3">
                <Field label="نوع الإجازة" value={r.leaveTypeName} />
                <Field label="المصدر" value={LEAVE_SOURCE_AR[r.source] ?? r.source} />
                <Field label="من تاريخ" value={d(r.startDate)} />
                <Field label="إلى تاريخ" value={d(r.endDate)} />
                <Field label="عدد الأيام" value={r.daysCount} />
                <Field label="تاريخ الاعتماد" value={d(r.approvedAt)} />
                <Field label="اعتمدها" value={r.approvedByName} />
                {r.notes && <Field label="ملاحظات" value={r.notes} />}
              </div>
            </Section>

            <Section title="أثر الرصيد">
              <div className="rounded-lg border border-border bg-background p-3">
                {r.affectsBalance ? (
                  <div className="grid grid-cols-3 gap-3">
                    <Field label="الرصيد قبل" value={r.balanceBefore} />
                    <Field label="المخصوم" value={detail?.daysDeducted ?? r.daysCount} />
                    <Field label="الرصيد بعد" value={r.balanceAfter} />
                  </div>
                ) : (
                  <p className="text-sm text-muted-foreground">لا تؤثر على الرصيد</p>
                )}
              </div>
            </Section>

            <Section title="أثر الحضور">
              <div className="rounded-lg border border-border bg-background p-3 text-sm">
                {detail && detail.attendanceDays.length > 0 ? (
                  <div className="flex items-center gap-2">
                    <CalendarDays className="h-4 w-4 text-muted-foreground" />
                    {detail.attendanceDays.length} يوم مُعلّم كإجازة في الحضور
                  </div>
                ) : (
                  <span className="text-muted-foreground">لا توجد أيام حضور معلّمة</span>
                )}
                {detail?.affectsPayroll && <p className="mt-1 text-xs text-amber-600">تؤثر على الرواتب</p>}
              </div>
            </Section>

            {r.requestNumber && (
              <Section title="الطلب الأصلي">
                <div className="flex items-center gap-2 rounded-lg border border-border bg-background p-2.5 text-sm">
                  <FileText className="h-4 w-4 text-muted-foreground" />
                  {r.requestNumber}
                </div>
              </Section>
            )}

            {detail?.attachmentUrl && (
              <Section title="المرفقات">
                <a href={fileUrl(detail.attachmentUrl)} target="_blank" rel="noreferrer"
                  className="flex items-center gap-2 rounded-lg border border-border bg-background p-2.5 text-sm text-primary hover:underline">
                  <Paperclip className="h-4 w-4" /> عرض المرفق
                </a>
              </Section>
            )}

            {detail && detail.timeline.length > 0 && (
              <Section title="المسار الزمني">
                <div className="space-y-1.5">
                  {detail.timeline.map((t, i) => (
                    <div key={i} className="flex items-start gap-2 text-xs">
                      <span className="mt-1 h-1.5 w-1.5 shrink-0 rounded-full bg-primary/40" />
                      <div>
                        <span className="font-medium">{t.description ?? t.action}</span>
                        <span className="ms-1 text-muted-foreground">({new Date(t.at).toLocaleString("ar")})</span>
                      </div>
                    </div>
                  ))}
                </div>
              </Section>
            )}

            {detail && detail.audit.length > 0 && (
              <Section title="سجل التغييرات">
                <div className="space-y-1.5">
                  {detail.audit.map((a, i) => (
                    <div key={i} className="flex items-start gap-2 text-xs">
                      <span className="mt-1 h-1.5 w-1.5 shrink-0 rounded-full bg-muted-foreground/40" />
                      <div>
                        <span className="font-medium">{a.action}</span>
                        {a.description && <span className="text-muted-foreground"> — {a.description}</span>}
                        <span className="ms-1 text-muted-foreground">({new Date(a.at).toLocaleString("ar")})</span>
                      </div>
                    </div>
                  ))}
                </div>
              </Section>
            )}
          </div>
        )}
      </SheetContent>
    </Sheet>
  );
}
