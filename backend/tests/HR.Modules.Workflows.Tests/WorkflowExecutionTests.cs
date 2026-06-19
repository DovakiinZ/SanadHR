using FluentAssertions;
using HR.Application.Common.Exceptions;
using HR.Domain.Engines.FlowBuilder;
using HR.Modules.Workflows.Commands;
using HR.Modules.Workflows.DTOs;
using Xunit;

namespace HR.Modules.Workflows.Tests;

/// <summary>
/// Exercises the MediatR handlers end-to-end over the in-memory provider: starting a request,
/// auto-advancing through non-blocking steps, executing an approval decision, and idempotency.
/// Each handler call gets its own DbContext (sharing one named in-memory DB) to mirror the
/// scoped-per-request lifetime used in production.
/// </summary>
public class WorkflowExecutionTests
{
    private readonly FakeCurrentUserService _user = new();
    private readonly FakeEmailSender _email = new();
    private readonly string _db = TestHarness.NewDbName();

    // ---- helpers ----------------------------------------------------------------------------

    private static WorkflowStep Step(WorkflowStepType type, string name,
        Guid? ok = null, Guid? fail = null, string config = "{}") => new()
    {
        Id = Guid.NewGuid(),
        TenantId = FakeCurrentUserService.Tenant,
        Type = type,
        Name = name,
        Config = config,
        NextStepIdSuccess = ok,
        NextStepIdFailure = fail
    };

