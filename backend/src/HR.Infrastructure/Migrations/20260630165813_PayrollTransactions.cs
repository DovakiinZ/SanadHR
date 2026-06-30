using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PayrollTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "engine_payroll_transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TargetPeriodYear = table.Column<int>(type: "integer", nullable: true),
                    TargetPeriodMonth = table.Column<int>(type: "integer", nullable: true),
                    IsRecurring = table.Column<bool>(type: "boolean", nullable: false),
                    RecurrenceEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AttachmentFileId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceModule = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    ReferenceType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StatusReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PayrollRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    PostedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PostedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LedgerEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReversesTransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReversalReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_engine_payroll_transactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_transactions_ReferenceType_ReferenceId",
                table: "engine_payroll_transactions",
                columns: new[] { "ReferenceType", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_transactions_ReversesTransactionId",
                table: "engine_payroll_transactions",
                column: "ReversesTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_transactions_TenantId",
                table: "engine_payroll_transactions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_transactions_TenantId_EmployeeId",
                table: "engine_payroll_transactions",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_transactions_TenantId_Kind_Status",
                table: "engine_payroll_transactions",
                columns: new[] { "TenantId", "Kind", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_payroll_transactions_TenantId_TargetPeriodYear_Targe~",
                table: "engine_payroll_transactions",
                columns: new[] { "TenantId", "TargetPeriodYear", "TargetPeriodMonth" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "engine_payroll_transactions");
        }
    }
}
