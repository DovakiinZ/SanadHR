using FluentValidation;
using HR.Modules.Platform.Commands.Automation;

namespace HR.Modules.Platform.Validators;

public class CreateAutomationRuleValidator : AbstractValidator<CreateAutomationRuleCommand>
{
    public CreateAutomationRuleValidator()
    {
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
    }
}
