namespace HR.Application.Engines.Permissions;

public interface IPermissionEvaluator
{
    Task<bool> HasPermission(Guid userId, string permissionCode,
        Guid? entityTenantId = null, Guid? entityBranchId = null, Guid? entityDepartmentId = null);
}
