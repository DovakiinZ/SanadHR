using FluentValidation;
using HR.Modules.Platform.Commands.Permissions;

namespace HR.Modules.Platform.Validators;

public class CreatePermissionTemplateValidator : AbstractValidator<CreatePermissionTemplateCommand>
{
    public CreatePermissionTemplateValidator()
    {
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
    }
}

public class UpdatePermissionTemplateValidator : AbstractValidator<UpdatePermissionTemplateCommand>
{
    public UpdatePermissionTemplateValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
    }
}

public class AddPermissionTemplateItemValidator : AbstractValidator<AddPermissionTemplateItemCommand>
{
    public AddPermissionTemplateItemValidator()
    {
        RuleFor(x => x.PermissionTemplateId).NotEmpty();
        RuleFor(x => x.PermissionCode).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Scope).IsInEnum();
    }
}

public class AssignPermissionTemplateValidator : AbstractValidator<AssignPermissionTemplateCommand>
{
    public AssignPermissionTemplateValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PermissionTemplateId).NotEmpty();
    }
}

public class SetUserPermissionOverrideValidator : AbstractValidator<SetUserPermissionOverrideCommand>
{
    public SetUserPermissionOverrideValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PermissionCode).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Scope).IsInEnum();
    }
}
