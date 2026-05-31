using FluentValidation;
using HR.Modules.Platform.Commands.Documents;

namespace HR.Modules.Platform.Validators;

public class CreateDocumentTemplateValidator : AbstractValidator<CreateDocumentTemplateCommand>
{
    public CreateDocumentTemplateValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Module).NotEmpty().MaximumLength(100);
        RuleFor(x => x.OutputFormat).IsInEnum();
        RuleFor(x => x.BodyTemplate).NotEmpty();
    }
}

public class UpdateDocumentTemplateValidator : AbstractValidator<UpdateDocumentTemplateCommand>
{
    public UpdateDocumentTemplateValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BodyTemplate).NotEmpty();
    }
}

public class GenerateDocumentValidator : AbstractValidator<GenerateDocumentCommand>
{
    public GenerateDocumentValidator()
    {
        RuleFor(x => x.DocumentTemplateId).NotEmpty();
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EntityId).NotEmpty();
    }
}

public class AddDocumentTokenValidator : AbstractValidator<AddDocumentTokenCommand>
{
    public AddDocumentTokenValidator()
    {
        RuleFor(x => x.DocumentTemplateId).NotEmpty();
        RuleFor(x => x.TokenCode).NotEmpty().MaximumLength(200);
    }
}

public class SaveCompanyBrandingValidator : AbstractValidator<SaveCompanyBrandingCommand>
{
    public SaveCompanyBrandingValidator()
    {
        RuleFor(x => x.ElementType).IsInEnum();
    }
}
