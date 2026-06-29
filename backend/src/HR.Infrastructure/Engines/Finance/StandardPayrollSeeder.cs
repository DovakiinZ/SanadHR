using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Engines.Finance.Expressions;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Finance;

/// <summary>Seeds a tenant's default "Standard Monthly" rule set + payroll definition so payroll is
/// runnable out of the box. Rules are authored in the expression language and compiled to AST on save,
/// exactly as a user-built rule set would be.</summary>
public sealed class StandardPayrollSeeder : IStandardPayrollSeeder
{
    private const string RuleSetCode = "STD_MONTHLY";
    private const string DefinitionCode = "MONTHLY";

    private readonly ApplicationDbContext _db;
    public StandardPayrollSeeder(ApplicationDbContext db) => _db = db;

    private sealed record RuleDef(string Code, string NameAr, PayComponentKind Kind, string Expr);

    // Facts come from the fact provider: BasicSalary, TotalAllowances, TotalAdditions, TotalDeductions,
    // GosiBase, GosiRate, and the attendance inputs (DailyWage, HourlyWage, AbsentDays, LateHours,
    // ShortageHours). Earnings build gross; deductions reduce net (aggregated by RuleEngineCore).
    private static readonly RuleDef[] Rules =
    {
        new("BASIC", "الراتب الأساسي", PayComponentKind.Earning, "BasicSalary"),
        new("ALLOWANCES", "البدلات", PayComponentKind.Earning, "TotalAllowances"),
        new("ADDITIONS", "الإضافات", PayComponentKind.Earning, "TotalAdditions"),
        new("GOSI", "التأمينات الاجتماعية", PayComponentKind.Deduction, "ROUND(PERCENT(GosiBase, GosiRate), 2)"),
        new("DEDUCTIONS", "الاستقطاعات", PayComponentKind.Deduction, "TotalDeductions"),
        // Automatic attendance deduction: full-day absences at the daily wage, plus late + missing
        // hours at the hourly wage. Computed by the system from the attendance engine; HR reviews it
        // in the run preview before approving.
        new("ATTENDANCE_DED", "حسم الغياب والتأخير والساعات الناقصة", PayComponentKind.Deduction,
            "ROUND(AbsentDays * DailyWage + (LateHours + ShortageHours) * HourlyWage, 2)"),
    };

    public async Task<Guid> EnsureStandardMonthlyAsync(CancellationToken ct = default)
    {
        var definition = await _db.PayrollDefinitions.FirstOrDefaultAsync(d => d.Code == DefinitionCode, ct);

        // Rule set + published version with compiled rules.
        var ruleSet = await _db.FinanceRuleSets.FirstOrDefaultAsync(r => r.Code == RuleSetCode, ct);
        Guid ruleSetVersionId;
        if (ruleSet is null)
        {
            ruleSet = new RuleSet { Code = RuleSetCode, Name = "Standard Monthly", NameAr = "الرواتب الشهرية القياسية", Status = RuleSetStatus.Active };
            _db.FinanceRuleSets.Add(ruleSet);

            var version = new RuleSetVersion { RuleSetId = ruleSet.Id, VersionNumber = 1, Status = VersionStatus.Published, PublishedAt = DateTime.UtcNow };
            _db.FinanceRuleSetVersions.Add(version);

            var seq = 1;
            foreach (var r in Rules)
                _db.FinanceRules.Add(BuildRule(version.Id, r, seq++));
            ruleSet.CurrentVersionId = version.Id;
            ruleSetVersionId = version.Id;
        }
        else
        {
            ruleSetVersionId = ruleSet.CurrentVersionId
                ?? (await _db.FinanceRuleSetVersions.Where(v => v.RuleSetId == ruleSet.Id)
                        .OrderByDescending(v => v.VersionNumber).Select(v => v.Id).FirstAsync(ct));
        }

        // Idempotently top-up any rules missing from the active version (so an already-seeded tenant
        // picks up newly-added standard rules such as ATTENDANCE_DED without a version bump).
        await EnsureRulesPresentAsync(ruleSetVersionId, ct);

        if (definition is not null && definition.CurrentVersionId is not null)
        {
            await _db.SaveChangesAsync(ct);
            return definition.Id;
        }

        // Definition + published version pinned to the rule set.
        if (definition is null)
        {
            definition = new PayrollDefinition { Code = DefinitionCode, Name = "Monthly Payroll", NameAr = "الرواتب الشهرية", Scope = PayrollScope.Company, Status = PayrollDefinitionStatus.Active };
            _db.PayrollDefinitions.Add(definition);
        }
        var defVersion = new PayrollDefinitionVersion
        {
            PayrollDefinitionId = definition.Id,
            VersionNumber = 1,
            Status = VersionStatus.Published,
            Frequency = PayFrequency.Monthly,
            RuleSetVersionId = ruleSetVersionId,
            Currency = "SAR",
            PublishedAt = DateTime.UtcNow,
        };
        _db.PayrollDefinitionVersions.Add(defVersion);
        definition.CurrentVersionId = defVersion.Id;

        await _db.SaveChangesAsync(ct);
        return definition.Id;
    }

    /// <summary>Adds any standard rule (by Code) not already present on the given rule-set version.</summary>
    private async Task EnsureRulesPresentAsync(Guid ruleSetVersionId, CancellationToken ct)
    {
        var existing = await _db.FinanceRules
            .Where(r => r.RuleSetVersionId == ruleSetVersionId)
            .Select(r => new { r.Code, r.Sequence })
            .ToListAsync(ct);
        var have = existing.Select(r => r.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var seq = existing.Count == 0 ? 1 : existing.Max(r => r.Sequence) + 1;

        foreach (var r in Rules)
        {
            if (have.Contains(r.Code)) continue;
            _db.FinanceRules.Add(BuildRule(ruleSetVersionId, r, seq++));
        }
    }

    private static Rule BuildRule(Guid ruleSetVersionId, RuleDef r, int sequence)
    {
        var ast = ExpressionParser.Parse(r.Expr);
        return new Rule
        {
            RuleSetVersionId = ruleSetVersionId,
            Code = r.Code,
            Name = r.Code,
            NameAr = r.NameAr,
            Kind = r.Kind,
            Sequence = sequence,
            ExpressionText = r.Expr,
            ExpressionAstJson = AstJson.Serialize(ast),
            OutputComponentCode = r.Code,
            IsActive = true,
        };
    }
}
