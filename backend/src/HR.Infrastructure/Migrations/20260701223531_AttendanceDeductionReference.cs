using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AttendanceDeductionReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "engine_payroll_transaction_attendance_refs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendanceRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PenaltyKind = table.Column<int>(type: "integer", nullable: false),
                    Minutes = table.Column<int>(type: "integer", nullable: false),
                    Days = table.Column<int>(type: "integer", nullable: false),
                    AmountContribution = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_engine_payroll_transaction_attendance_refs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_payroll_transaction_attendance_refs_engine_payroll_t~",
                        column: x => x.PayrollTransactionId,
                        principalTable: "engine_payroll_transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_transaction_attendance_refs_AttendanceRecord~",
                table: "engine_payroll_transaction_attendance_refs",
                column: "AttendanceRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_transaction_attendance_refs_PayrollTransacti~",
                table: "engine_payroll_transaction_attendance_refs",
                column: "PayrollTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_transaction_attendance_refs_TenantId",
                table: "engine_payroll_transaction_attendance_refs",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "engine_payroll_transaction_attendance_refs");
        }
    }
}
