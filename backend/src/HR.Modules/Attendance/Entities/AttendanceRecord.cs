using HR.Domain.Common;

namespace HR.Modules.Attendance.Entities;

// TODO: Implement attendance record entity
public class AttendanceRecord : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
}