    private async Task<Guid> SeedAsync(string code, Guid rootId, params WorkflowStep[] steps)
    {
        using var ctx = TestHarness.NewContext(_user, _db);
        var def = new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = FakeCurrentUserService.Tenant,
            Code = code,
            Name = code,
            IsActive = true,
            RootStepId = rootId
        };
        foreach (var s in steps) s.DefinitionId = def.Id;
        ctx.FlowDefinitions.Add(def);
        ctx.FlowSteps.AddRange(steps);
        await ctx.SaveChangesAsync();
        return def.Id;
    }

    private async Task<WorkflowRequestDto> StartAsync(Guid defId, string payload = "{}")
    {
        using var ctx = TestHarness.NewContext(_user, _db);
        var handler = new StartWorkflowRequestCommandHandler(ctx, _user, TestHarness.NewRunner(ctx, _email));
        return await handler.Handle(new StartWorkflowRequestCommand { DefinitionId = defId, Payload = payload }, default);
    }

    private async Task<WorkflowRequestDto> ExecuteAsync(Guid requestId, bool approved)
    {
        using var ctx = TestHarness.NewContext(_user, _db);
        var handler = new ExecuteWorkflowStepCommandHandler(ctx, _user, TestHarness.NewRunner(ctx, _email));
        return await handler.Handle(new ExecuteWorkflowStepCommand { RequestId = requestId, Approved = approved }, default);
    }

    // ---- tests ------------------------------------------------------------------------------

    [Fact]
    public async Task Start_parks_on_the_first_approval_step()
    {
        var end = Step(WorkflowStepType.End, "End");
        var approval = Step(WorkflowStepType.Approval, "Manager", ok: end.Id);
        var defId = await SeedAsync("leave", approval.Id, approval, end);

        var dto = await StartAsync(defId);

        dto.Status.Should().Be(WorkflowRequestStatus.InProgress);
        dto.CurrentStepId.Should().Be(approval.Id);
        dto.RequestNumber.Should().StartWith("WF-");
        dto.AuditTrail.Should().ContainSingle(a => a.Action == "Submitted");
    }

    [Fact]
    public async Task Approving_advances_to_end_and_completes()
    {
        var end = Step(WorkflowStepType.End, "End");
        var approval = Step(WorkflowStepType.Approval, "Manager", ok: end.Id);
        var defId = await SeedAsync("leave", approval.Id, approval, end);

        var started = await StartAsync(defId);
        var done = await ExecuteAsync(started.Id, approved: true);

        done.Status.Should().Be(WorkflowRequestStatus.Completed);
        done.CurrentStepId.Should().BeNull();
        done.AuditTrail.Should().Contain(a => a.Action == "Approved");
        done.AuditTrail.Should().Contain(a => a.Action == "Completed");
    }

    [Fact]
    public async Task Rejecting_with_no_failure_branch_marks_request_rejected()
    {
        var end = Step(WorkflowStepType.End, "End");
        var approval = Step(WorkflowStepType.Approval, "Manager", ok: end.Id, fail: null);
        var defId = await SeedAsync("leave", approval.Id, approval, end);

        var started = await StartAsync(defId);
        var done = await ExecuteAsync(started.Id, approved: false);

        done.Status.Should().Be(WorkflowRequestStatus.Rejected);
        done.AuditTrail.Should().Contain(a => a.Action == "Rejected");
    }

    [Fact]
    public async Task Condition_true_routes_through_email_action_to_completion()
    {
        var end = Step(WorkflowStepType.End, "End");
        var emailStep = Step(WorkflowStepType.Action, "Notify", ok: end.Id,
            config: "{\"actionType\":\"email\",\"toEmail\":\"boss@x.com\",\"subject\":\"Amount {{amount}}\",\"body\":\"hi\"}");
        var cond = Step(WorkflowStepType.Condition, "Over 1000?", ok: emailStep.Id, fail: end.Id,
            config: "{\"field\":\"amount\",\"operator\":\"gt\",\"value\":\"1000\"}");
        var defId = await SeedAsync("expense", cond.Id, cond, emailStep, end);

        var dto = await StartAsync(defId, payload: "{\"amount\":5000}");

        dto.Status.Should().Be(WorkflowRequestStatus.Completed); // no blocking step on this path
        _email.Sent.Should().ContainSingle();
        _email.Sent[0].Subject.Should().Be("Amount 5000"); // token substitution
        dto.AuditTrail.Should().Contain(a => a.Action == "ConditionEvaluated");
        dto.AuditTrail.Should().Contain(a => a.Action == "ActionExecuted");
    }

    [Fact]
    public async Task Condition_false_skips_the_action_branch()
    {
        var end = Step(WorkflowStepType.End, "End");
        var emailStep = Step(WorkflowStepType.Action, "Notify", ok: end.Id,
            config: "{\"actionType\":\"email\",\"toEmail\":\"boss@x.com\",\"subject\":\"s\",\"body\":\"b\"}");
        var cond = Step(WorkflowStepType.Condition, "Over 1000?", ok: emailStep.Id, fail: end.Id,
            config: "{\"field\":\"amount\",\"operator\":\"gt\",\"value\":\"1000\"}");
        var defId = await SeedAsync("expense", cond.Id, cond, emailStep, end);

        var dto = await StartAsync(defId, payload: "{\"amount\":10}");

        dto.Status.Should().Be(WorkflowRequestStatus.Completed);
        _email.Sent.Should().BeEmpty();
    }

    [Fact]
    public async Task Executing_a_finished_request_is_rejected_idempotently()
    {
        var end = Step(WorkflowStepType.End, "End");
        var approval = Step(WorkflowStepType.Approval, "Manager", ok: end.Id);
        var defId = await SeedAsync("leave", approval.Id, approval, end);

        var started = await StartAsync(defId);
        await ExecuteAsync(started.Id, approved: true); // completes

        var act = async () => await ExecuteAsync(started.Id, approved: true);
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Multi_level_approval_parks_between_each_approver()
    {
        var end = Step(WorkflowStepType.End, "End");
        var second = Step(WorkflowStepType.Approval, "Director", ok: end.Id);
        var first = Step(WorkflowStepType.Approval, "Manager", ok: second.Id);
        var defId = await SeedAsync("leave", first.Id, first, second, end);

        var started = await StartAsync(defId);
        started.CurrentStepId.Should().Be(first.Id);

        var afterFirst = await ExecuteAsync(started.Id, approved: true);
        afterFirst.Status.Should().Be(WorkflowRequestStatus.InProgress);
        afterFirst.CurrentStepId.Should().Be(second.Id); // parked on the next approver

        var afterSecond = await ExecuteAsync(started.Id, approved: true);
        afterSecond.Status.Should().Be(WorkflowRequestStatus.Completed);
    }
}
