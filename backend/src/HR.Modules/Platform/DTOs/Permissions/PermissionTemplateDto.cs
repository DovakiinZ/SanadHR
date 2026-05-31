namespace HR.Modules.Platform.DTOs.Permissions;

public class PermissionTemplateDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public List<PermissionTemplateItemDto> Items { get; set; } = new();
}

public class PermissionTemplateItemDto
{
    public Guid Id { get; set; }
    public string PermissionCode { get; set; } = null!;
    public string Scope { get; set; } = null!;
}
