using FluentAssertions;
using HR.Domain.Engines.Finance.StateMachine;
using HR.Domain.Enums;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class PayrollTransactionStateMachineTests
{
    [Theory]
    [InlineData(PayrollTransactionStatus.Draft, PayrollTransactionStatus.PendingApproval)]
    [InlineData(PayrollTransactionStatus.Draft, PayrollTransactionStatus.Cancelled)]
    [InlineData(PayrollTransactionStatus.PendingApproval, PayrollTransactionStatus.Approved)]
    [InlineData(PayrollTransactionStatus.PendingApproval, PayrollTransactionStatus.Rejected)]
    [InlineData(PayrollTransactionStatus.Rejected, PayrollTransactionStatus.Draft)]
    [InlineData(PayrollTransactionStatus.Approved, PayrollTransactionStatus.Cancelled)]
    [InlineData(PayrollTransactionStatus.Approved, PayrollTransactionStatus.Posted)]
    [InlineData(PayrollTransactionStatus.Approved, PayrollTransactionStatus.CarriedForward)]
    [InlineData(PayrollTransactionStatus.CarriedForward, PayrollTransactionStatus.Posted)]
    [InlineData(PayrollTransactionStatus.Posted, PayrollTransactionStatus.Reversed)]
    public void Allows_legal_transitions(PayrollTransactionStatus from, PayrollTransactionStatus to)
    {
        PayrollTransactionStateMachine.CanTransition(from, to).Should().BeTrue();
    }

    [Theory]
    [InlineData(PayrollTransactionStatus.Draft, PayrollTransactionStatus.Approved)]   // can't skip approval
    [InlineData(PayrollTransactionStatus.Draft, PayrollTransactionStatus.Posted)]     // can't skip to posted
    [InlineData(PayrollTransactionStatus.Approved, PayrollTransactionStatus.Draft)]   // approved is locked
    [InlineData(PayrollTransactionStatus.Posted, PayrollTransactionStatus.Approved)]  // posted is immutable
    [InlineData(PayrollTransactionStatus.Cancelled, PayrollTransactionStatus.Draft)]  // terminal
    [InlineData(PayrollTransactionStatus.Reversed, PayrollTransactionStatus.Posted)]  // terminal
    public void Rejects_illegal_transitions(PayrollTransactionStatus from, PayrollTransactionStatus to)
    {
        PayrollTransactionStateMachine.CanTransition(from, to).Should().BeFalse();
    }

    [Fact]
    public void EnsureCanTransition_throws_on_illegal_move()
    {
        var act = () => PayrollTransactionStateMachine.EnsureCanTransition(
            PayrollTransactionStatus.Draft, PayrollTransactionStatus.Posted);
        act.Should().Throw<InvalidPayrollTransactionStateException>();
    }

    [Fact]
    public void Posted_is_immutable()
    {
        PayrollTransactionStateMachine.IsImmutable(PayrollTransactionStatus.Posted).Should().BeTrue();
        PayrollTransactionStateMachine.IsImmutable(PayrollTransactionStatus.Draft).Should().BeFalse();
        PayrollTransactionStateMachine.IsImmutable(PayrollTransactionStatus.Approved).Should().BeFalse();
    }

    [Fact]
    public void Terminal_states_have_no_successors()
    {
        PayrollTransactionStateMachine.NextStates(PayrollTransactionStatus.Cancelled).Should().BeEmpty();
        PayrollTransactionStateMachine.NextStates(PayrollTransactionStatus.Reversed).Should().BeEmpty();
        PayrollTransactionStateMachine.IsTerminal(PayrollTransactionStatus.Cancelled).Should().BeTrue();
        PayrollTransactionStateMachine.IsTerminal(PayrollTransactionStatus.Reversed).Should().BeTrue();
    }
}
