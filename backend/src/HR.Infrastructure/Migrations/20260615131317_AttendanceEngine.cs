using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AttendanceEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BreakMinutes",
                table: "engine_attendance_records",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsFlexible",
                table: "engine_attendance_records",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LateMinutes",
                table: "engine_attendance_records",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OvertimeMinutes",
                table: "engine_attendance_records",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RequiredMinutes",
                table: "engine_attendance_records",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ShiftId",
                table: "engine_attendance_records",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShortageMinutes",
                table: "engine_attendance_records",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkedMinutes",
                table: "engine_attendance_records",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "attendance_audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendanceRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Action = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    DetailsAr = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DetailsEn = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DetailsJson = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_attendance_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "attendance_corrections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendanceRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OldCheckIn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OldCheckOut = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NewCheckIn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NewCheckOut = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RequestInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_attendance_corrections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "attendance_policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NameAr = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DefaultGraceMinutes = table.Column<int>(type: "integer", nullable: false),
                    RoundingMinutes = table.Column<int>(type: "integer", nullable: false),
                    AutoMarkAbsent = table.Column<bool>(type: "boolean", nullable: false),
                    CountOvertime = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_attendance_policies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "attendance_punches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendanceRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    PunchTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
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
                    table.PrimaryKey("PK_attendance_punches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "attendance_shifts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NameAr = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    RequiredMinutes = table.Column<int>(type: "integer", nullable: false),
                    BreakMinutes = table.Column<int>(type: "integer", nullable: false),
                    GraceBeforeStartMinutes = table.Column<int>(type: "integer", nullable: false),
                    GraceAfterStartMinutes = table.Column<int>(type: "integer", nullable: false),
                    GraceBeforeEndMinutes = table.Column<int>(type: "integer", nullable: false),
                    GraceAfterEndMinutes = table.Column<int>(type: "integer", nullable: false),
                    OvertimeAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    LateDeductionEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsFlexible = table.Column<bool>(type: "boolean", nullable: false),
                    WeekendDays = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
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
                    table.PrimaryKey("PK_attendance_shifts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "attendance_shift_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    JobTitleId = table.Column<Guid>(type: "uuid", nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_attendance_shift_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_attendance_shift_assignments_attendance_shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "attendance_shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_audit_logs_TenantId",
                table: "attendance_audit_logs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_audit_logs_TenantId_AttendanceRecordId",
                table: "attendance_audit_logs",
                columns: new[] { "TenantId", "AttendanceRecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_audit_logs_TenantId_EmployeeId_Date",
                table: "attendance_audit_logs",
                columns: new[] { "TenantId", "EmployeeId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_corrections_AttendanceRecordId",
                table: "attendance_corrections",
                column: "AttendanceRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_corrections_TenantId",
                table: "attendance_corrections",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_policies_TenantId",
                table: "attendance_policies",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_punches_AttendanceRecordId",
                table: "attendance_punches",
                column: "AttendanceRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_punches_TenantId",
                table: "attendance_punches",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_punches_TenantId_EmployeeId_PunchTime",
                table: "attendance_punches",
                columns: new[] { "TenantId", "EmployeeId", "PunchTime" });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_shift_assignments_ShiftId",
                table: "attendance_shift_assignments",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_shift_assignments_TenantId",
                table: "attendance_shift_assignments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_shift_assignments_TenantId_BranchId",
                table: "attendance_shift_assignments",
                columns: new[] { "TenantId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_shift_assignments_TenantId_DepartmentId",
                table: "attendance_shift_assignments",
                columns: new[] { "TenantId", "DepartmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_shift_assignments_TenantId_EmployeeId",
                table: "attendance_shift_assignments",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_shift_assignments_TenantId_JobTitleId",
                table: "attendance_shift_assignments",
                columns: new[] { "TenantId", "JobTitleId" });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_shifts_TenantId",
                table: "attendance_shifts",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attendance_audit_logs");

            migrationBuilder.DropTable(
                name: "attendance_corrections");

            migrationBuilder.DropTable(
                name: "attendance_policies");

            migrationBuilder.DropTable(
                name: "attendance_punches");

            migrationBuilder.DropTable(
                name: "attendance_shift_assignments");

            migrationBuilder.DropTable(
                name: "attendance_shifts");

            migrationBuilder.DropColumn(
                name: "BreakMinutes",
                table: "engine_attendance_records");

            migrationBuilder.DropColumn(
                name: "IsFlexible",
                table: "engine_attendance_records");

            migrationBuilder.DropColumn(
                name: "LateMinutes",
                table: "engine_attendance_records");

            migrationBuilder.DropColumn(
                name: "OvertimeMinutes",
                table: "engine_attendance_records");

            migrationBuilder.DropColumn(
                name: "RequiredMinutes",
                table: "engine_attendance_records");

            migrationBuilder.DropColumn(
                name: "ShiftId",
                table: "engine_attendance_records");

            migrationBuilder.DropColumn(
                name: "ShortageMinutes",
                table: "engine_attendance_records");

            migrationBuilder.DropColumn(
                name: "WorkedMinutes",
                table: "engine_attendance_records");
        }
    }
}
