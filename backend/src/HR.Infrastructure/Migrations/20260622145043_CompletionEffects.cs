using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompletionEffects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "engine_completion_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationMs = table.Column<int>(type: "integer", nullable: true),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FinalApproverUserId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_engine_completion_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_completion_effects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletionRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationMs = table.Column<int>(type: "integer", nullable: true),
                    ExecutorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExecutorVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TargetEntityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TargetRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResultSummary = table.Column<string>(type: "jsonb", nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_engine_completion_effects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_completion_effects_engine_completion_runs_Completion~",
                        column: x => x.CompletionRunId,
                        principalTable: "engine_completion_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_engine_completion_effects_CompletionRunId",
                table: "engine_completion_effects",
                column: "CompletionRunId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_completion_effects_RequestInstanceId",
                table: "engine_completion_effects",
                column: "RequestInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_completion_effects_Status",
                table: "engine_completion_effects",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_engine_completion_effects_TenantId",
                table: "engine_completion_effects",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_completion_runs_RequestInstanceId",
                table: "engine_completion_runs",
                column: "RequestInstanceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_completion_runs_Status",
                table: "engine_completion_runs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_engine_completion_runs_TenantId",
                table: "engine_completion_runs",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "engine_completion_effects");

            migrationBuilder.DropTable(
                name: "engine_completion_runs");
        }
    }
}
