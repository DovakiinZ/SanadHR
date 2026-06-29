using HR.Domain.Engines.Finance;
using HR.Domain.Enums;

namespace HR.Application.Engines.Finance;

public sealed record CreatePayrollTypeArgs(string Code, string Name, string? NameAr, Guid? CategoryId);

public sealed class UpdatePayrollVersionArgs
{
    public int? CutoffDay { get; init; }
    public DayBasis? DayBasis { get; init; }
    public DateTime? ClosingDate { get; init; }
    public DateTime? PaymentDate { get; init; }
    public bool? CarryToNextPeriod { get; init; }
    public Guid? DefaultExportFormatId { get; init; }
    public Guid? PaymentMethodId { get; init; }
    public Guid? ApprovalWorkflowId { get; init; }
    public Guid? RuleSetVersionId { get; init; }
    public string? Currency { get; init; }
    public PayFrequency? Frequency { get; init; }
    public string? SelectionScopeJson { get; init; }
    public string? CalcSettingsJson { get; init; }
    public string? PaymentMethodScopeJson { get; init; }
}

public interface IPayrollTypeService
{
    Task<Guid> CreateTypeAsync(CreatePayrollTypeArgs args, CancellationToken ct);
    Task UpdateHeaderAsync(Guid typeId, string name, string? nameAr, Guid? categoryId, PayrollDefinitionStatus status, CancellationToken ct);
    Task<Guid> CreateDraftVersionAsync(Guid typeId, CancellationToken ct);
    Task UpdateDraftVersionAsync(Guid typeId, Guid versionId, UpdatePayrollVersionArgs args, CancellationToken ct);
    Task<Guid> CloneVersionAsync(Guid typeId, Guid versionId, CancellationToken ct);
    Task PublishVersionAsync(Guid typeId, Guid versionId, CancellationToken ct);
    Task<PayrollPreview> SimulateAsync(Guid typeId, Guid versionId, int year, int month, CancellationToken ct);
}
