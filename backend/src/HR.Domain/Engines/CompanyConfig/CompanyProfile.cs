using HR.Domain.Common;

namespace HR.Domain.Engines.CompanyConfig;

public class CompanyProfile : TenantEntity
{
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public string? StampUrl { get; set; }
    public string? CommercialRegistration { get; set; }
    public string? VatNumber { get; set; }
    public string? NationalAddress { get; set; } // JSONB
    public string? ContactInfo { get; set; } // JSONB - phones, emails, website
    public string? FiscalYearStart { get; set; } // MM-DD
    public string? DefaultCurrency { get; set; }
    public string? DefaultLanguage { get; set; }
    public string? TimeZone { get; set; }
}
