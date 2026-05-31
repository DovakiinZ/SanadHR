using FluentValidation;
using HR.Modules.Platform.Commands.OrgGraph;

namespace HR.Modules.Platform.Validators;

public class CreateOrgNodeValidator : AbstractValidator<CreateOrgNodeCommand>
{
    public CreateOrgNodeValidator()
    {
        RuleFor(x => x.NodeType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
    }
}

public class UpdateOrgNodeValidator : AbstractValidator<UpdateOrgNodeCommand>
{
    public UpdateOrgNodeValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
    }
}

public class CreateOrgEdgeValidator : AbstractValidator<CreateOrgEdgeCommand>
{
    public CreateOrgEdgeValidator()
    {
        RuleFor(x => x.SourceNodeId).NotEmpty();
        RuleFor(x => x.TargetNodeId).NotEmpty();
        RuleFor(x => x.RelationType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SourceNodeId)
            .NotEqual(x => x.TargetNodeId)
            .WithMessage("Source and target nodes must be different.");
    }
}

public class CreateOrgGraphLayoutValidator : AbstractValidator<CreateOrgGraphLayoutCommand>
{
    public CreateOrgGraphLayoutValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.GraphType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LayoutData).NotEmpty();
    }
}

public class UpdateOrgGraphLayoutValidator : AbstractValidator<UpdateOrgGraphLayoutCommand>
{
    public UpdateOrgGraphLayoutValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.LayoutData).NotEmpty();
    }
}

public class CreateEmployeeReportingLineValidator : AbstractValidator<CreateEmployeeReportingLineCommand>
{
    public CreateEmployeeReportingLineValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.ManagerId).NotEmpty();
        RuleFor(x => x.ReportingType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EffectiveFrom).NotEmpty();
        RuleFor(x => x.EmployeeId)
            .NotEqual(x => x.ManagerId)
            .WithMessage("Employee cannot report to themselves.");
    }
}

public class UpdateEmployeeReportingLineValidator : AbstractValidator<UpdateEmployeeReportingLineCommand>
{
    public UpdateEmployeeReportingLineValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ManagerId).NotEmpty();
        RuleFor(x => x.ReportingType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EffectiveFrom).NotEmpty();
    }
}
