using FluentAssertions;
using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance;
using HR.Infrastructure.Engines.Finance;
using HR.Infrastructure.Engines.Finance.Validators;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class PayrollValidationTests
{
    private static EmployeePayrollInput Input(
        string number, decimal basic, string currency = "SAR", decimal gosiRate = 9.75m,
        bool attendance = true, Guid? id = null)
        => new()
        {
            EmployeeId = id ?? Guid.NewGuid(),
            EmployeeNumber = number,
            EmployeeName = number,
            Currency = currency,
            HasAttendanceData = attendance,
            Facts = new Dictionary<string, object?>
            {
                ["BasicSalary"] = basic,
                ["GosiRate"] = gosiRate,
            },
        };

    private static EmployeePayrollResult Result(EmployeePayrollInput input, decimal net)
        => new()
        {
            Input = input,
            Evaluation = new RuleSetEvaluation(
                Array.Empty<ComponentResult>(), Array.Empty<string>(), net, 0m, net),
        };

    private static PayrollValidationContext Ctx(
        IReadOnlyList<EmployeePayrollInput> inputs,
        IReadOnlyList<EmployeePayrollResult>? results = null,
        string currency = "SAR",
        RuleCompilationResult? compilation = null,
        IReadOnlyList<(Guid, DateTime, DateTime)>? overlapping = null)
        => new()
        {
            Period = PayrollPeriod.Monthly(2026, 6),
            Currency = currency,
            Inputs = inputs,
            Results = results ?? Array.Empty<EmployeePayrollResult>(),
            RuleCompilation = compilation,
            OverlappingRuns = overlapping ?? Array.Empty<(Guid, DateTime, DateTime)>(),
        };

    [Fact]
    public void Negative_basic_salary_is_an_error()
    {
        var findings = new NegativeSalaryValidator().Validate(Ctx(new[] { Input("E1", -100m) })).ToList();
        findings.Should().ContainSingle().Which.Severity.Should().Be(ValidationSeverity.Error);
    }

    [Fact]
    public void Negative_net_pay_is_an_error()
    {
        var i = Input("E1", 1000m);
        var findings = new NegativeSalaryValidator().Validate(Ctx(new[] { i }, new[] { Result(i, -50m) })).ToList();
        findings.Should().Contain(f => f.Code == "NEGATIVE_SALARY" && f.Severity == ValidationSeverity.Error);
    }

    [Theory]
    [InlineData(-1, true)]
    [InlineData(75, true)]
    [InlineData(9.75, false)]
    public void Invalid_gosi_rate_is_flagged(double rate, bool expectError)
    {
        var findings = new InvalidGosiValidator().Validate(Ctx(new[] { Input("E1", 1000m, gosiRate: (decimal)rate) })).ToList();
        findings.Any(f => f.Severity == ValidationSeverity.Error).Should().Be(expectError);
    }

    [Fact]
    public void Duplicate_employee_is_an_error()
    {
        var id = Guid.NewGuid();
        var inputs = new[] { Input("E1", 1000m, id: id), Input("E1", 1000m, id: id) };
        new DuplicateEmployeeValidator().Validate(Ctx(inputs)).Should()
            .ContainSingle().Which.Code.Should().Be("DUPLICATE_EMPLOYEE");
    }

    [Fact]
    public void Currency_mismatch_is_an_error()
    {
        var inputs = new[] { Input("E1", 1000m, currency: "USD") };
        new CurrencyValidator().Validate(Ctx(inputs, currency: "SAR")).Should()
            .ContainSingle().Which.Code.Should().Be("CURRENCY_MISMATCH");
    }

    [Fact]
    public void Missing_attendance_is_a_warning()
    {
        var inputs = new[] { Input("E1", 1000m, attendance: false) };
        var findings = new MissingAttendanceValidator().Validate(Ctx(inputs)).ToList();
        findings.Should().ContainSingle().Which.Severity.Should().Be(ValidationSeverity.Warning);
    }

    [Fact]
    public void Overlapping_run_is_an_error()
    {
        var overlap = new[] { (Guid.NewGuid(), DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(5)) };
        new OverlappingPayrollValidator().Validate(Ctx(new[] { Input("E1", 1000m) }, overlapping: overlap))
            .Should().ContainSingle().Which.Code.Should().Be("OVERLAPPING_PAYROLL");
    }

    [Fact]
    public void Rule_compilation_errors_surface_as_conflicts()
    {
        var comp = new RuleCompilationResult { IsValid = false, Errors = new[] { "cycle: A → B → A" } };
        new RuleConflictValidator().Validate(Ctx(new[] { Input("E1", 1000m) }, compilation: comp))
            .Should().ContainSingle().Which.Code.Should().Be("RULE_CONFLICT");
    }

    [Fact]
    public void Engine_aggregates_validators_and_blocks_on_any_error()
    {
        var engine = new PayrollValidationEngine(new IPayrollValidator[]
        {
            new NegativeSalaryValidator(), new CurrencyValidator(), new MissingAttendanceValidator(),
        });

        var bad = Input("E1", -100m, currency: "USD", attendance: false);
        var report = engine.Validate(Ctx(new[] { bad }, currency: "SAR"));

        report.IsValid.Should().BeFalse();                 // has errors
        report.Errors.Should().HaveCountGreaterThanOrEqualTo(2);   // negative salary + currency
        report.Warnings.Should().ContainSingle();           // missing attendance
    }

    [Fact]
    public void Clean_run_passes_validation()
    {
        var engine = new PayrollValidationEngine(new IPayrollValidator[]
        {
            new NegativeSalaryValidator(), new CurrencyValidator(), new InvalidGosiValidator(),
        });
        var ok = Input("E1", 10000m);
        engine.Validate(Ctx(new[] { ok }, new[] { Result(ok, 9000m) })).IsValid.Should().BeTrue();
    }
}
