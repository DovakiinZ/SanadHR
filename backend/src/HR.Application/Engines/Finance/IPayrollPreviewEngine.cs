using HR.Domain.Engines.Finance;

namespace HR.Application.Engines.Finance;

/// <summary>Computes a full payroll preview — every employee calculated, validated and summarized — with
/// zero database writes. The safe "what would happen" view before a run is created or calculated.</summary>
public interface IPayrollPreviewEngine
{
    /// <summary>Preview a published payroll definition version for a period.</summary>
    Task<PayrollPreview> PreviewAsync(
        Guid payrollDefinitionVersionId, PayrollPeriod period, CancellationToken ct = default);
}
