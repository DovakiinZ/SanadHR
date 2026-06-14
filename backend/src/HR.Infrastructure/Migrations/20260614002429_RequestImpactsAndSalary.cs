using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RequestImpactsAndSalary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CreatesAttendancePunch",
                table: "engine_request_impact_mappings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CreatesExpenseRecord",
                table: "engine_request_impact_mappings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "GosiNumber",
                table: "engine_company_profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GosiRate",
                table: "engine_company_profiles",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MolNumber",
                table: "engine_company_profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckIn",
                table: "engine_attendance_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckOut",
                table: "engine_attendance_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "employee_additions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdditionTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_employee_additions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_employee_additions_employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employee_deductions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeductionTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_employee_deductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_employee_deductions_employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_expenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpenseCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReceiptUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequestInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_engine_expenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_loans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Principal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    InstallmentMonths = table.Column<int>(type: "integer", nullable: false),
                    MonthlyInstallment = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequestInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_engine_loans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_loan_installments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanId = table.Column<Guid>(type: "uuid", nullable: false),
                    DueMonth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Paid = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_loan_installments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_loan_installments_engine_loans_LoanId",
                        column: x => x.LoanId,
                        principalTable: "engine_loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_employee_additions_EmployeeId_AdditionTypeId",
                table: "employee_additions",
                columns: new[] { "EmployeeId", "AdditionTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employee_additions_TenantId",
                table: "employee_additions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_employee_deductions_EmployeeId_DeductionTypeId",
                table: "employee_deductions",
                columns: new[] { "EmployeeId", "DeductionTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employee_deductions_TenantId",
                table: "employee_deductions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_expenses_TenantId_EmployeeId",
                table: "engine_expenses",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_loan_installments_LoanId",
                table: "engine_loan_installments",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_loans_TenantId_EmployeeId",
                table: "engine_loans",
                columns: new[] { "TenantId", "EmployeeId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employee_additions");

            migrationBuilder.DropTable(
                name: "employee_deductions");

            migrationBuilder.DropTable(
                name: "engine_expenses");

            migrationBuilder.DropTable(
                name: "engine_loan_installments");

            migrationBuilder.DropTable(
                name: "engine_loans");

            migrationBuilder.DropColumn(
                name: "CreatesAttendancePunch",
                table: "engine_request_impact_mappings");

            migrationBuilder.DropColumn(
                name: "CreatesExpenseRecord",
                table: "engine_request_impact_mappings");

            migrationBuilder.DropColumn(
                name: "GosiNumber",
                table: "engine_company_profiles");

            migrationBuilder.DropColumn(
                name: "GosiRate",
                table: "engine_company_profiles");

            migrationBuilder.DropColumn(
                name: "MolNumber",
                table: "engine_company_profiles");

            migrationBuilder.DropColumn(
                name: "CheckIn",
                table: "engine_attendance_records");

            migrationBuilder.DropColumn(
                name: "CheckOut",
                table: "engine_attendance_records");
        }
    }
}
