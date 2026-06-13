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

    // Canonical contact + address (used by documents/reports/printing — single source).
    public string? Website { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }

    public string? NationalAddress { get; set; } // JSONB (legacy structured address)
    public string? ContactInfo { get; set; } // JSONB (legacy structured contact)
    public string? FiscalYearStart { get; set; } // MM-DD
    public string? DefaultCurrency { get; set; }
    public string? DefaultLanguage { get; set; }
    public string? TimeZone { get; set; }
}
