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
            // Finding 1 fix: correct column defaults that were wrong in the initial AddColumn calls
            // (DayBasis=0 is invalid — enum starts at CalendarMonth=1; CutoffDay=0 is invalid — must be 1-31)
            migrationBuilder.AlterColumn<int>(
                name: "CutoffDay",
                table: "engine_payroll_definition_versions",
                type: "integer",
                nullable: false,
                defaultValue: 27,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "DayBasis",
                table: "engine_payroll_definition_versions",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "CarryToNextPeriod",
                table: "engine_payroll_definition_versions",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            // Backfill: fix any rows that got the bad defaults from the initial migration
            migrationBuilder.Sql(@"
UPDATE engine_payroll_definition_versions
SET ""CutoffDay"" = 27
WHERE ""CutoffDay"" = 0;");

            migrationBuilder.Sql(@"
UPDATE engine_payroll_definition_versions
SET ""DayBasis"" = 1
WHERE ""DayBasis"" = 0;");

            // Finding 2 fix: add FK from engine_payroll_run_population to engine_payroll_runs
            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_run_population_PayrollRunId",
                table: "engine_payroll_run_population",
                column: "PayrollRunId");

            migrationBuilder.AddForeignKey(
                name: "FK_engine_payroll_run_population_engine_payroll_runs_PayrollRu~",
                table: "engine_payroll_run_population",
                column: "PayrollRunId",
                principalTable: "engine_payroll_runs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
    )::text
WHERE ""SelectionScopeJson"" IS NULL;");

            migrationBuilder.Sql(@"
UPDATE engine_payroll_definition_versions
SET ""SelectionScopeJson"" = jsonb_set(""SelectionScopeJson""::jsonb, '{mode}', '""All""')::text
WHERE (""SelectionScopeJson""::jsonb -> 'include') = '[]'::jsonb
  AND (""SelectionScopeJson""::jsonb -> 'includeEmployeeIds') = '[]'::jsonb;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_engine_payroll_run_population_engine_payroll_runs_PayrollRu~",
                table: "engine_payroll_run_population");

            migrationBuilder.DropIndex(
                name: "IX_engine_payroll_run_population_PayrollRunId",
                table: "engine_payroll_run_population");

            migrationBuilder.AlterColumn<bool>(
                name: "CarryToNextPeriod",
                table: "engine_payroll_definition_versions",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "DayBasis",
                table: "engine_payroll_definition_versions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "CutoffDay",
                table: "engine_payroll_definition_versions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 27);
        }
    }
}
