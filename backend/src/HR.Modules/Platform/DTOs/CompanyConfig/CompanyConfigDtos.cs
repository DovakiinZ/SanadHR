namespace HR.Modules.Platform.DTOs.CompanyConfig;

public record CompanyProfileDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? LogoUrl { get; init; }
    public string? StampUrl { get; init; }
    public string? HrSignatureUrl { get; init; }
    public string? CeoSignatureUrl { get; init; }
    public string? CommercialRegistration { get; init; }
    public string? VatNumber { get; init; }
    public string? Website { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public string? PostalCode { get; init; }
    public string? NationalAddress { get; init; }
    public string? ContactInfo { get; init; }
    public string? FiscalYearStart { get; init; }
    public string? DefaultCurrency { get; init; }
    public string? DefaultLanguage { get; init; }
    public string? TimeZone { get; init; }
    public string? MolNumber { get; init; }
    public string? GosiNumber { get; init; }
    public decimal GosiRate { get; init; }
}

public record PositionDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public Guid? DepartmentId { get; init; }
    public Guid? ParentPositionId { get; init; }
    public string? JobDescription { get; init; }
    public int? MinGrade { get; init; }
    public int? MaxGrade { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
    public List<PositionDto> ChildPositions { get; init; } = new();
}

public record GradeDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public int Level { get; init; }
    public decimal? MinSalary { get; init; }
    public decimal? MaxSalary { get; init; }
    public string? Benefits { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
}

public record CostCenterDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public Guid? ParentCostCenterId { get; init; }
    public Guid? DepartmentId { get; init; }
    public Guid? BranchId { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
    public List<CostCenterDto> ChildCostCenters { get; init; } = new();
}

public record CalendarSettingDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string CalendarType { get; init; } = null!;
    public string WorkWeekDays { get; init; } = null!;
    public string? Holidays { get; init; }
    public TimeSpan WorkDayStart { get; init; }
    public TimeSpan WorkDayEnd { get; init; }
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; }
}

public record FiscalPeriodDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public int Year { get; init; }
    public int PeriodNumber { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsClosed { get; init; }
    public DateTime? ClosedAt { get; init; }
}
