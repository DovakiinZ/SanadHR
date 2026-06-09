using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EmployeeOrgPayrollFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BankId",
                table: "employees",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardProvider",
                table: "employees",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "employees",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhone",
                table: "employees",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmploymentTypeId",
                table: "employees",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LeavePolicyId",
                table: "employees",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentMethodId",
                table: "employees",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PayrollGroupId",
                table: "employees",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalaryCardNumber",
                table: "employees",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkLocationId",
                table: "employees",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "branches",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GeofenceRadiusMeters",
                table: "branches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "branches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "branches",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "branches",
                type: "double precision",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "employee_allowances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllowanceTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_allowances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_employee_allowances_employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_employee_allowances_EmployeeId_AllowanceTypeId",
                table: "employee_allowances",
                columns: new[] { "EmployeeId", "AllowanceTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employee_allowances_TenantId",
                table: "employee_allowances",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employee_allowances");

            migrationBuilder.DropColumn(
                name: "BankId",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "CardProvider",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "EmploymentTypeId",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "LeavePolicyId",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "PayrollGroupId",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "SalaryCardNumber",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "WorkLocationId",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "branches");

            migrationBuilder.DropColumn(
                name: "GeofenceRadiusMeters",
                table: "branches");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "branches");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "branches");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "branches");
        }
    }
}
