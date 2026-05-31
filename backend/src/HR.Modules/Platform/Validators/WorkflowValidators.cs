using FluentValidation;
using HR.Modules.Platform.Commands.Workflows;

namespace HR.Modules.Platform.Validators;

public class CreateWorkflowDefinitionValidator : AbstractValidator<CreateWorkflowDefinitionCommand>
{
    public CreateWorkflowDefinitionValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TriggerEntityType).NotEmpty().MaximumLength(200);
    }
}

public class UpdateWorkflowDefinitionValidator : AbstractValidator<UpdateWorkflowDefinitionCommand>
{
    public UpdateWorkflowDefinitionValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TriggerEntityType).NotEmpty().MaximumLength(200);
    }
}

public class AddWorkflowNodeValidator : AbstractValidator<AddWorkflowNodeCommand>
{
    public AddWorkflowNodeValidator()
    {
        RuleFor(x => x.WorkflowVersionId).NotEmpty();
        RuleFor(x => x.NodeType).IsInEnum();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
    }
}

public class UpdateWorkflowNodeValidator : AbstractValidator<UpdateWorkflowNodeCommand>
{
    public UpdateWorkflowNodeValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NodeType).IsInEnum();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
    }
}

public class AddWorkflowEdgeValidator : AbstractValidator<AddWorkflowEdgeCommand>
{
    public AddWorkflowEdgeValidator()
    {
        RuleFor(x => x.WorkflowVersionId).NotEmpty();
        RuleFor(x => x.SourceNodeId).NotEmpty();
        RuleFor(x => x.TargetNodeId).NotEmpty();
        RuleFor(x => x.SourceNodeId).NotEqual(x => x.TargetNodeId)
            .WithMessage("Source and target nodes must be different");
    }
}

public class AddWorkflowConditionValidator : AbstractValidator<AddWorkflowConditionCommand>
{
    public AddWorkflowConditionValidator()
    {
        RuleFor(x => x.WorkflowNodeId).NotEmpty();
        RuleFor(x => x.Field).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Operator).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Value).NotEmpty();
    }
}

public class SetWorkflowApproverRuleValidator : AbstractValidator<SetWorkflowApproverRuleCommand>
{
    public SetWorkflowApproverRuleValidator()
    {
        RuleFor(x => x.WorkflowNodeId).NotEmpty();
        RuleFor(x => x.ApproverType).IsInEnum();
    }
}
