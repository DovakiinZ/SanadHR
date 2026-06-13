using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DocumentPlatform : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "BodyTemplate",
                table: "engine_document_templates",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                table: "engine_document_templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LayoutJson",
                table: "engine_document_templates",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PageTemplateId",
                table: "engine_document_templates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CeoSignatureUrl",
                table: "engine_company_profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HrSignatureUrl",
                table: "engine_company_profiles",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "engine_page_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    HeaderConfig = table.Column<string>(type: "jsonb", nullable: true),
                    FooterConfig = table.Column<string>(type: "jsonb", nullable: true),
                    Margins = table.Column<string>(type: "jsonb", nullable: true),
                    Watermark = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("PK_engine_page_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_request_template_mappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    TriggerEvent = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_engine_request_template_mappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_request_template_mappings_engine_document_templates_~",
                        column: x => x.DocumentTemplateId,
                        principalTable: "engine_document_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_engine_request_template_mappings_engine_request_types_Reque~",
                        column: x => x.RequestTypeId,
                        principalTable: "engine_request_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_engine_document_templates_PageTemplateId",
                table: "engine_document_templates",
                column: "PageTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_page_templates_TenantId_Code",
                table: "engine_page_templates",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_request_template_mappings_DocumentTemplateId",
                table: "engine_request_template_mappings",
                column: "DocumentTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_request_template_mappings_RequestTypeId",
                table: "engine_request_template_mappings",
                column: "RequestTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_request_template_mappings_TenantId_RequestTypeId_Tri~",
                table: "engine_request_template_mappings",
                columns: new[] { "TenantId", "RequestTypeId", "TriggerEvent" });

            migrationBuilder.AddForeignKey(
                name: "FK_engine_document_templates_engine_page_templates_PageTemplat~",
                table: "engine_document_templates",
                column: "PageTemplateId",
                principalTable: "engine_page_templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_engine_document_templates_engine_page_templates_PageTemplat~",
                table: "engine_document_templates");

            migrationBuilder.DropTable(
                name: "engine_page_templates");

            migrationBuilder.DropTable(
                name: "engine_request_template_mappings");

            migrationBuilder.DropIndex(
                name: "IX_engine_document_templates_PageTemplateId",
                table: "engine_document_templates");

            migrationBuilder.DropColumn(
                name: "IsSystem",
                table: "engine_document_templates");

            migrationBuilder.DropColumn(
                name: "LayoutJson",
                table: "engine_document_templates");

            migrationBuilder.DropColumn(
                name: "PageTemplateId",
                table: "engine_document_templates");

            migrationBuilder.DropColumn(
                name: "CeoSignatureUrl",
                table: "engine_company_profiles");

            migrationBuilder.DropColumn(
                name: "HrSignatureUrl",
                table: "engine_company_profiles");

            migrationBuilder.AlterColumn<string>(
                name: "BodyTemplate",
                table: "engine_document_templates",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
