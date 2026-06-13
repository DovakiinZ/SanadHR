using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Requests;

/// <summary>Who may submit/approve/view a request type. References roles/permissions, not text.</summary>
public class RequestPermission : BaseEntity
{
    public Guid RequestTypeId { get; set; }
    public RequestPermissionAction Action { get; set; }
    public string? PermissionCode { get; set; }
    public Guid? RoleId { get; set; }

    public RequestType RequestType { get; set; } = null!;
}
