using FluentValidation;
using HR.Modules.Platform.Commands.Dashboards;

namespace HR.Modules.Platform.Validators;

public class CreateDashboardDefinitionValidator : AbstractValidator<CreateDashboardDefinitionCommand>
{
    public CreateDashboardDefinitionValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
    }
}
