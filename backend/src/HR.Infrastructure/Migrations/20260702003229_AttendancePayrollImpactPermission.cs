using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AttendancePayrollImpactPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "Id", "Description", "Module", "Name" },
                values: new object[] { new Guid("b6f22f3d-5aed-3838-070b-4f0233ed4426"), "Create permission for Attendance.PayrollImpact", "Attendance.PayrollImpact", "Create" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: new Guid("b6f22f3d-5aed-3838-070b-4f0233ed4426"));
        }
    }
}
