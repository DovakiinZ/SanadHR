using HR.Domain.Enums;

namespace HR.Modules.Payroll.DTOs;

public sealed class CreateTransactionRequest
{
    public PayrollTransactionKind Kind { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid TypeId { get; set; }
    public decimal Amount { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? TransactionDate { get; set; }
    public bool IsRecurring { get; set; }
    public DateTime? RecurrenceEndDate { get; set; }
    public string? Notes { get; set; }
    public Guid? AttachmentFileId { get; set; }
    public bool SubmitImmediately { get; set; }
}

public sealed class UpdateTransactionRequest
{
    public Guid TypeId { get; set; }
    public decimal Amount { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? TransactionDate { get; set; }
    public bool IsRecurring { get; set; }
    public DateTime? RecurrenceEndDate { get; set; }
    public string? Notes { get; set; }
    public Guid? AttachmentFileId { get; set; }
}

public sealed class RejectTransactionRequest { public string Reason { get; set; } = ""; }
public sealed class CancelTransactionRequest { public string? Reason { get; set; } }
public sealed class SetAttachmentRequest { public Guid FileId { get; set; } }

public sealed record ReverseTransactionRequest(string Reason, bool CreateCorrection, decimal? CorrectedAmount);

public sealed record TransactionImpactDto(int PeriodYear, int PeriodMonth, int CutoffDay, bool CarriedAfterCutoff);
