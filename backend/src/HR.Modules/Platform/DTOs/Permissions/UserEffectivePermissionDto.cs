namespace HR.Modules.Platform.DTOs.Permissions;

public class UserEffectivePermissionDto
{
    public string PermissionCode { get; set; } = null!;
    public bool IsGranted { get; set; }
    public string Scope { get; set; } = null!;
    public string Source { get; set; } = null!; // Template or Override
}

public class UserPermissionTemplateDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PermissionTemplateId { get; set; }
    public string TemplateNameEn { get; set; } = null!;
    public string TemplateNameAr { get; set; } = null!;
    public DateTime AssignedAt { get; set; }
    public string? AssignedBy { get; set; }
}
