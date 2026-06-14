using HR.Domain.Engines.Expenses;
using HR.Domain.Engines.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations.Engines;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("engine_expenses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Currency).HasMaxLength(10);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.ReceiptUrl).HasMaxLength(1000);
        builder.Property(x => x.Status).HasMaxLength(50);
        builder.HasIndex(x => new { x.TenantId, x.EmployeeId });
    }
}

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("engine_loans");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Kind).HasMaxLength(20);
        builder.Property(x => x.Status).HasMaxLength(50);
        builder.Property(x => x.Principal).HasColumnType("decimal(18,2)");
        builder.Property(x => x.MonthlyInstallment).HasColumnType("decimal(18,2)");
        builder.HasIndex(x => new { x.TenantId, x.EmployeeId });
        builder.HasMany(x => x.Installments).WithOne(x => x.Loan).HasForeignKey(x => x.LoanId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class LoanInstallmentConfiguration : IEntityTypeConfiguration<LoanInstallment>
{
    public void Configure(EntityTypeBuilder<LoanInstallment> builder)
    {
        builder.ToTable("engine_loan_installments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.HasIndex(x => x.LoanId);
    }
}
