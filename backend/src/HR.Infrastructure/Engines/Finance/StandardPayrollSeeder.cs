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
    // GosiBase, GosiRate. Earnings build gross; deductions reduce net (aggregated by RuleEngineCore).
    private static readonly RuleDef[] Rules =
    {
        new("BASIC", "الراتب الأساسي", PayComponentKind.Earning, "BasicSalary"),
        new("ALLOWANCES", "البدلات", PayComponentKind.Earning, "TotalAllowances"),
        new("ADDITIONS", "الإضافات", PayComponentKind.Earning, "TotalAdditions"),
        new("GOSI", "التأمينات الاجتماعية", PayComponentKind.Deduction, "ROUND(PERCENT(GosiBase, GosiRate), 2)"),
        new("DEDUCTIONS", "الاستقطاعات", PayComponentKind.Deduction, "TotalDeductions"),
    };

    public async Task<Guid> EnsureStandardMonthlyAsync(CancellationToken ct = default)
    {
        var definition = await _db.PayrollDefinitions.FirstOrDefaultAsync(d => d.Code == DefinitionCode, ct);
        if (definition is not null && definition.CurrentVersionId is not null)
            return definition.Id;

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
            {
                var ast = ExpressionParser.Parse(r.Expr);
                _db.FinanceRules.Add(new Rule
                {
                    RuleSetVersionId = version.Id,
                    Code = r.Code,
                    Name = r.Code,
                    NameAr = r.NameAr,
                    Kind = r.Kind,
                    Sequence = seq++,
                    ExpressionText = r.Expr,
                    ExpressionAstJson = AstJson.Serialize(ast),
                    OutputComponentCode = r.Code,
                    IsActive = true,
                });
            }
            ruleSet.CurrentVersionId = version.Id;
            ruleSetVersionId = version.Id;
        }
        else
        {
            ruleSetVersionId = ruleSet.CurrentVersionId
                ?? (await _db.FinanceRuleSetVersions.Where(v => v.RuleSetId == ruleSet.Id)
                        .OrderByDescending(v => v.VersionNumber).Select(v => v.Id).FirstAsync(ct));
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
}
