using FluentValidation;
using HR.Modules.Platform.Commands.Dashboards;

namespace HR.Modules.Platform.Validators;

public class CreateDashboardCategoryValidator : AbstractValidator<CreateDashboardCategoryCommand>
{
    public CreateDashboardCategoryValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
    }
}

public class CreateDashboardTemplateValidator : AbstractValidator<CreateDashboardTemplateCommand>
{
    public CreateDashboardTemplateValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.LayoutConfiguration).NotEmpty();
        RuleFor(x => x.WidgetConfiguration).NotEmpty();
    }
}

public class CreateDashboardValidator : AbstractValidator<CreateDashboardCommand>
{
    public CreateDashboardValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Scope).IsInEnum();
    }
}

public class UpdateDashboardValidator : AbstractValidator<UpdateDashboardCommand>
{
    public UpdateDashboardValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Scope).IsInEnum();
    }
}

public class CloneDashboardValidator : AbstractValidator<CloneDashboardCommand>
{
    public CloneDashboardValidator()
    {
        RuleFor(x => x.SourceDashboardId).NotEmpty();
        RuleFor(x => x.NewCode).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
    }
}

public class AddDashboardWidgetValidator : AbstractValidator<AddDashboardWidgetCommand>
{
    public AddDashboardWidgetValidator()
    {
        RuleFor(x => x.DashboardDefinitionId).NotEmpty();
        RuleFor(x => x.WidgetType).IsInEnum();
        RuleFor(x => x.TitleEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TitleAr).NotEmpty().MaximumLength(200);
    }
}

public class CreateWidgetDefinitionValidator : AbstractValidator<CreateWidgetDefinitionCommand>
{
    public CreateWidgetDefinitionValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.WidgetType).IsInEnum();
    }
}
