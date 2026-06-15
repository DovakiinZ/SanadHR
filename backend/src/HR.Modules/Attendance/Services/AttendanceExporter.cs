using ClosedXML.Excel;
using HR.Modules.Attendance.DTOs;

namespace HR.Modules.Attendance.Services;

/// <summary>Builds .xlsx exports for the attendance views (detail rows or per-employee summary).</summary>
public static class AttendanceExporter
{
    private static string Hours(int minutes) => $"{minutes / 60}:{minutes % 60:D2}";
    private static string Time(DateTime? t) => t?.ToString("HH:mm") ?? "—";

    public static byte[] ExportRows(IReadOnlyList<AttendanceDayDto> rows)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Attendance");
        ws.RightToLeft = true;

        var headers = new[]
        {
            "الرقم الوظيفي", "الموظف", "الإدارة", "الفرع", "التاريخ", "الوردية",
            "الحضور", "الانصراف", "ساعات العمل", "المطلوبة", "التأخير (د)", "النقص (د)", "الإضافي (د)",
            "الحالة", "المصدر",
        };
        for (int c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(1, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            var row = i + 2;
            ws.Cell(row, 1).Value = r.EmployeeNumber;
            ws.Cell(row, 2).Value = r.EmployeeName;
            ws.Cell(row, 3).Value = r.DepartmentName;
            ws.Cell(row, 4).Value = r.BranchName;
            ws.Cell(row, 5).Value = r.Date.ToString("yyyy-MM-dd");
            ws.Cell(row, 6).Value = r.ShiftName;
            ws.Cell(row, 7).Value = Time(r.CheckIn);
            ws.Cell(row, 8).Value = Time(r.CheckOut);
            ws.Cell(row, 9).Value = Hours(r.WorkedMinutes);
            ws.Cell(row, 10).Value = Hours(r.RequiredMinutes);
            ws.Cell(row, 11).Value = r.LateMinutes;
            ws.Cell(row, 12).Value = r.ShortageMinutes;
            ws.Cell(row, 13).Value = r.OvertimeMinutes;
            ws.Cell(row, 14).Value = r.Status;
            ws.Cell(row, 15).Value = r.Source;
        }
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public static byte[] ExportSummary(IReadOnlyList<AttendanceSummaryDto> rows)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Summary");
        ws.RightToLeft = true;

        var headers = new[]
        {
            "الرقم الوظيفي", "الموظف", "الإدارة", "الفرع",
            "أيام الحضور", "أيام الغياب", "أيام الإجازة", "أيام التأخير", "نقص الساعات", "الإضافي",
            "ساعات العمل", "الساعات المطلوبة", "إجمالي التأخير (د)", "إجمالي الإضافي (د)",
        };
        for (int c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(1, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            var row = i + 2;
            ws.Cell(row, 1).Value = r.EmployeeNumber;
            ws.Cell(row, 2).Value = r.EmployeeName;
            ws.Cell(row, 3).Value = r.DepartmentName;
            ws.Cell(row, 4).Value = r.BranchName;
            ws.Cell(row, 5).Value = r.PresentDays;
            ws.Cell(row, 6).Value = r.AbsentDays;
            ws.Cell(row, 7).Value = r.LeaveDays;
            ws.Cell(row, 8).Value = r.LateDays;
            ws.Cell(row, 9).Value = r.ShortDays;
            ws.Cell(row, 10).Value = r.OvertimeDays;
            ws.Cell(row, 11).Value = Hours(r.WorkedMinutes);
            ws.Cell(row, 12).Value = Hours(r.RequiredMinutes);
            ws.Cell(row, 13).Value = r.LateMinutes;
            ws.Cell(row, 14).Value = r.OvertimeMinutes;
        }
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
