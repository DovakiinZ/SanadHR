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
        builder.Property(x => x.City).HasMaxLength(100);
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.TenantId, x.EmployeeNumber }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
        builder.HasIndex(x => x.JobTitleId);
        builder.HasIndex(x => x.NationalityId);
        builder.HasIndex(x => x.ContractTypeId);
    }
}
