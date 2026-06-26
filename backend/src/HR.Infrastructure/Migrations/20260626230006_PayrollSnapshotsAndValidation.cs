using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PayrollSnapshotsAndValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ValidatedAt",
                table: "engine_payroll_runs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidationResultJson",
                table: "engine_payroll_runs",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "engine_payroll_payslips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EmployeeName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    GrossEarnings = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FactsJson = table.Column<string>(type: "jsonb", nullable: true),
                    ComponentsJson = table.Column<string>(type: "jsonb", nullable: true),
                    WarningsJson = table.Column<string>(type: "jsonb", nullable: true),
                    LedgerPosted = table.Column<bool>(type: "boolean", nullable: false),
                    LedgerPostedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_engine_payroll_payslips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_payroll_payslips_engine_payroll_runs_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalTable: "engine_payroll_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_payslips_EmployeeId",
                table: "engine_payroll_payslips",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_payslips_PayrollRunId_EmployeeId",
                table: "engine_payroll_payslips",
                columns: new[] { "PayrollRunId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_payslips_TenantId",
                table: "engine_payroll_payslips",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "engine_payroll_payslips");

            migrationBuilder.DropColumn(
                name: "ValidatedAt",
                table: "engine_payroll_runs");

            migrationBuilder.DropColumn(
                name: "ValidationResultJson",
                table: "engine_payroll_runs");
        }
    }
}
