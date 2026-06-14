using HR.Domain.Common;

namespace HR.Domain.Engines.Documents;

/// <summary>
/// A file attached to an employee's personnel record (ID, Iqama, passport, contract, certificate,
/// medical report, or a custom type). The optional <see cref="ExpiryDate"/> drives expiry-reminder
/// notifications (see the notification rules engine). Files themselves live in <c>stored_files</c>;
/// this row holds the metadata + the capability URL.
/// </summary>
public class EmployeeDocument : TenantEntity
{
    public Guid EmployeeId { get; set; }

    /// <summary>Stable type code: Id | Iqama | Passport | Contract | Certificate | MedicalReport | Custom.</summary>
    public string Type { get; set; } = "Custom";

    /// <summary>Display name — a custom label or the document's title.</summary>
    public string Title { get; set; } = null!;

    /// <summary>Optional document number (e.g. Iqama / passport number).</summary>
    public string? DocumentNumber { get; set; }

    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }

    /// <summary>Capability URL of the stored file, e.g. <c>/api/files/{id}</c>.</summary>
    public string FileUrl { get; set; } = null!;
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long SizeBytes { get; set; }
}
