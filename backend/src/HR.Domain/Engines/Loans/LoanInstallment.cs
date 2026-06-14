using HR.Domain.Common;

namespace HR.Domain.Engines.Loans;

/// <summary>One monthly installment of a <see cref="Loan"/> (the payroll deduction schedule).</summary>
public class LoanInstallment : BaseEntity
{
    public Guid LoanId { get; set; }
    public DateTime DueMonth { get; set; }   // first day of the due month
    public decimal Amount { get; set; }
    public bool Paid { get; set; }

    public Loan Loan { get; set; } = null!;
}
