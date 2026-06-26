using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BackfillSystemRolePermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // System roles ("Admin") are meant to hold every permission, but a tenant created before a new
            // permission existed won't have it. Grant any missing permission to every system role so that
            // existing admins keep full access (including the new Settings.Manage* keys). Idempotent.
            migrationBuilder.Sql(@"
                INSERT INTO role_permissions (""Id"", ""RoleId"", ""PermissionId"")
                SELECT gen_random_uuid(), r.""Id"", p.""Id""
                FROM roles r
                CROSS JOIN permissions p
                WHERE r.""IsSystemRole"" = true
                  AND NOT EXISTS (
                      SELECT 1 FROM role_permissions rp
                      WHERE rp.""RoleId"" = r.""Id"" AND rp.""PermissionId"" = p.""Id""
                  );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
