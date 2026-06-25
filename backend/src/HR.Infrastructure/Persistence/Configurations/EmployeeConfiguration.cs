using HR.Modules.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EmployeeNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FirstNameAr).HasMaxLength(100);
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastNameAr).HasMaxLength(100);
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(20);
        builder.Property(x => x.NationalId).HasMaxLength(20);
        builder.Property(x => x.BasicSalary).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Currency).HasMaxLength(10);
        builder.Property(x => x.BankName).HasMaxLength(200);
        builder.Property(x => x.BankAccountNumber).HasMaxLength(50);
        builder.Property(x => x.Iban).HasMaxLength(50);
        builder.Property(x => x.SalaryCardNumber).HasMaxLength(50);
        builder.Property(x => x.CardProvider).HasMaxLength(100);
        builder.Property(x => x.EmergencyContactName).HasMaxLength(150);
        builder.Property(x => x.EmergencyContactPhone).HasMaxLength(20);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.ContractTermType).HasDefaultValue(HR.Domain.Enums.ContractTermType.Indefinite);
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.TenantId, x.EmployeeNumber }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
        builder.HasIndex(x => x.JobTitleId);
        builder.HasIndex(x => x.NationalityId);
        builder.HasIndex(x => x.ContractTypeId);

        builder.HasMany(x => x.Allowances)
            .WithOne(a => a.Employee)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeAllowanceConfiguration : IEntityTypeConfiguration<EmployeeAllowance>
{
    public void Configure(EntityTypeBuilder<EmployeeAllowance> builder)
    {
        builder.ToTable("employee_allowances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.EmployeeId, x.AllowanceTypeId }).IsUnique();
    }
}

public class EmployeeAdditionConfiguration : IEntityTypeConfiguration<EmployeeAddition>
{
    public void Configure(EntityTypeBuilder<EmployeeAddition> builder)
    {
        builder.ToTable("employee_additions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.EmployeeId, x.AdditionTypeId }).IsUnique();
        builder.HasOne(x => x.Employee).WithMany(e => e.Additions).HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeDeductionConfiguration : IEntityTypeConfiguration<EmployeeDeduction>
{
    public void Configure(EntityTypeBuilder<EmployeeDeduction> builder)
    {
        builder.ToTable("employee_deductions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.EmployeeId, x.DeductionTypeId }).IsUnique();
        builder.HasOne(x => x.Employee).WithMany(e => e.Deductions).HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class TerminationSettlementConfiguration : IEntityTypeConfiguration<TerminationSettlement>
{
    public void Configure(EntityTypeBuilder<TerminationSettlement> builder)
    {
        builder.ToTable("employee_termination_settlements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MonthlyWage).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DailyWage).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ServiceYears).HasColumnType("decimal(10,4)");
        builder.Property(x => x.EffectiveServiceDays).HasColumnType("decimal(10,2)");
        builder.Property(x => x.UnpaidLeaveDays).HasColumnType("decimal(10,2)");
        builder.Property(x => x.GratuityAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Article77Award).HasColumnType("decimal(18,2)");
        builder.Property(x => x.NoticeCompensation).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalAward).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Currency).HasMaxLength(10);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.TenantId, x.EmployeeId });
        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(i => i.TerminationSettlementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TerminationSettlementItemConfiguration : IEntityTypeConfiguration<TerminationSettlementItem>
{
    public void Configure(EntityTypeBuilder<TerminationSettlementItem> builder)
    {
        builder.ToTable("employee_termination_settlement_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LabelEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.LabelAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ArticleRef).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.TerminationSettlementId);
    }
}
