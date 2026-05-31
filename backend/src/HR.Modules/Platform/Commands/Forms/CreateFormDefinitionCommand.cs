using HR.Modules.Platform.DTOs.Forms;
using MediatR;

namespace HR.Modules.Platform.Commands.Forms;

public record CreateFormDefinitionCommand : IRequest<FormDefinitionDto>
{
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? Description { get; init; }
    public string Module { get; init; } = null!;
}

public record UpdateFormDefinitionCommand : IRequest<FormDefinitionDto>
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? Description { get; init; }
    public bool IsPublished { get; init; }
    public bool IsActive { get; init; }
}

public record SubmitFormCommand : IRequest<FormSubmissionDto>
{
    public Guid FormDefinitionId { get; init; }
    public List<FormSubmissionValueInput> Values { get; init; } = new();
}

public class FormSubmissionValueInput
{
    public Guid FormFieldId { get; set; }
    public string FieldCode { get; set; } = null!;
    public string? Value { get; set; }
    public string? FileUrl { get; set; }
}
