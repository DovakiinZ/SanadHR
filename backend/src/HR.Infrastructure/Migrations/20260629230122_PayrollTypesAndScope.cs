using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PayrollTypesAndScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "engine_payroll_definitions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CalcSettingsJson",
                table: "engine_payroll_definition_versions",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CarryToNextPeriod",
                table: "engine_payroll_definition_versions",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosingDate",
                table: "engine_payroll_definition_versions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CutoffDay",
                table: "engine_payroll_definition_versions",
                type: "integer",
                nullable: false,
                defaultValue: 27);

            migrationBuilder.AddColumn<int>(
                name: "DayBasis",
                table: "engine_payroll_definition_versions",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultExportFormatId",
                table: "engine_payroll_definition_versions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveFrom",
                table: "engine_payroll_definition_versions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveTo",
                table: "engine_payroll_definition_versions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSimulation",
                table: "engine_payroll_definition_versions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "engine_payroll_definition_versions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethodScopeJson",
                table: "engine_payroll_definition_versions",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectionScopeJson",
                table: "engine_payroll_definition_versions",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "engine_payroll_run_population",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EmployeeName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    JobTitleId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentMethodId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsIncluded = table.Column<bool>(type: "boolean", nullable: false),
                    ExclusionReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
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
                    table.PrimaryKey("PK_engine_payroll_run_population", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_payroll_run_population_engine_payroll_runs_PayrollRu~",
                        column: x => x.PayrollRunId,
                        principalTable: "engine_payroll_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "Id", "Description", "Module", "Name" },
                values: new object[] { new Guid("143f32fa-5cdb-aa94-0014-37fb0090bbe6"), "Configure permission for Payroll", "Payroll", "Configure" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_run_population_PayrollRunId",
                table: "engine_payroll_run_population",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_run_population_TenantId_PayrollRunId",
                table: "engine_payroll_run_population",
                columns: new[] { "TenantId", "PayrollRunId" });

            // Backfill: convert EmployeeFilterJson → SelectionScopeJson for existing rows
            migrationBuilder.Sql(@"
UPDATE engine_payroll_definition_versions
SET ""SelectionScopeJson"" = jsonb_build_object(
        'mode', 'Criteria',
        'include', CASE
            WHEN ""EmployeeFilterJson"" IS NOT NULL
                 AND (""EmployeeFilterJson""::jsonb ? 'departmentIds')
            THEN jsonb_build_array(jsonb_build_object(
                'dimension', 'Department',
                'valueIds', (""EmployeeFilterJson""::jsonb -> 'departmentIds')))
            ELSE '[]'::jsonb END,
        'exclude', '[]'::jsonb,
        'includeEmployeeIds', COALESCE(""EmployeeFilterJson""::jsonb -> 'employeeIds', '[]'::jsonb),
        'excludeEmployeeIds', '[]'::jsonb
    )
WHERE ""SelectionScopeJson"" IS NULL;");

            migrationBuilder.Sql(@"
UPDATE engine_payroll_definition_versions
SET ""SelectionScopeJson"" = jsonb_set(""SelectionScopeJson"", '{mode}', '""All""')
WHERE (""SelectionScopeJson""::jsonb -> 'include') = '[]'::jsonb
  AND (""SelectionScopeJson""::jsonb -> 'includeEmployeeIds') = '[]'::jsonb;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "engine_payroll_run_population");

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: new Guid("143f32fa-5cdb-aa94-0014-37fb0090bbe6"));

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "engine_payroll_definitions");

            migrationBuilder.DropColumn(
                name: "CalcSettingsJson",
                table: "engine_payroll_definition_versions");

            migrationBuilder.DropColumn(
                name: "CarryToNextPeriod",
                table: "engine_payroll_definition_versions");

            migrationBuilder.DropColumn(
                name: "ClosingDate",
                table: "engine_payroll_definition_versions");

            migrationBuilder.DropColumn(
                name: "CutoffDay",
                table: "engine_payroll_definition_versions");

            migrationBuilder.DropColumn(
                name: "DayBasis",
                table: "engine_payroll_definition_versions");

            migrationBuilder.DropColumn(
                name: "DefaultExportFormatId",
                table: "engine_payroll_definition_versions");

            migrationBuilder.DropColumn(
                name: "EffectiveFrom",
                table: "engine_payroll_definition_versions");

            migrationBuilder.DropColumn(
                name: "EffectiveTo",
                table: "engine_payroll_definition_versions");

            migrationBuilder.DropColumn(
                name: "IsSimulation",
                table: "engine_payroll_definition_versions");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "engine_payroll_definition_versions");

            migrationBuilder.DropColumn(
                name: "PaymentMethodScopeJson",
                table: "engine_payroll_definition_versions");

            migrationBuilder.DropColumn(
                name: "SelectionScopeJson",
                table: "engine_payroll_definition_versions");
        }
    }
}
