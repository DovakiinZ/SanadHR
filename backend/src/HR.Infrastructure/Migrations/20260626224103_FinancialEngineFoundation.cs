using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FinancialEngineFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "engine_finance_formula_functions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ParametersCsv = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpressionText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ExpressionAstJson = table.Column<string>(type: "jsonb", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_engine_finance_formula_functions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_finance_ledger_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceModule = table.Column<int>(type: "integer", nullable: false),
                    ComponentCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    ReferenceType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    PayrollRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    ReversesEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    PostedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_engine_finance_ledger_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_finance_rule_sets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrentVersionId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_engine_finance_rule_sets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_payroll_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Scope = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrentVersionId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_engine_payroll_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_payroll_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RunNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    PayrollDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollDefinitionVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleSetVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    EmployeeCount = table.Column<int>(type: "integer", nullable: false),
                    GrossTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DeductionTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NetTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CalculationVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_engine_payroll_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_finance_rule_set_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_engine_finance_rule_set_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_finance_rule_set_versions_engine_finance_rule_sets_R~",
                        column: x => x.RuleSetId,
                        principalTable: "engine_finance_rule_sets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_payroll_definition_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Frequency = table.Column<int>(type: "integer", nullable: false),
                    EmployeeFilterJson = table.Column<string>(type: "jsonb", nullable: true),
                    CycleConfigJson = table.Column<string>(type: "jsonb", nullable: true),
                    PaymentMethodId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkingCalendarId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovalWorkflowId = table.Column<Guid>(type: "uuid", nullable: true),
                    RuleSetVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_engine_payroll_definition_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_payroll_definition_versions_engine_payroll_definitio~",
                        column: x => x.PayrollDefinitionId,
                        principalTable: "engine_payroll_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_payroll_run_transitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromState = table.Column<int>(type: "integer", nullable: false),
                    ToState = table.Column<int>(type: "integer", nullable: false),
                    At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_engine_payroll_run_transitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_payroll_run_transitions_engine_payroll_runs_PayrollR~",
                        column: x => x.PayrollRunId,
                        principalTable: "engine_payroll_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_finance_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    ConditionText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ConditionAstJson = table.Column<string>(type: "jsonb", nullable: true),
                    ExpressionText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ExpressionAstJson = table.Column<string>(type: "jsonb", nullable: true),
                    OutputComponentCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
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
                    table.PrimaryKey("PK_engine_finance_rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_finance_rules_engine_finance_rule_set_versions_RuleS~",
                        column: x => x.RuleSetVersionId,
                        principalTable: "engine_finance_rule_set_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_engine_finance_formula_functions_TenantId",
                table: "engine_finance_formula_functions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_finance_formula_functions_TenantId_Name",
                table: "engine_finance_formula_functions",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_finance_ledger_entries_PayrollRunId",
                table: "engine_finance_ledger_entries",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_finance_ledger_entries_ReferenceType_ReferenceId",
                table: "engine_finance_ledger_entries",
                columns: new[] { "ReferenceType", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_finance_ledger_entries_ReversesEntryId",
                table: "engine_finance_ledger_entries",
                column: "ReversesEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_finance_ledger_entries_SourceModule",
                table: "engine_finance_ledger_entries",
                column: "SourceModule");

            migrationBuilder.CreateIndex(
                name: "IX_engine_finance_ledger_entries_TenantId",
                table: "engine_finance_ledger_entries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_finance_ledger_entries_TenantId_EmployeeId_Currency",
                table: "engine_finance_ledger_entries",
                columns: new[] { "TenantId", "EmployeeId", "Currency" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_finance_ledger_entries_TenantId_EntryNumber",
                table: "engine_finance_ledger_entries",
                columns: new[] { "TenantId", "EntryNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_finance_rule_set_versions_RuleSetId_VersionNumber",
                table: "engine_finance_rule_set_versions",
                columns: new[] { "RuleSetId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_finance_rule_set_versions_TenantId",
                table: "engine_finance_rule_set_versions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_finance_rule_sets_TenantId",
                table: "engine_finance_rule_sets",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_finance_rule_sets_TenantId_Code",
                table: "engine_finance_rule_sets",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_finance_rules_RuleSetVersionId_Code",
                table: "engine_finance_rules",
                columns: new[] { "RuleSetVersionId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_finance_rules_TenantId",
                table: "engine_finance_rules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_definition_versions_PayrollDefinitionId_Vers~",
                table: "engine_payroll_definition_versions",
                columns: new[] { "PayrollDefinitionId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_definition_versions_RuleSetVersionId",
                table: "engine_payroll_definition_versions",
                column: "RuleSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_definition_versions_TenantId",
                table: "engine_payroll_definition_versions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_definitions_Status",
                table: "engine_payroll_definitions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_definitions_TenantId",
                table: "engine_payroll_definitions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_definitions_TenantId_Code",
                table: "engine_payroll_definitions",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_run_transitions_PayrollRunId",
                table: "engine_payroll_run_transitions",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_run_transitions_TenantId",
                table: "engine_payroll_run_transitions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_runs_PayrollDefinitionId",
                table: "engine_payroll_runs",
                column: "PayrollDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_runs_State",
                table: "engine_payroll_runs",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_runs_TenantId",
                table: "engine_payroll_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_runs_TenantId_RunNumber",
                table: "engine_payroll_runs",
                columns: new[] { "TenantId", "RunNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "engine_finance_formula_functions");

            migrationBuilder.DropTable(
                name: "engine_finance_ledger_entries");

            migrationBuilder.DropTable(
                name: "engine_finance_rules");

            migrationBuilder.DropTable(
                name: "engine_payroll_definition_versions");

            migrationBuilder.DropTable(
                name: "engine_payroll_run_transitions");

            migrationBuilder.DropTable(
                name: "engine_finance_rule_set_versions");

            migrationBuilder.DropTable(
                name: "engine_payroll_definitions");

            migrationBuilder.DropTable(
                name: "engine_payroll_runs");

            migrationBuilder.DropTable(
                name: "engine_finance_rule_sets");
        }
    }
}
