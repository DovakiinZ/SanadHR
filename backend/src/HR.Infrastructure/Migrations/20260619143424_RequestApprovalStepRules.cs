using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RequestApprovalStepRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanDelegate",
                table: "engine_request_approvals",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanReject",
                table: "engine_request_approvals",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanReturn",
                table: "engine_request_approvals",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOptional",
                table: "engine_request_approvals",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanDelegate",
                table: "engine_request_approvals");

            migrationBuilder.DropColumn(
                name: "CanReject",
                table: "engine_request_approvals");

            migrationBuilder.DropColumn(
                name: "CanReturn",
                table: "engine_request_approvals");

            migrationBuilder.DropColumn(
                name: "IsOptional",
                table: "engine_request_approvals");
        }
    }
}
