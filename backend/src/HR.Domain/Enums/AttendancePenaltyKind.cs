namespace HR.Domain.Enums;

/// <summary>Business semantics for an attendance-driven deduction. The engine keys on this enum; the
/// customer-configurable DeductionType master-data item (codes ABSENCE/LATE/SHORTAGE) supplies labels only.</summary>
public enum AttendancePenaltyKind
{
    Absence = 1,
    Late = 2,
    Shortage = 3,
}
