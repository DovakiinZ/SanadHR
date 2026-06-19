using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LeaveRecordsEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "engine_leave_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DaysCount = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    TargetScope = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    JobTitleId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AttachmentUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AssignedCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_engine_leave_assignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_leave_audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    DetailsAr = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DetailsEn = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_engine_leave_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_leave_balance_transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    LeaveRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    Delta = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_engine_leave_balance_transactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_leave_cancellations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RestoredDays = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    CanceledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CanceledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_engine_leave_cancellations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_leave_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DaysCount = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    AffectsBalance = table.Column<bool>(type: "boolean", nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    RequestInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaveAssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AttachmentUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GeneratedDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CanceledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CanceledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_engine_leave_records", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "Id", "Description", "Module", "Name" },
                values: new object[,]
                {
                    { new Guid("0642f021-62b0-5def-9523-4b3778526383"), "Cancel permission for Leaves", "Leaves", "Cancel" },
                    { new Guid("31697a96-f55e-673a-5d47-d4a7fddaa40a"), "Delete permission for Leaves", "Leaves", "Delete" },
                    { new Guid("32fa5250-c204-9af8-f859-5f0d33a3f1f6"), "Assign permission for Leaves", "Leaves", "Assign" },
                    { new Guid("9923f61d-119c-592f-2c5d-ecf023d5eeee"), "Edit permission for Leaves", "Leaves", "Edit" },
                    { new Guid("c5199fa4-430b-e984-556e-763882d2b8d7"), "Create permission for Leaves", "Leaves", "Create" },
                    { new Guid("d94bd01a-1e0e-4c2f-1322-a70e9fc47b08"), "View permission for Leaves", "Leaves", "View" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_engine_leave_assignments_TenantId",
                table: "engine_leave_assignments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_leave_audit_logs_TenantId",
                table: "engine_leave_audit_logs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_leave_audit_logs_TenantId_EmployeeId",
                table: "engine_leave_audit_logs",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_leave_audit_logs_TenantId_LeaveRecordId",
                table: "engine_leave_audit_logs",
                columns: new[] { "TenantId", "LeaveRecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_leave_balance_transactions_LeaveRecordId",
                table: "engine_leave_balance_transactions",
                column: "LeaveRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_leave_balance_transactions_TenantId",
                table: "engine_leave_balance_transactions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_leave_balance_transactions_TenantId_EmployeeId_Leave~",
                table: "engine_leave_balance_transactions",
                columns: new[] { "TenantId", "EmployeeId", "LeaveTypeId", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_leave_cancellations_LeaveRecordId",
                table: "engine_leave_cancellations",
                column: "LeaveRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_leave_cancellations_TenantId",
                table: "engine_leave_cancellations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_leave_records_TenantId",
                table: "engine_leave_records",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_leave_records_TenantId_EmployeeId",
                table: "engine_leave_records",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_leave_records_TenantId_RecordNumber",
                table: "engine_leave_records",
                columns: new[] { "TenantId", "RecordNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_leave_records_TenantId_RequestInstanceId",
                table: "engine_leave_records",
                columns: new[] { "TenantId", "RequestInstanceId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "engine_leave_assignments");

            migrationBuilder.DropTable(
                name: "engine_leave_audit_logs");

            migrationBuilder.DropTable(
                name: "engine_leave_balance_transactions");

            migrationBuilder.DropTable(
                name: "engine_leave_cancellations");

            migrationBuilder.DropTable(
                name: "engine_leave_records");

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: new Guid("0642f021-62b0-5def-9523-4b3778526383"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: new Guid("31697a96-f55e-673a-5d47-d4a7fddaa40a"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: new Guid("32fa5250-c204-9af8-f859-5f0d33a3f1f6"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: new Guid("9923f61d-119c-592f-2c5d-ecf023d5eeee"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: new Guid("c5199fa4-430b-e984-556e-763882d2b8d7"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: new Guid("d94bd01a-1e0e-4c2f-1322-a70e9fc47b08"));
        }
    }
}
