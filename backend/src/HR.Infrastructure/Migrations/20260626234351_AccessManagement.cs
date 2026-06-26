using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AccessManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ResetTokenExpiresAt",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResetTokenHash",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResetTokenPurpose",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "Id", "Description", "Module", "Name" },
                values: new object[,]
                {
                    { new Guid("0f993be1-195a-6619-1bbf-cc315e959199"), "View permission for Requests", "Requests", "View" },
                    { new Guid("39c0b264-41d0-5918-be11-9df84ef7ecc2"), "ManageTemplates permission for Settings", "Settings", "ManageTemplates" },
                    { new Guid("44d16d33-4958-01c6-27d7-424942af81d7"), "ManageUsers permission for Settings", "Settings", "ManageUsers" },
                    { new Guid("476a9d1a-393e-4dde-9f96-42a58884bc7b"), "ViewAudit permission for Settings", "Settings", "ViewAudit" },
                    { new Guid("82b38544-a228-6dc2-3325-e13086acb022"), "Reject permission for Requests", "Requests", "Reject" },
                    { new Guid("8a257e8f-c0b2-69e6-65d6-b27368690e32"), "Lock permission for Payroll", "Payroll", "Lock" },
                    { new Guid("9cd46e60-f870-bddf-f712-761a8e0f67c5"), "Create permission for Requests", "Requests", "Create" },
                    { new Guid("ad9f7b20-9e25-1242-f227-1f088c551bc5"), "Run permission for Payroll", "Payroll", "Run" },
                    { new Guid("bcff9126-260c-323e-a69d-939a113aeb60"), "ViewSettlement permission for Employees", "Employees", "ViewSettlement" },
                    { new Guid("c1ccefe3-e5e0-8425-0130-f1d87aab5d29"), "ManageRoles permission for Settings", "Settings", "ManageRoles" },
                    { new Guid("cb27bdfb-cfb9-772f-0881-08415e5f84eb"), "Approve permission for Requests", "Requests", "Approve" },
                    { new Guid("daeb1770-1622-ae41-3025-0f6681bb0a84"), "Edit permission for Requests", "Requests", "Edit" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: new Guid("0f993be1-195a-6619-1bbf-cc315e959199"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: new Guid("39c0b264-41d0-5918-be11-9df84ef7ecc2"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: new Guid("44d16d33-4958-01c6-27d7-424942af81d7"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: new Guid("476a9d1a-393e-4dde-9f96-42a58884bc7b"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: new Guid("82b38544-a228-6dc2-3325-e13086acb022"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: new Guid("8a257e8f-c0b2-69e6-65d6-b27368690e32"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: new Guid("9cd46e60-f870-bddf-f712-761a8e0f67c5"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: new Guid("ad9f7b20-9e25-1242-f227-1f088c551bc5"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: new Guid("bcff9126-260c-323e-a69d-939a113aeb60"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: new Guid("c1ccefe3-e5e0-8425-0130-f1d87aab5d29"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: new Guid("cb27bdfb-cfb9-772f-0881-08415e5f84eb"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: new Guid("daeb1770-1622-ae41-3025-0f6681bb0a84"));

            migrationBuilder.DropColumn(
                name: "ResetTokenExpiresAt",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ResetTokenHash",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ResetTokenPurpose",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "users");
        }
    }
}
