namespace HR.Domain.Enums;

public enum DocumentTemplateStatus
{
    Draft = 1,
    Published = 2,
    Archived = 3
}

public enum DocumentGenerationStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4
}

public enum DocumentOutputFormat
{
    Pdf = 1,
    Docx = 2,
    Html = 3
}

public enum BrandingElementType
{
    Logo = 1,
    Stamp = 2,
    Header = 3,
    Footer = 4,
    Watermark = 5
}
