namespace HR.Modules.Platform.DTOs.Leaves;

public sealed class LeaveRecordListDto
{
    public Guid Id { get; set; }
    public string RecordNumber { get; set; } = null!;
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeNumber { get; set; }
    public string? DepartmentName { get; set; }
    public string? BranchName { get; set; }
    public string? JobTitleName { get; set; }
    public Guid LeaveTypeId { get; set; }
    public string? LeaveTypeName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal DaysCount { get; set; }
    public bool AffectsBalance { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Status { get; set; } = null!;
    public string Source { get; set; } = null!;
    public Guid? RequestInstanceId { get; set; }
    public string? RequestNumber { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByName { get; set; }
    public string? Notes { get; set; }
    public bool HasAttachment { get; set; }
    public DateTime? CanceledAt { get; set; }
}

public sealed class LeaveTimelineDto
{
    public string Action { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime At { get; set; }
}

public sealed class LeaveDetailDto
{
    public LeaveRecordListDto Record { get; set; } = new();

    // Employee info
    public string? Nationality { get; set; }
    public string? NationalId { get; set; }

    // Impacts
    public decimal DaysDeducted { get; set; }
    public List<DateTime> AttendanceDays { get; set; } = new();
    public bool AffectsPayroll { get; set; }
    public string? AttachmentUrl { get; set; }

    public List<LeaveTimelineDto> Audit { get; set; } = new();
    public List<LeaveTimelineDto> Timeline { get; set; } = new();
}

public sealed class LeaveBalanceDto
{
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = null!;
    public bool AffectsBalance { get; set; }
    public decimal Entitled { get; set; }
    public decimal Used { get; set; }
    public decimal Remaining { get; set; }
}

// ── Write payloads ──

public sealed class AssignLeaveRequest
{
    public Guid LeaveTypeId { get; set; }
    public string Scope { get; set; } = "Employees"; // Employees | Department | Branch | JobTitle
    public List<Guid> EmployeeIds { get; set; } = new();
    public Guid? DepartmentId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? JobTitleId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentUrl { get; set; }
}

public sealed class EditLeaveRequest
{
    public Guid? LeaveTypeId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentUrl { get; set; }
}

public sealed class CancelLeaveRequest
{
    public string? Reason { get; set; }
}

public sealed class LeaveFilter
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public Guid? EmployeeId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? LeaveTypeId { get; set; }
    public string? Status { get; set; }
    public string? Source { get; set; }
    public bool Mine { get; set; }
    public Guid? MyEmployeeId { get; set; }
}
