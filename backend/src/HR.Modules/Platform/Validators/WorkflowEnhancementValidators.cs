using FluentValidation;
using HR.Modules.Platform.Commands.Workflows;

namespace HR.Modules.Platform.Validators;

public class AddDynamicApproverCommandValidator : AbstractValidator<AddDynamicApproverCommand>
{
    public AddDynamicApproverCommandValidator()
    {
        RuleFor(x => x.WorkflowNodeId).NotEmpty();
        RuleFor(x => x.ApproverStrategy).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ChainLevel).GreaterThan(0);
        RuleFor(x => x.FallbackStrategy).MaximumLength(200).When(x => x.FallbackStrategy != null);
    }
}

public class UpdateDynamicApproverCommandValidator : AbstractValidator<UpdateDynamicApproverCommand>
{
    public UpdateDynamicApproverCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ApproverStrategy).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ChainLevel).GreaterThan(0);
        RuleFor(x => x.FallbackStrategy).MaximumLength(200).When(x => x.FallbackStrategy != null);
    }
}

public class AddDynamicConditionCommandValidator : AbstractValidator<AddDynamicConditionCommand>
{
    public AddDynamicConditionCommandValidator()
    {
        RuleFor(x => x.WorkflowNodeId).NotEmpty();
        RuleFor(x => x.ConditionType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FieldPath).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Operator).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Value).NotEmpty().MaximumLength(200);
        RuleFor(x => x.LogicalOperator).MaximumLength(200).When(x => x.LogicalOperator != null);
    }
}

public class UpdateDynamicConditionCommandValidator : AbstractValidator<UpdateDynamicConditionCommand>
{
    public UpdateDynamicConditionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ConditionType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FieldPath).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Operator).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Value).NotEmpty().MaximumLength(200);
        RuleFor(x => x.LogicalOperator).MaximumLength(200).When(x => x.LogicalOperator != null);
    }
}

public class AddWorkflowActionCommandValidator : AbstractValidator<AddWorkflowActionCommand>
{
    public AddWorkflowActionCommandValidator()
    {
        RuleFor(x => x.WorkflowNodeId).NotEmpty();
        RuleFor(x => x.ActionType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Configuration).MaximumLength(4000).When(x => x.Configuration != null);
    }
}

public class UpdateWorkflowActionCommandValidator : AbstractValidator<UpdateWorkflowActionCommand>
{
    public UpdateWorkflowActionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ActionType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Configuration).MaximumLength(4000).When(x => x.Configuration != null);
    }
}

public class RunWorkflowSimulationCommandValidator : AbstractValidator<RunWorkflowSimulationCommand>
{
    public RunWorkflowSimulationCommandValidator()
    {
        RuleFor(x => x.WorkflowVersionId).NotEmpty();
        RuleFor(x => x.InputData).NotEmpty();
        RuleFor(x => x.SimulatedById).NotEmpty();
    }
}
