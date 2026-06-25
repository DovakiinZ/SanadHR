using System.Text.Json.Serialization;
using HR.Application.Engines.Settlement;
using HR.Modules.Employees.DTOs;
using HR.Domain.Engines.Settlement;
using HR.Domain.Enums;
using MediatR;

namespace HR.Modules.Employees.Commands;

/// <summary>Compute and persist an end-of-service settlement for an employee, then transition the
/// employee's status (Terminated/Resigned). Returns the itemized award.</summary>
public record TerminateEmployeeCommand : IRequest<SettlementResultDto>
{
    public Guid EmployeeId { get; init; }
    public DateTime TerminationDate { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TerminationScenario Scenario { get; init; } = TerminationScenario.NormalEmployerTermination;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ContractTermType ContractTermType { get; init; } = ContractTermType.Indefinite;
    public DateTime? ContractEndDate { get; init; }
    public string? Notes { get; init; }
}

public class TerminateEmployeeCommandHandler : IRequestHandler<TerminateEmployeeCommand, SettlementResultDto>
{
    private readonly IEndOfServiceEngine _engine;

    public TerminateEmployeeCommandHandler(IEndOfServiceEngine engine) => _engine = engine;

    public async Task<SettlementResultDto> Handle(TerminateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var settlement = await _engine.SettleAsync(new SettlementRequest
        {
            EmployeeId = request.EmployeeId,
            TerminationDate = request.TerminationDate,
            Scenario = request.Scenario,
            ContractTermType = request.ContractTermType,
            ContractEndDate = request.ContractEndDate,
            Notes = request.Notes,
        }, cancellationToken);

        // Re-run the pure calc shape from the persisted snapshot for a stable DTO (cheap; no extra IO).
        var result = new SettlementResult
        {
            Scenario = settlement.Scenario,
            ContractTermType = settlement.ContractTermType,
            Currency = settlement.Currency,
            MonthlyWage = settlement.MonthlyWage,
            DailyWage = settlement.DailyWage,
            ServiceYears = settlement.ServiceYears,
            EffectiveServiceDays = settlement.EffectiveServiceDays,
            UnpaidLeaveDays = settlement.UnpaidLeaveDays,
            GratuityAmount = settlement.GratuityAmount,
            Article77Award = settlement.Article77Award,
            NoticeCompensation = settlement.NoticeCompensation,
            TotalAward = settlement.TotalAward,
            Lines = settlement.Items
                .Select(i => new SettlementLine(i.LabelEn, i.LabelAr, i.ArticleRef, i.Amount))
                .ToList(),
        };
        return SettlementResultDto.From(result, settlement.Id);
    }
}
