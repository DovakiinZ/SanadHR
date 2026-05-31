using HR.Domain.Engines.CompanyConfig;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations.Engines;

public class CompanyProfileConfiguration : IEntityTypeConfiguration<CompanyProfile>
{
    public void Configure(EntityTypeBuilder<CompanyProfile> builder)
    {
        builder.ToTable("engine_company_profiles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.LogoUrl).HasMaxLength(1000);
        builder.Property(x => x.StampUrl).HasMaxLength(1000);
        builder.Property(x => x.CommercialRegistration).HasMaxLength(100);
        builder.Property(x => x.VatNumber).HasMaxLength(100);
        builder.Property(x => x.NationalAddress).HasColumnType("jsonb");
        builder.Property(x => x.ContactInfo).HasColumnType("jsonb");
        builder.Property(x => x.FiscalYearStart).HasMaxLength(10);
        builder.Property(x => x.DefaultCurrency).HasMaxLength(10);
        builder.Property(x => x.DefaultLanguage).HasMaxLength(10);
        builder.Property(x => x.TimeZone).HasMaxLength(100);
        builder.HasIndex(x => x.TenantId).IsUnique();
    }
}

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("engine_positions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.JobDescription).HasMaxLength(2000);
        builder.HasOne(x => x.ParentPosition).WithMany(x => x.ChildPositions).HasForeignKey(x => x.ParentPositionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}

public class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> builder)
    {
        builder.ToTable("engine_grades");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.MinSalary).HasPrecision(18, 2);
        builder.Property(x => x.MaxSalary).HasPrecision(18, 2);
        builder.Property(x => x.Benefits).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}

public class CostCenterConfiguration : IEntityTypeConfiguration<CostCenter>
{
    public void Configure(EntityTypeBuilder<CostCenter> builder)
    {
        builder.ToTable("engine_cost_centers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.HasOne(x => x.ParentCostCenter).WithMany(x => x.ChildCostCenters).HasForeignKey(x => x.ParentCostCenterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}

public class CalendarSettingConfiguration : IEntityTypeConfiguration<CalendarSetting>
{
    public void Configure(EntityTypeBuilder<CalendarSetting> builder)
    {
        builder.ToTable("engine_calendar_settings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CalendarType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.WorkWeekDays).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.Holidays).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}

public class FiscalPeriodConfiguration : IEntityTypeConfiguration<FiscalPeriod>
{
    public void Configure(EntityTypeBuilder<FiscalPeriod> builder)
    {
        builder.ToTable("engine_fiscal_periods");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Year, x.PeriodNumber }).IsUnique();
    }
}
