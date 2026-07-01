using System.Text.Json;
using FluentAssertions;
using HR.Domain.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Engines.Finance;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class PayslipLedgerMapperTests
{
    private static PayrollPayslip Payslip(params (string code, PayComponentKind kind, decimal amount, bool applied)[] comps)
    {
        var components = comps.Select(c => new ComponentResult(c.code, c.code, c.kind, c.amount, c.applied)).ToList();
        return new PayrollPayslip
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            EmployeeNumber = "E001",
            Currency = "SAR",
            ComponentsJson = JsonSerializer.Serialize(new { order = comps.Select(c => c.code), components }),
        };
    }

    [Fact]
    public void Earnings_credit_and_deductions_debit_the_employee()
    {
        var p = Payslip(
            ("BASIC", PayComponentKind.Earning, 10000m, true),
            ("GOSI", PayComponentKind.Deduction, 975m, true));

        var postings = PayslipLedgerMapper.Map(Guid.NewGuid(), p);

        postings.Should().HaveCount(2);
        postings.Single(x => x.ComponentCode == "BASIC").Direction.Should().Be(LedgerDirection.Credit);
        postings.Single(x => x.ComponentCode == "GOSI").Direction.Should().Be(LedgerDirection.Debit);
    }

    [Fact]
    public void Contributions_information_zero_and_unapplied_are_skipped()
    {
        var p = Payslip(
            ("BASIC", PayComponentKind.Earning, 10000m, true),
            ("EMPLOYER_GOSI", PayComponentKind.Contribution, 1175m, true),  // employer side — skip
            ("GROSS", PayComponentKind.Information, 12500m, true),           // derived — skip
            ("ZERO", PayComponentKind.Earning, 0m, true),                   // zero — skip
            ("INACTIVE", PayComponentKind.Earning, 500m, false));           // not applied — skip

        var postings = PayslipLedgerMapper.Map(Guid.NewGuid(), p);

        postings.Should().ContainSingle().Which.ComponentCode.Should().Be("BASIC");
    }

    [Fact]
    public void Entry_numbers_are_deterministic_and_reference_the_payslip()
    {
        var p = Payslip(
            ("BASIC", PayComponentKind.Earning, 10000m, true),
            ("GOSI", PayComponentKind.Deduction, 975m, true));
        var runId = Guid.NewGuid();

        var first = PayslipLedgerMapper.Map(runId, p);
        var second = PayslipLedgerMapper.Map(runId, p);

        first.Select(x => x.EntryNumber).Should().Equal(second.Select(x => x.EntryNumber)); // stable across runs
        first.Select(x => x.EntryNumber).Should().OnlyHaveUniqueItems();
        first.Should().OnlyContain(x => x.ReferenceType == PayslipLedgerMapper.PayslipReference && x.ReferenceId == p.Id);
        first.Should().OnlyContain(x => x.PayrollRunId == runId && x.SourceModule == FinanceSourceModule.Payroll);
    }

    [Fact]
    public void Amounts_are_posted_as_absolute_values()
    {
        var p = Payslip(("ADJ", PayComponentKind.Deduction, 250m, true));
        PayslipLedgerMapper.Map(Guid.NewGuid(), p).Single().Amount.Should().Be(250m);
    }

    [Fact]
    public void Transaction_sourced_component_is_tagged_with_its_transaction_reference()
    {
        var txnId = Guid.NewGuid();
        var payslip = Payslip(
            ("BASIC", PayComponentKind.Earning, 1000m, true),
            ($"TXN:{txnId:N}", PayComponentKind.Earning, 200m, true));

        var postings = PayslipLedgerMapper.Map(Guid.NewGuid(), payslip);

        var txnPosting = postings.Single(p => p.ReferenceType == "PayrollTransaction");
        txnPosting.ReferenceId.Should().Be(txnId);
        txnPosting.Amount.Should().Be(200m);
        postings.Should().Contain(p => p.ReferenceType == "PayrollPayslip" && p.Amount == 1000m);
    }
}
