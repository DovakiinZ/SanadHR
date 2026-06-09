using HR.Domain.Common;

namespace HR.Domain.Engines.Files;

/// <summary>
/// A binary file persisted in the database (employee photos, attachments…). Served
/// through an unguessable capability URL (/api/files/{id}) so it can be rendered in
/// &lt;img&gt; tags without a bearer header. Not a TenantEntity — reads are anonymous by
/// capability — but carries TenantId for scoping/cleanup.
/// </summary>
public class StoredFile : AuditableEntity
{
    public Guid TenantId { get; set; }
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public long SizeBytes { get; set; }
    public string? Category { get; set; }
}
