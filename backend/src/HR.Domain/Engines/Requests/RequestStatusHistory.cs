using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Requests;

/// <summary>Append-only status transition log for a request (drives the request timeline view).</summary>
public class RequestStatusHistory : BaseEntity
{
    public Guid RequestInstanceId { get; set; }
    public RequestStatus? FromStatus { get; set; }
    public RequestStatus ToStatus { get; set; }
    public Guid? ActorUserId { get; set; }
    public string? NoteAr { get; set; }
    public string? NoteEn { get; set; }
    public DateTime At { get; set; }

    public RequestInstance RequestInstance { get; set; } = null!;
}
