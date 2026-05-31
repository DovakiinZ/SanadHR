using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Forms;

public class FormSubmission : TenantEntity
{
    public Guid FormDefinitionId { get; set; }
    public Guid SubmittedById { get; set; }
    public DateTime SubmittedAt { get; set; }
    public FormSubmissionStatus Status { get; set; } = FormSubmissionStatus.Draft;

    public FormDefinition FormDefinition { get; set; } = null!;
    public ICollection<FormSubmissionValue> Values { get; set; } = new List<FormSubmissionValue>();
}
