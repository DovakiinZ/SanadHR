namespace HR.Modules.Attendance.DTOs;

public sealed class ShiftDto
{
    public Guid Id { get; set; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string StartTime { get; set; } = "08:00";  // HH:mm
    public string EndTime { get; set; } = "17:00";
    public int RequiredMinutes { get; set; }
    public int BreakMinutes { get; set; }
    public int GraceBeforeStartMinutes { get; set; }
    public int GraceAfterStartMinutes { get; set; }
    public int GraceBeforeEndMinutes { get; set; }
    public int GraceAfterEndMinutes { get; set; }
    public bool OvertimeAllowed { get; set; }
    public bool LateDeductionEnabled { get; set; }
    public bool IsFlexible { get; set; }
    public string WeekendDays { get; set; } = "5,6";
    public bool IsActive { get; set; } = true;
    public int AssignedCount { get; set; }
}

/// <summary>Create/update payload for a shift (times as "HH:mm").</summary>
public sealed class ShiftInput
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string StartTime { get; set; } = "08:00";
    public string EndTime { get; set; } = "17:00";
    public int RequiredMinutes { get; set; } = 480;
    public int BreakMinutes { get; set; }
    public int GraceBeforeStartMinutes { get; set; }
    public int GraceAfterStartMinutes { get; set; }
    public int GraceBeforeEndMinutes { get; set; }
    public int GraceAfterEndMinutes { get; set; }
    public bool OvertimeAllowed { get; set; }
    public bool LateDeductionEnabled { get; set; }
    public bool IsFlexible { get; set; }
    public string WeekendDays { get; set; } = "5,6";
    public bool IsActive { get; set; } = true;
}

public sealed class ShiftAssignmentDto
{
    public Guid Id { get; set; }
    public Guid ShiftId { get; set; }
    public string? ShiftName { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public Guid? JobTitleId { get; set; }
    public string? JobTitleName { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>Assign a shift to one or many targets at once. Any combination of employee ids /
/// department / branch / job-title produces one assignment row per target.</summary>
public sealed class AssignShiftRequest
{
    public Guid ShiftId { get; set; }
    public List<Guid> EmployeeIds { get; set; } = new();
    public Guid? DepartmentId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? JobTitleId { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
}
