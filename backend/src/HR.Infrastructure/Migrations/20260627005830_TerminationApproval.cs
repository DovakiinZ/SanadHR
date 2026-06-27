using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TerminationApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "employee_termination_settlements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentStep",
                table: "employee_termination_settlements",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "DocumentFileId",
                table: "employee_termination_settlements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ExpenseId",
                table: "employee_termination_settlements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "employee_termination_settlements",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "employee_termination_settlements",
                type: "integer",
                nullable: false,
                defaultValue: 3); // existing immediate settlements are already Approved

            migrationBuilder.CreateTable(
                name: "employee_termination_approval_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TerminationSettlementId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepOrder = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    ApproverUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_employee_termination_approval_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_employee_termination_approval_steps_employee_termination_se~",
                        column: x => x.TerminationSettlementId,
                        principalTable: "employee_termination_settlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_employee_termination_settlements_TenantId_Status",
                table: "employee_termination_settlements",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_employee_termination_approval_steps_TenantId",
                table: "employee_termination_approval_steps",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_employee_termination_approval_steps_TerminationSettlementId",
                table: "employee_termination_approval_steps",
                column: "TerminationSettlementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employee_termination_approval_steps");

            migrationBuilder.DropIndex(
                name: "IX_employee_termination_settlements_TenantId_Status",
                table: "employee_termination_settlements");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "employee_termination_settlements");

            migrationBuilder.DropColumn(
                name: "CurrentStep",
                table: "employee_termination_settlements");

            migrationBuilder.DropColumn(
                name: "DocumentFileId",
                table: "employee_termination_settlements");

            migrationBuilder.DropColumn(
                name: "ExpenseId",
                table: "employee_termination_settlements");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "employee_termination_settlements");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "employee_termination_settlements");
        }
    }
}
