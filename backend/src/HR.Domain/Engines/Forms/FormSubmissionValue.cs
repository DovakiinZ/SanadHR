using HR.Domain.Common;

namespace HR.Domain.Engines.Forms;

public class FormSubmissionValue : BaseEntity
{
    public Guid FormSubmissionId { get; set; }
    public Guid FormFieldId { get; set; }
    public string FieldCode { get; set; } = null!;
    public string? Value { get; set; }
    public string? FileUrl { get; set; }

    public FormSubmission FormSubmission { get; set; } = null!;
    public FormField FormField { get; set; } = null!;
}
