using FluentValidation;
using HR.Modules.Platform.Commands.Forms;

namespace HR.Modules.Platform.Validators;

public class CreateFormDefinitionValidator : AbstractValidator<CreateFormDefinitionCommand>
{
    public CreateFormDefinitionValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Module).NotEmpty().MaximumLength(100);
    }
}

public class UpdateFormDefinitionValidator : AbstractValidator<UpdateFormDefinitionCommand>
{
    public UpdateFormDefinitionValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
    }
}

public class AddFormFieldValidator : AbstractValidator<AddFormFieldCommand>
{
    public AddFormFieldValidator()
    {
        RuleFor(x => x.FormDefinitionId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FieldType).IsInEnum();
    }
}

public class UpdateFormFieldValidator : AbstractValidator<UpdateFormFieldCommand>
{
    public UpdateFormFieldValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FieldType).IsInEnum();
    }
}

public class CloneFormValidator : AbstractValidator<CloneFormCommand>
{
    public CloneFormValidator()
    {
        RuleFor(x => x.SourceFormId).NotEmpty();
        RuleFor(x => x.NewCode).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
    }
}

public class UpdateFormSubmissionStatusValidator : AbstractValidator<UpdateFormSubmissionStatusCommand>
{
    public UpdateFormSubmissionStatusValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}
