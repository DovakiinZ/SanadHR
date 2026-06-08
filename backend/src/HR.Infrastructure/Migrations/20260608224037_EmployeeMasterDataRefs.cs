using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EmployeeMasterDataRefs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractType",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "JobTitleAr",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "employees");

            migrationBuilder.AddColumn<Guid>(
                name: "ContractTypeId",
                table: "employees",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "JobTitleId",
                table: "employees",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NationalityId",
                table: "employees",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_employees_ContractTypeId",
                table: "employees",
                column: "ContractTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_employees_JobTitleId",
                table: "employees",
                column: "JobTitleId");

            migrationBuilder.CreateIndex(
                name: "IX_employees_NationalityId",
                table: "employees",
                column: "NationalityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_employees_ContractTypeId",
                table: "employees");

            migrationBuilder.DropIndex(
                name: "IX_employees_JobTitleId",
                table: "employees");

            migrationBuilder.DropIndex(
                name: "IX_employees_NationalityId",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "ContractTypeId",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "JobTitleId",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "NationalityId",
                table: "employees");

            migrationBuilder.AddColumn<int>(
                name: "ContractType",
                table: "employees",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "employees",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobTitleAr",
                table: "employees",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "employees",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
