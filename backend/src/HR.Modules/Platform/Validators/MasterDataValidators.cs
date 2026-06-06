using FluentValidation;
using HR.Domain.Engines.MasterData;
using HR.Modules.Platform.Commands.MasterData;

namespace HR.Modules.Platform.Validators;

public class CreateMasterDataItemValidator : AbstractValidator<CreateMasterDataItemCommand>
{
    public CreateMasterDataItemValidator()
    {
        RuleFor(x => x.ObjectType)
            .NotEmpty()
            .Must(t => MasterDataObjectType.Normalize(t) is not null)
            .WithMessage("Unknown master data object type.");
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Color).MaximumLength(20);
        RuleFor(x => x.Icon).MaximumLength(80);
    }
}

public class UpdateMasterDataItemValidator : AbstractValidator<UpdateMasterDataItemCommand>
{
    public UpdateMasterDataItemValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Color).MaximumLength(20);
        RuleFor(x => x.Icon).MaximumLength(80);
    }
}

public class ReorderMasterDataItemsValidator : AbstractValidator<ReorderMasterDataItemsCommand>
{
    public ReorderMasterDataItemsValidator()
    {
        RuleFor(x => x.ObjectType)
            .NotEmpty()
            .Must(t => MasterDataObjectType.Normalize(t) is not null)
            .WithMessage("Unknown master data object type.");
        RuleFor(x => x.OrderedIds).NotEmpty();
    }
}

public class MergeMasterDataItemsValidator : AbstractValidator<MergeMasterDataItemsCommand>
{
    public MergeMasterDataItemsValidator()
    {
        RuleFor(x => x.SourceId).NotEmpty();
        RuleFor(x => x.TargetId).NotEmpty().NotEqual(x => x.SourceId)
            .WithMessage("Target must differ from source.");
    }
}
