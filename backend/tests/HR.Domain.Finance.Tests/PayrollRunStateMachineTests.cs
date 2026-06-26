using FluentAssertions;
using HR.Domain.Engines.Finance.StateMachine;
using HR.Domain.Enums;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class PayrollRunStateMachineTests
{
    [Theory]
    [InlineData(PayrollRunState.Draft, PayrollRunState.Preview)]
    [InlineData(PayrollRunState.Preview, PayrollRunState.Validated)]
    [InlineData(PayrollRunState.Validated, PayrollRunState.PendingApproval)]
    [InlineData(PayrollRunState.PendingApproval, PayrollRunState.Approved)]
    [InlineData(PayrollRunState.Approved, PayrollRunState.Executing)]
    [InlineData(PayrollRunState.Executing, PayrollRunState.Completed)]
    [InlineData(PayrollRunState.Completed, PayrollRunState.Locked)]
    [InlineData(PayrollRunState.Locked, PayrollRunState.Archived)]
    public void Allows_the_happy_path(PayrollRunState from, PayrollRunState to)
    {
        PayrollRunStateMachine.CanTransition(from, to).Should().BeTrue();
    }

    [Theory]
    [InlineData(PayrollRunState.Draft, PayrollRunState.Approved)]      // can't skip
    [InlineData(PayrollRunState.Completed, PayrollRunState.Draft)]     // can't go back
    [InlineData(PayrollRunState.Approved, PayrollRunState.Draft)]      // approved is frozen
    [InlineData(PayrollRunState.Archived, PayrollRunState.Draft)]      // terminal
    [InlineData(PayrollRunState.Cancelled, PayrollRunState.Preview)]   // terminal
    public void Rejects_illegal_transitions(PayrollRunState from, PayrollRunState to)
    {
        PayrollRunStateMachine.CanTransition(from, to).Should().BeFalse();
    }

    [Fact]
    public void EnsureCanTransition_throws_on_illegal_move()
    {
        var act = () => PayrollRunStateMachine.EnsureCanTransition(
            PayrollRunState.Draft, PayrollRunState.Completed);
        act.Should().Throw<InvalidStateTransitionException>();
    }

    [Fact]
    public void Approved_and_later_states_are_immutable()
    {
        PayrollRunStateMachine.IsImmutable(PayrollRunState.Approved).Should().BeTrue();
        PayrollRunStateMachine.IsImmutable(PayrollRunState.Completed).Should().BeTrue();
        PayrollRunStateMachine.IsImmutable(PayrollRunState.Draft).Should().BeFalse();
        PayrollRunStateMachine.IsImmutable(PayrollRunState.Validated).Should().BeFalse();
    }

    [Fact]
    public void Terminal_states_have_no_successors()
    {
        PayrollRunStateMachine.NextStates(PayrollRunState.Archived).Should().BeEmpty();
        PayrollRunStateMachine.NextStates(PayrollRunState.Cancelled).Should().BeEmpty();
        PayrollRunStateMachine.IsTerminal(PayrollRunState.Archived).Should().BeTrue();
    }

    [Fact]
    public void Failed_execution_can_be_retried()
    {
        PayrollRunStateMachine.CanTransition(PayrollRunState.Executing, PayrollRunState.Failed).Should().BeTrue();
        PayrollRunStateMachine.CanTransition(PayrollRunState.Failed, PayrollRunState.Executing).Should().BeTrue();
    }
}
