using HR.Domain.Common;

namespace HR.Modules.Documents.Entities;

// TODO: Implement document entity
public class Document : TenantEntity
{
    public string FileName { get; set; } = null!;
    public string FileKey { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long FileSize { get; set; }
}
