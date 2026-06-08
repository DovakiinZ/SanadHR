using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DepartmentOrgFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "departments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "departments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CostCenterId",
                table: "departments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeputyManagerId",
                table: "departments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "departments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_departments_BranchId",
                table: "departments",
                column: "BranchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_departments_BranchId",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "CostCenterId",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "DeputyManagerId",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "departments");
        }
    }
}
