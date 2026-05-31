using HR.Domain.Common;

namespace HR.Domain.Engines.Documents;

public class DocumentWorkflowLink : BaseEntity
{
    public Guid DocumentTemplateId { get; set; }
    public string TriggerType { get; set; } = null!; // RequestType, WorkflowAction, Automation
    public Guid? TriggerEntityId { get; set; }
    public string? TriggerCondition { get; set; } // JSONB
    public bool AutoGenerate { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public DocumentTemplate DocumentTemplate { get; set; } = null!;
}
