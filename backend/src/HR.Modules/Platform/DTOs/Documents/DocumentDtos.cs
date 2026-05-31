namespace HR.Modules.Platform.DTOs.Documents;

public class DocumentTemplateDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Description { get; set; }
    public string Module { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string OutputFormat { get; set; } = null!;
    public string BodyTemplate { get; set; } = null!;
    public string? HeaderTemplate { get; set; }
    public string? FooterTemplate { get; set; }
    public string? StyleSheet { get; set; }
    public bool UseBranding { get; set; }
    public string? PageSettings { get; set; }
    public int Version { get; set; }
    public List<DocumentTemplateTokenDto> Tokens { get; set; } = new();
}

public class DocumentTemplateTokenDto
{
    public Guid Id { get; set; }
    public string TokenCode { get; set; } = null!;
    public string? DefaultValue { get; set; }
    public bool IsRequired { get; set; }
}

public class DocumentTemplateVersionDto
{
    public Guid Id { get; set; }
    public int VersionNumber { get; set; }
    public string BodyTemplate { get; set; } = null!;
    public string? ChangeNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class GeneratedDocumentDto
{
    public Guid Id { get; set; }
    public Guid DocumentTemplateId { get; set; }
    public string EntityType { get; set; } = null!;
    public Guid EntityId { get; set; }
    public string Status { get; set; } = null!;
    public string OutputFormat { get; set; } = null!;
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public long? FileSizeBytes { get; set; }
    public DateTime? GeneratedAt { get; set; }
}

public class CompanyBrandingDto
{
    public Guid Id { get; set; }
    public string ElementType { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public string? Content { get; set; }
    public string? Configuration { get; set; }
    public bool IsActive { get; set; }
}
