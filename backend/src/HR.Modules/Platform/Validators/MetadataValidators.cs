using FluentValidation;
using HR.Modules.Platform.Commands.Metadata;

namespace HR.Modules.Platform.Validators;

public class CreateMetadataDefinitionValidator : AbstractValidator<CreateMetadataDefinitionCommand>
{
    public CreateMetadataDefinitionValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Module).NotEmpty().MaximumLength(100);
    }
}

public class UpdateMetadataDefinitionValidator : AbstractValidator<UpdateMetadataDefinitionCommand>
{
    public UpdateMetadataDefinitionValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
    }
}

public class AddMetadataFieldValidator : AbstractValidator<AddMetadataFieldCommand>
{
    public AddMetadataFieldValidator()
    {
        RuleFor(x => x.MetadataDefinitionId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FieldType).IsInEnum();
    }
}

public class UpdateMetadataFieldValidator : AbstractValidator<UpdateMetadataFieldCommand>
{
    public UpdateMetadataFieldValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FieldType).IsInEnum();
    }
}

public class AddMetadataOptionValidator : AbstractValidator<AddMetadataOptionCommand>
{
    public AddMetadataOptionValidator()
    {
        RuleFor(x => x.MetadataFieldId).NotEmpty();
        RuleFor(x => x.Value).NotEmpty().MaximumLength(200);
        RuleFor(x => x.LabelEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.LabelAr).NotEmpty().MaximumLength(200);
    }
}

public class UpdateMetadataOptionValidator : AbstractValidator<UpdateMetadataOptionCommand>
{
    public UpdateMetadataOptionValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Value).NotEmpty().MaximumLength(200);
        RuleFor(x => x.LabelEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.LabelAr).NotEmpty().MaximumLength(200);
    }
}
