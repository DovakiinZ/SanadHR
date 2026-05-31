using FluentValidation;
using HR.Modules.Platform.Commands.ObjectRegistry;

namespace HR.Modules.Platform.Validators;

public class CreateObjectDefinitionValidator : AbstractValidator<CreateObjectDefinitionCommand>
{
    public CreateObjectDefinitionValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Module).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TableName).NotEmpty().MaximumLength(200);
    }
}

public class UpdateObjectDefinitionValidator : AbstractValidator<UpdateObjectDefinitionCommand>
{
    public UpdateObjectDefinitionValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Module).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TableName).NotEmpty().MaximumLength(200);
    }
}

public class AddObjectFieldValidator : AbstractValidator<AddObjectFieldCommand>
{
    public AddObjectFieldValidator()
    {
        RuleFor(x => x.ObjectDefinitionId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FieldType).IsInEnum();
    }
}

public class AddObjectRelationshipValidator : AbstractValidator<AddObjectRelationshipCommand>
{
    public AddObjectRelationshipValidator()
    {
        RuleFor(x => x.SourceObjectId).NotEmpty();
        RuleFor(x => x.TargetObjectId).NotEmpty();
        RuleFor(x => x.RelationType).IsInEnum();
        RuleFor(x => x.ForeignKeyField).NotEmpty().MaximumLength(200);
    }
}
