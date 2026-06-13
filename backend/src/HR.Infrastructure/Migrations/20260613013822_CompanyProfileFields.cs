using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompanyProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "engine_company_profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "engine_company_profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "engine_company_profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "engine_company_profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "engine_company_profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "engine_company_profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "engine_company_profiles",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "engine_company_profiles");

            migrationBuilder.DropColumn(
                name: "City",
                table: "engine_company_profiles");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "engine_company_profiles");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "engine_company_profiles");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "engine_company_profiles");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "engine_company_profiles");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "engine_company_profiles");
        }
    }
}
