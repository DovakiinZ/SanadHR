namespace HR.Domain.Enums;

/// <summary>Business semantics for an attendance-driven payroll impact. The engine keys on this enum;
/// customer-configurable master-data items (DeductionType ABSENCE/LATE/SHORTAGE, AdditionType OVERTIME)
/// supply labels only.</summary>
public enum AttendancePayrollKind
{
    Absence = 1,
    Late = 2,
    Shortage = 3,
    Overtime = 4,
}
