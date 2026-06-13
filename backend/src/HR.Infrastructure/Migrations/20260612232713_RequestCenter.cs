using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RequestCenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "engine_attendance_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_engine_attendance_records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_leave_balances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    EntitledDays = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    UsedDays = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    CarriedForwardDays = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
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
                    table.PrimaryKey("PK_engine_leave_balances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TitleAr = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    TitleEn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    BodyAr = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    BodyEn = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Link = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_engine_notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_request_types",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DescriptionAr = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DescriptionEn = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    FormDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrintTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaveTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_engine_request_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_request_impact_mappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AffectsLeaveBalance = table.Column<bool>(type: "boolean", nullable: false),
                    AffectsAttendance = table.Column<bool>(type: "boolean", nullable: false),
                    AffectsPayroll = table.Column<bool>(type: "boolean", nullable: false),
                    AffectsExpenses = table.Column<bool>(type: "boolean", nullable: false),
                    AffectsLoans = table.Column<bool>(type: "boolean", nullable: false),
                    CreatesLoanRecord = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresFinanceApproval = table.Column<bool>(type: "boolean", nullable: false),
                    AffectsTimeline = table.Column<bool>(type: "boolean", nullable: false),
                    AffectsAudit = table.Column<bool>(type: "boolean", nullable: false),
                    NotifiesManager = table.Column<bool>(type: "boolean", nullable: false),
                    GeneratesDocument = table.Column<bool>(type: "boolean", nullable: false),
                    ExtraJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_request_impact_mappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_request_impact_mappings_engine_request_types_Request~",
                        column: x => x.RequestTypeId,
                        principalTable: "engine_request_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_request_instances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    FormSubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrentStepOrder = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    GeneratedDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaveTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DaysCount = table.Column<decimal>(type: "numeric(7,2)", nullable: true),
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
                    table.PrimaryKey("PK_engine_request_instances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_request_instances_engine_request_types_RequestTypeId",
                        column: x => x.RequestTypeId,
                        principalTable: "engine_request_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "engine_request_permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    PermissionCode = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_request_permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_request_permissions_engine_request_types_RequestType~",
                        column: x => x.RequestTypeId,
                        principalTable: "engine_request_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_request_approvals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepOrder = table.Column<int>(type: "integer", nullable: false),
                    StepNameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StepNameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ApproverType = table.Column<int>(type: "integer", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_request_approvals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_request_approvals_engine_request_instances_RequestIn~",
                        column: x => x.RequestInstanceId,
                        principalTable: "engine_request_instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_request_status_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: true),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoteAr = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    NoteEn = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_request_status_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_request_status_history_engine_request_instances_Requ~",
                        column: x => x.RequestInstanceId,
                        principalTable: "engine_request_instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_engine_attendance_records_TenantId_EmployeeId_Date",
                table: "engine_attendance_records",
                columns: new[] { "TenantId", "EmployeeId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_leave_balances_TenantId_EmployeeId_LeaveTypeId_Year",
                table: "engine_leave_balances",
                columns: new[] { "TenantId", "EmployeeId", "LeaveTypeId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_notifications_TenantId_UserId_IsRead",
                table: "engine_notifications",
                columns: new[] { "TenantId", "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_request_approvals_AssignedToUserId",
                table: "engine_request_approvals",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_request_approvals_RequestInstanceId_StepOrder",
                table: "engine_request_approvals",
                columns: new[] { "RequestInstanceId", "StepOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_request_impact_mappings_RequestTypeId",
                table: "engine_request_impact_mappings",
                column: "RequestTypeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_request_instances_RequestTypeId",
                table: "engine_request_instances",
                column: "RequestTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_request_instances_TenantId_EmployeeId",
                table: "engine_request_instances",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_request_instances_TenantId_RequestNumber",
                table: "engine_request_instances",
                columns: new[] { "TenantId", "RequestNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_request_instances_TenantId_Status",
                table: "engine_request_instances",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_request_permissions_RequestTypeId",
                table: "engine_request_permissions",
                column: "RequestTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_request_status_history_RequestInstanceId",
                table: "engine_request_status_history",
                column: "RequestInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_request_types_TenantId_Code",
                table: "engine_request_types",
                columns: new[] { "TenantId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "engine_attendance_records");

            migrationBuilder.DropTable(
                name: "engine_leave_balances");

            migrationBuilder.DropTable(
                name: "engine_notifications");

            migrationBuilder.DropTable(
                name: "engine_request_approvals");

            migrationBuilder.DropTable(
                name: "engine_request_impact_mappings");

            migrationBuilder.DropTable(
                name: "engine_request_permissions");

            migrationBuilder.DropTable(
                name: "engine_request_status_history");

            migrationBuilder.DropTable(
                name: "engine_request_instances");

            migrationBuilder.DropTable(
                name: "engine_request_types");
        }
    }
}
