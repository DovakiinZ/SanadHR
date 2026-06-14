"use client";

import { useEffect, useState } from "react";
import { Clock, Loader2 } from "lucide-react";
import { AttendanceRecord, ATTENDANCE_STATUS_AR, getAttendance } from "@/lib/api/attendance";

export default function AttendancePage() {
  const [rows, setRows] = useState<AttendanceRecord[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => { getAttendance({ scope: "all", year: new Date().getFullYear() }).then(setRows).catch(() => setRows([])).finally(() => setLoading(false)); }, []);
  const time = (s?: string | null) => (s ? new Date(s).toLocaleTimeString("ar", { hour: "2-digit", minute: "2-digit" }) : "—");

  return (
    <div className="space-y-5">
      <div>
        <h1 className="text-2xl font-bold">الحضور والانصراف</h1>
        <p className="mt-1 text-sm text-muted-foreground">سجلات الحضور — تُحدّث تلقائياً عند اعتماد الإجازات والبصمات الناقصة وتصحيح الحضور</p>
      </div>

      {loading ? <div className="flex h-64 items-center justify-center"><Loader2 className="h-6 w-6 animate-spin text-muted-foreground" /></div> : rows.length === 0 ? (
        <div className="flex flex-col items-center justify-center border border-dashed border-border p-12 text-center"><Clock className="mb-3 h-10 w-10 text-muted-foreground" /><p className="text-sm text-muted-foreground">لا توجد سجلات حضور بعد</p></div>
      ) : (
        <div className="overflow-x-auto border border-border bg-card">
          <table className="w-full text-sm">
            <thead><tr className="border-b border-border text-right text-xs text-muted-foreground">
              <th className="px-4 py-2 font-medium">الموظف</th><th className="px-4 py-2 font-medium">التاريخ</th><th className="px-4 py-2 font-medium">الحالة</th>
              <th className="px-4 py-2 font-medium">الحضور</th><th className="px-4 py-2 font-medium">الانصراف</th><th className="px-4 py-2 font-medium">المصدر</th>
            </tr></thead>
            <tbody>
              {rows.map((r) => (
                <tr key={r.id} className="border-b border-border/40 hover:bg-muted/30">
                  <td className="px-4 py-2">{r.employeeName ?? "—"}</td>
                  <td className="px-4 py-2 text-muted-foreground">{r.date?.slice(0, 10)}</td>
                  <td className="px-4 py-2">{ATTENDANCE_STATUS_AR[r.status] ?? r.status}</td>
                  <td className="px-4 py-2 tabular-nums">{time(r.checkIn)}</td>
                  <td className="px-4 py-2 tabular-nums">{time(r.checkOut)}</td>
                  <td className="px-4 py-2 text-xs text-muted-foreground">{r.source ?? "—"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
