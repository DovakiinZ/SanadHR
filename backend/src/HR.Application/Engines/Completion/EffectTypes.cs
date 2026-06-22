namespace HR.Application.Engines.Completion;

/// <summary>
/// Canonical effect-type identifiers. The "{Module}.{Action}" convention keys executors to the
/// module that owns them. Adding a future effect is just a new constant + a new executor.
/// </summary>
public static class EffectTypes
{
    // Leave
    public const string LeaveCreateApprovedLeave = "Leave.CreateApprovedLeave";

    // Attendance
    public const string AttendanceApplyLeaveDays = "Attendance.ApplyLeaveDays";
    public const string AttendanceCreatePunch = "Attendance.CreatePunch";
    public const string AttendanceCorrect = "Attendance.Correct";

    // Payroll-adjacent
    public const string ExpenseCreateClaim = "Expense.CreateClaim";
    public const string LoanCreate = "Loan.Create";
}
