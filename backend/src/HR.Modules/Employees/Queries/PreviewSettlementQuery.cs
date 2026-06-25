using System.Text.Json.Serialization;
using HR.Application.Engines.Settlement;
using HR.Modules.Employees.DTOs;
using HR.Domain.Enums;
using MediatR;

namespace HR.Modules.Employees.Queries;

/// <summary>Compute an end-of-service settlement without persisting anything — drives the live preview
/// on the termination form.</summary>
public record PreviewSettlementQuery : IRequest<SettlementResultDto>
{
    public Guid EmployeeId { get; init; }
    public DateTime TerminationDate { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TerminationScenario Scenario { get; init; } = TerminationScenario.NormalEmployerTermination;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ContractTermType ContractTermType { get; init; } = ContractTermType.Indefinite;
    public DateTime? ContractEndDate { get; init; }
}

public class PreviewSettlementQueryHandler : IRequestHandler<PreviewSettlementQuery, SettlementResultDto>
{
    private readonly IEndOfServiceEngine _engine;

    public PreviewSettlementQueryHandler(IEndOfServiceEngine engine) => _engine = engine;

    public async Task<SettlementResultDto> Handle(PreviewSettlementQuery request, CancellationToken cancellationToken)
    {
        var result = await _engine.PreviewAsync(new SettlementRequest
        {
            EmployeeId = request.EmployeeId,
            TerminationDate = request.TerminationDate,
            Scenario = request.Scenario,
            ContractTermType = request.ContractTermType,
            ContractEndDate = request.ContractEndDate,
        }, cancellationToken);
        return SettlementResultDto.From(result);
    }
}
