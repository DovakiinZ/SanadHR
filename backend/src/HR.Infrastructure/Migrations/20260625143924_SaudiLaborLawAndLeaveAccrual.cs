using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SaudiLaborLawAndLeaveAccrual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "engine_leave_balance_transactions",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractEndDate",
                table: "employees",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContractTermType",
                table: "employees",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "employee_termination_settlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    HireDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TerminationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Scenario = table.Column<int>(type: "integer", nullable: false),
                    ContractTermType = table.Column<int>(type: "integer", nullable: false),
                    MonthlyWage = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DailyWage = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ServiceYears = table.Column<decimal>(type: "numeric(10,4)", nullable: false),
                    EffectiveServiceDays = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    UnpaidLeaveDays = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    GratuityAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Article77Award = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NoticeCompensation = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalAward = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ComputedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ComputedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_employee_termination_settlements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "employee_termination_settlement_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TerminationSettlementId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabelEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LabelAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ArticleRef = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_employee_termination_settlement_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_employee_termination_settlement_items_employee_termination_~",
                        column: x => x.TerminationSettlementId,
                        principalTable: "employee_termination_settlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "Id", "Description", "Module", "Name" },
                values: new object[] { new Guid("dadab2f8-4231-905a-ed9c-102e7a76e1fc"), "Terminate permission for Employees", "Employees", "Terminate" });

            migrationBuilder.CreateIndex(
                name: "IX_employee_termination_settlement_items_TenantId",
                table: "employee_termination_settlement_items",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_employee_termination_settlement_items_TerminationSettlement~",
                table: "employee_termination_settlement_items",
                column: "TerminationSettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_employee_termination_settlements_TenantId",
                table: "employee_termination_settlements",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_employee_termination_settlements_TenantId_EmployeeId",
                table: "employee_termination_settlements",
                columns: new[] { "TenantId", "EmployeeId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employee_termination_settlement_items");

            migrationBuilder.DropTable(
                name: "employee_termination_settlements");

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: new Guid("dadab2f8-4231-905a-ed9c-102e7a76e1fc"));

            migrationBuilder.DropColumn(
                name: "Type",
                table: "engine_leave_balance_transactions");

            migrationBuilder.DropColumn(
                name: "ContractEndDate",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "ContractTermType",
                table: "employees");
        }
    }
}
