using HR.Domain.Common;

namespace HR.Domain.Engines.Documents;

/// <summary>
/// Reusable document "chrome" — header / footer / margins / logo placement / watermark / QR /
/// signature area. A <see cref="DocumentTemplate"/> inherits a page template so body content
/// stays separate from presentation. Config blocks are stored as JSONB and interpreted by the
/// renderer. System presets (Modern Corporate, Formal Letter, …) ship out of the box.
/// </summary>
public class PageTemplate : TenantEntity
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Description { get; set; }

    /// <summary>JSONB: { showLogo, logoPlacement (Left|Center|Right), showIdentity, showCrVat, customText }.</summary>
    public string? HeaderConfig { get; set; }

    /// <summary>JSONB: { showQr, qrPlacement (Left|Center|Right), showStamp, showSignatures, showGeneratedDate, customText }.</summary>
    public string? FooterConfig { get; set; }

    /// <summary>JSONB: { top, right, bottom, left } in points.</summary>
    public string? Margins { get; set; }

    /// <summary>JSONB: { text, imageFileId, opacity }.</summary>
    public string? Watermark { get; set; }

    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
