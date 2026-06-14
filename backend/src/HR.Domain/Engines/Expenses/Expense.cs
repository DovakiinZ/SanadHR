using HR.Domain.Common;

namespace HR.Domain.Engines.Expenses;

/// <summary>
/// An expense record. Created when an Expense Claim request is approved (the request is the
/// entry point; this is the tracked record shown on the Expenses page). Category references the
/// ExpenseCategory master-data item (governed, no free text).
/// </summary>
public class Expense : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public Guid? ExpenseCategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR";
    public string? Description { get; set; }
    public string? ReceiptUrl { get; set; }
    public string Status { get; set; } = "Approved";   // Approved | Paid | Rejected
    public Guid? RequestInstanceId { get; set; }
    public DateTime DecidedAt { get; set; } = DateTime.UtcNow;
}
