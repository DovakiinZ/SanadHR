using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Requests;

/// <summary>
/// Assigns a document template to a request type for a given lifecycle trigger. A request type
/// may have MANY mappings (e.g. Leave Approval Letter @ FinalApproval + Payroll Form @ Completed).
/// System mappings (shipped defaults) cannot be deleted — only duplicated / replaced.
/// </summary>
public class RequestTemplateMapping : TenantEntity
{
    public Guid RequestTypeId { get; set; }
    public Guid DocumentTemplateId { get; set; }
    public DocumentTriggerEvent TriggerEvent { get; set; } = DocumentTriggerEvent.FinalApproval;
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public RequestType RequestType { get; set; } = null!;
}
