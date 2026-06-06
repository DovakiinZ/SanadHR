using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    OldValues = table.Column<string>(type: "jsonb", nullable: true),
                    NewValues = table.Column<string>(type: "jsonb", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "branches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsMainBranch = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_branches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "company_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CompanyNameAr = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    Website = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Timezone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DateFormat = table.Column<string>(type: "text", nullable: true),
                    WorkingDaysPerWeek = table.Column<int>(type: "integer", nullable: false),
                    WeekStartDay = table.Column<string>(type: "text", nullable: true),
                    AnnualLeaveDays = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_company_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ParentDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ManagerId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_departments_departments_ParentDepartmentId",
                        column: x => x.ParentDepartmentId,
                        principalTable: "departments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FirstNameAr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastNameAr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NationalId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Nationality = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ContractType = table.Column<int>(type: "integer", nullable: false),
                    HireDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TerminationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JobTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    JobTitleAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ManagerId = table.Column<Guid>(type: "uuid", nullable: true),
                    BasicSalary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    BankName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BankAccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Iban = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PhotoUrl = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_audit_configurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    TrackedFields = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_audit_configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_audit_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    OldValues = table.Column<string>(type: "jsonb", nullable: true),
                    NewValues = table.Column<string>(type: "jsonb", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_engine_audit_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_automation_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_engine_automation_rules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_calendar_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CalendarType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WorkWeekDays = table.Column<string>(type: "jsonb", nullable: false),
                    Holidays = table.Column<string>(type: "jsonb", nullable: true),
                    WorkDayStart = table.Column<TimeSpan>(type: "interval", nullable: false),
                    WorkDayEnd = table.Column<TimeSpan>(type: "interval", nullable: false),
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
                    table.PrimaryKey("PK_engine_calendar_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_company_branding",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ElementType = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    Configuration = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("PK_engine_company_branding", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_company_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StampUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CommercialRegistration = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VatNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NationalAddress = table.Column<string>(type: "jsonb", nullable: true),
                    ContactInfo = table.Column<string>(type: "jsonb", nullable: true),
                    FiscalYearStart = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DefaultCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DefaultLanguage = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    TimeZone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_engine_company_profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_cost_centers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ParentCostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_engine_cost_centers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_cost_centers_engine_cost_centers_ParentCostCenterId",
                        column: x => x.ParentCostCenterId,
                        principalTable: "engine_cost_centers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "engine_dashboard_categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_engine_dashboard_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_dashboard_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PreviewImageUrl = table.Column<string>(type: "text", nullable: true),
                    DefaultScope = table.Column<int>(type: "integer", nullable: false),
                    LayoutConfiguration = table.Column<string>(type: "jsonb", nullable: false),
                    WidgetConfiguration = table.Column<string>(type: "jsonb", nullable: false),
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
                    table.PrimaryKey("PK_engine_dashboard_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_document_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OutputFormat = table.Column<int>(type: "integer", nullable: false),
                    BodyTemplate = table.Column<string>(type: "text", nullable: false),
                    HeaderTemplate = table.Column<string>(type: "text", nullable: true),
                    FooterTemplate = table.Column<string>(type: "text", nullable: true),
                    StyleSheet = table.Column<string>(type: "text", nullable: true),
                    UseBranding = table.Column<bool>(type: "boolean", nullable: false),
                    PageSettings = table.Column<string>(type: "jsonb", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_engine_document_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_employee_reporting_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManagerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportingType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_engine_employee_reporting_lines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_fiscal_periods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    PeriodNumber = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_engine_fiscal_periods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_form_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_engine_form_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_grades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    MinSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MaxSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Benefits = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("PK_engine_grades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_metadata_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_engine_metadata_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_object_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TableName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_engine_object_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_org_graph_layouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    GraphType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LayoutData = table.Column<string>(type: "jsonb", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_engine_org_graph_layouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_org_nodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ParentNodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    PositionX = table.Column<int>(type: "integer", nullable: false),
                    PositionY = table.Column<int>(type: "integer", nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_engine_org_nodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_org_nodes_engine_org_nodes_ParentNodeId",
                        column: x => x.ParentNodeId,
                        principalTable: "engine_org_nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "engine_permission_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_engine_permission_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_positions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParentPositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    JobDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MinGrade = table.Column<int>(type: "integer", nullable: true),
                    MaxGrade = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_engine_positions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_positions_engine_positions_ParentPositionId",
                        column: x => x.ParentPositionId,
                        principalTable: "engine_positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "engine_report_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ReportType = table.Column<int>(type: "integer", nullable: false),
                    PrimaryObjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Configuration = table.Column<string>(type: "jsonb", nullable: false),
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
                    table.PrimaryKey("PK_engine_report_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_timeline_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DescriptionEn = table.Column<string>(type: "text", nullable: true),
                    DescriptionAr = table.Column<string>(type: "text", nullable: true),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_engine_timeline_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_timeline_subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_engine_timeline_subscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_token_categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_token_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_user_permission_overrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsGranted = table.Column<bool>(type: "boolean", nullable: false),
                    Scope = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_engine_user_permission_overrides", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_user_permission_scopes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeType = table.Column<int>(type: "integer", nullable: false),
                    ScopeValue = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_user_permission_scopes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_widget_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    WidgetType = table.Column<int>(type: "integer", nullable: false),
                    Icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DefaultConfiguration = table.Column<string>(type: "jsonb", nullable: true),
                    DefaultWidth = table.Column<int>(type: "integer", nullable: false),
                    DefaultHeight = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_engine_widget_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_workflow_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TriggerEntityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_engine_workflow_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hr_tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssigneeId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedById = table.Column<Guid>(type: "uuid", nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Tags = table.Column<string>(type: "jsonb", nullable: true),
                    Progress = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_hr_tasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsSystemRole = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tenant_master_data_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Icon = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsSystemDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("PK_tenant_master_data_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CompanyNameAr = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    Domain = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SubscriptionPlan = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SubscriptionExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "engine_automation_actions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AutomationRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    Configuration = table.Column<string>(type: "jsonb", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_automation_actions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_automation_actions_engine_automation_rules_Automatio~",
                        column: x => x.AutomationRuleId,
                        principalTable: "engine_automation_rules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_automation_conditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AutomationRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Field = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Operator = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LogicalOperator = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_automation_conditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_automation_conditions_engine_automation_rules_Automa~",
                        column: x => x.AutomationRuleId,
                        principalTable: "engine_automation_rules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_automation_execution_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AutomationRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    TriggerEventType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_automation_execution_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_automation_execution_logs_engine_automation_rules_Au~",
                        column: x => x.AutomationRuleId,
                        principalTable: "engine_automation_rules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_automation_triggers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AutomationRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Configuration = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_automation_triggers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_automation_triggers_engine_automation_rules_Automati~",
                        column: x => x.AutomationRuleId,
                        principalTable: "engine_automation_rules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_dashboard_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    Scope = table.Column<int>(type: "integer", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LayoutConfiguration = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("PK_engine_dashboard_definitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_dashboard_definitions_engine_dashboard_categories_Ca~",
                        column: x => x.CategoryId,
                        principalTable: "engine_dashboard_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_engine_dashboard_definitions_engine_dashboard_templates_Tem~",
                        column: x => x.TemplateId,
                        principalTable: "engine_dashboard_templates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "engine_document_template_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DefaultValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_document_template_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_document_template_tokens_engine_document_templates_D~",
                        column: x => x.DocumentTemplateId,
                        principalTable: "engine_document_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_document_template_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    BodyTemplate = table.Column<string>(type: "text", nullable: false),
                    HeaderTemplate = table.Column<string>(type: "text", nullable: true),
                    FooterTemplate = table.Column<string>(type: "text", nullable: true),
                    ChangeNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_document_template_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_document_template_versions_engine_document_templates~",
                        column: x => x.DocumentTemplateId,
                        principalTable: "engine_document_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_document_workflow_links",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    TriggerType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TriggerEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    TriggerCondition = table.Column<string>(type: "jsonb", nullable: true),
                    AutoGenerate = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_document_workflow_links", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_document_workflow_links_engine_document_templates_Do~",
                        column: x => x.DocumentTemplateId,
                        principalTable: "engine_document_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_generated_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OutputFormat = table.Column<int>(type: "integer", nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    TokenValues = table.Column<string>(type: "jsonb", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GeneratedById = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_engine_generated_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_generated_documents_engine_document_templates_Docume~",
                        column: x => x.DocumentTemplateId,
                        principalTable: "engine_document_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "engine_form_fields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FormDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FieldType = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    SectionName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Placeholder = table.Column<string>(type: "text", nullable: true),
                    DefaultValue = table.Column<string>(type: "text", nullable: true),
                    ValidationRules = table.Column<string>(type: "jsonb", nullable: true),
                    Options = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_form_fields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_form_fields_engine_form_definitions_FormDefinitionId",
                        column: x => x.FormDefinitionId,
                        principalTable: "engine_form_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_form_submissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FormDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedById = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_engine_form_submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_form_submissions_engine_form_definitions_FormDefinit~",
                        column: x => x.FormDefinitionId,
                        principalTable: "engine_form_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_metadata_fields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MetadataDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FieldType = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    DefaultValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_metadata_fields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_metadata_fields_engine_metadata_definitions_Metadata~",
                        column: x => x.MetadataDefinitionId,
                        principalTable: "engine_metadata_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_metadata_values",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MetadataDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Values = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("PK_engine_metadata_values", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_metadata_values_engine_metadata_definitions_Metadata~",
                        column: x => x.MetadataDefinitionId,
                        principalTable: "engine_metadata_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_object_fields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FieldType = table.Column<int>(type: "integer", nullable: false),
                    IsFilterable = table.Column<bool>(type: "boolean", nullable: false),
                    IsSortable = table.Column<bool>(type: "boolean", nullable: false),
                    IsSearchable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_object_fields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_object_fields_engine_object_definitions_ObjectDefini~",
                        column: x => x.ObjectDefinitionId,
                        principalTable: "engine_object_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_object_permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionType = table.Column<int>(type: "integer", nullable: false),
                    PermissionCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_object_permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_object_permissions_engine_object_definitions_ObjectD~",
                        column: x => x.ObjectDefinitionId,
                        principalTable: "engine_object_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_object_relationships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceObjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetObjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationType = table.Column<int>(type: "integer", nullable: false),
                    ForeignKeyField = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_object_relationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_object_relationships_engine_object_definitions_Sourc~",
                        column: x => x.SourceObjectId,
                        principalTable: "engine_object_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_engine_object_relationships_engine_object_definitions_Targe~",
                        column: x => x.TargetObjectId,
                        principalTable: "engine_object_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "engine_org_edges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_org_edges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_org_edges_engine_org_nodes_SourceNodeId",
                        column: x => x.SourceNodeId,
                        principalTable: "engine_org_nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_engine_org_edges_engine_org_nodes_TargetNodeId",
                        column: x => x.TargetNodeId,
                        principalTable: "engine_org_nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_permission_template_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Scope = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_permission_template_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_permission_template_items_engine_permission_template~",
                        column: x => x.PermissionTemplateId,
                        principalTable: "engine_permission_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPermissionTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignedBy = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_UserPermissionTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPermissionTemplates_engine_permission_templates_Permiss~",
                        column: x => x.PermissionTemplateId,
                        principalTable: "engine_permission_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_report_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReportType = table.Column<int>(type: "integer", nullable: false),
                    Scope = table.Column<int>(type: "integer", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: true),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrimaryObjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_engine_report_definitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_report_definitions_engine_report_templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "engine_report_templates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "engine_token_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DataType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ResolverKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_token_definitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_token_definitions_engine_token_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "engine_token_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_widget_data_sources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WidgetDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    ObjectDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    QueryTemplate = table.Column<string>(type: "jsonb", nullable: true),
                    ApiEndpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Aggregation = table.Column<int>(type: "integer", nullable: true),
                    AggregationField = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GroupByField = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DateRangeField = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RefreshIntervalSeconds = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_widget_data_sources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_widget_data_sources_engine_widget_definitions_Widget~",
                        column: x => x.WidgetDefinitionId,
                        principalTable: "engine_widget_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_widget_permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WidgetDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_widget_permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_widget_permissions_engine_widget_definitions_WidgetD~",
                        column: x => x.WidgetDefinitionId,
                        principalTable: "engine_widget_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_workflow_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Configuration = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_workflow_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_workflow_versions_engine_workflow_definitions_Workfl~",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "engine_workflow_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hr_task_activities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Details = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_task_activities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hr_task_activities_hr_tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "hr_tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hr_task_checklists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_task_checklists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hr_task_checklists_hr_tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "hr_tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hr_task_comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_task_comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hr_task_comments_hr_tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "hr_tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_role_permissions_permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_role_permissions_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_permissions_permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_permissions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_roles_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_dashboard_shares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DashboardDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SharedWithUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SharedWithDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    SharedWithRoleId = table.Column<Guid>(type: "uuid", nullable: true),
                    CanEdit = table.Column<bool>(type: "boolean", nullable: false),
                    SharedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SharedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_dashboard_shares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_dashboard_shares_engine_dashboard_definitions_Dashbo~",
                        column: x => x.DashboardDefinitionId,
                        principalTable: "engine_dashboard_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_dashboard_widgets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DashboardDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    WidgetDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    WidgetType = table.Column<int>(type: "integer", nullable: false),
                    ObjectDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TitleEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TitleAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Configuration = table.Column<string>(type: "jsonb", nullable: true),
                    DataSourceConfig = table.Column<string>(type: "jsonb", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_dashboard_widgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_dashboard_widgets_engine_dashboard_definitions_Dashb~",
                        column: x => x.DashboardDefinitionId,
                        principalTable: "engine_dashboard_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_engine_dashboard_widgets_engine_widget_definitions_WidgetDe~",
                        column: x => x.WidgetDefinitionId,
                        principalTable: "engine_widget_definitions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "engine_form_submission_values",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FormSubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FormFieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true),
                    FileUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_form_submission_values", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_form_submission_values_engine_form_fields_FormFieldId",
                        column: x => x.FormFieldId,
                        principalTable: "engine_form_fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_engine_form_submission_values_engine_form_submissions_FormS~",
                        column: x => x.FormSubmissionId,
                        principalTable: "engine_form_submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_metadata_options",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MetadataFieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LabelEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LabelAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_metadata_options", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_metadata_options_engine_metadata_fields_MetadataFiel~",
                        column: x => x.MetadataFieldId,
                        principalTable: "engine_metadata_fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_report_fields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldType = table.Column<int>(type: "integer", nullable: false),
                    ObjectDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    FieldCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayNameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayNameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Aggregation = table.Column<int>(type: "integer", nullable: true),
                    CalculationExpression = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FormatPattern = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_report_fields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_report_fields_engine_report_definitions_ReportDefini~",
                        column: x => x.ReportDefinitionId,
                        principalTable: "engine_report_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_report_filters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Operator = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ValueTo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LogicalOperator = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsParameter = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_report_filters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_report_filters_engine_report_definitions_ReportDefin~",
                        column: x => x.ReportDefinitionId,
                        principalTable: "engine_report_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_report_groupings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_report_groupings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_report_groupings_engine_report_definitions_ReportDef~",
                        column: x => x.ReportDefinitionId,
                        principalTable: "engine_report_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_report_relationships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceObjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetObjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinField = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    JoinType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_report_relationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_report_relationships_engine_report_definitions_Repor~",
                        column: x => x.ReportDefinitionId,
                        principalTable: "engine_report_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_report_schedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Frequency = table.Column<int>(type: "integer", nullable: false),
                    CronExpression = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExportFormat = table.Column<int>(type: "integer", nullable: false),
                    Recipients = table.Column<string>(type: "jsonb", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastRunAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextRunAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_report_schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_report_schedules_engine_report_definitions_ReportDef~",
                        column: x => x.ReportDefinitionId,
                        principalTable: "engine_report_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_report_shares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SharedWithUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SharedWithDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    SharedWithRoleId = table.Column<Guid>(type: "uuid", nullable: true),
                    CanEdit = table.Column<bool>(type: "boolean", nullable: false),
                    SharedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_report_shares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_report_shares_engine_report_definitions_ReportDefini~",
                        column: x => x.ReportDefinitionId,
                        principalTable: "engine_report_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_report_sortings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_report_sortings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_report_sortings_engine_report_definitions_ReportDefi~",
                        column: x => x.ReportDefinitionId,
                        principalTable: "engine_report_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_workflow_instances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_engine_workflow_instances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_workflow_instances_engine_workflow_definitions_Workf~",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "engine_workflow_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_engine_workflow_instances_engine_workflow_versions_Workflow~",
                        column: x => x.WorkflowVersionId,
                        principalTable: "engine_workflow_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_workflow_nodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeType = table.Column<int>(type: "integer", nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Configuration = table.Column<string>(type: "jsonb", nullable: true),
                    PositionX = table.Column<int>(type: "integer", nullable: false),
                    PositionY = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_workflow_nodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_workflow_nodes_engine_workflow_versions_WorkflowVers~",
                        column: x => x.WorkflowVersionId,
                        principalTable: "engine_workflow_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_workflow_simulations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    InputData = table.Column<string>(type: "jsonb", nullable: false),
                    Result = table.Column<string>(type: "jsonb", nullable: false),
                    SimulatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SimulatedById = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_engine_workflow_simulations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_workflow_simulations_engine_workflow_versions_Workfl~",
                        column: x => x.WorkflowVersionId,
                        principalTable: "engine_workflow_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_widget_drilldowns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DashboardWidgetId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrilldownType = table.Column<int>(type: "integer", nullable: false),
                    TargetDashboardId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetRoute = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FilterMapping = table.Column<string>(type: "jsonb", nullable: true),
                    LabelEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LabelAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_widget_drilldowns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_widget_drilldowns_engine_dashboard_widgets_Dashboard~",
                        column: x => x.DashboardWidgetId,
                        principalTable: "engine_dashboard_widgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_widget_filters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DashboardWidgetId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Operator = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_widget_filters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_widget_filters_engine_dashboard_widgets_DashboardWid~",
                        column: x => x.DashboardWidgetId,
                        principalTable: "engine_dashboard_widgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_widget_layouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DashboardWidgetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Column = table.Column<int>(type: "integer", nullable: false),
                    Row = table.Column<int>(type: "integer", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_widget_layouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_widget_layouts_engine_dashboard_widgets_DashboardWid~",
                        column: x => x.DashboardWidgetId,
                        principalTable: "engine_dashboard_widgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_workflow_actions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Configuration = table.Column<string>(type: "jsonb", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_workflow_actions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_workflow_actions_engine_workflow_nodes_WorkflowNodeId",
                        column: x => x.WorkflowNodeId,
                        principalTable: "engine_workflow_nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_workflow_approver_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApproverType = table.Column<int>(type: "integer", nullable: false),
                    SpecificUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SpecificRoleId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_workflow_approver_rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_workflow_approver_rules_engine_workflow_nodes_Workfl~",
                        column: x => x.WorkflowNodeId,
                        principalTable: "engine_workflow_nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_workflow_conditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Field = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Operator = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LogicalOperator = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_workflow_conditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_workflow_conditions_engine_workflow_nodes_WorkflowNo~",
                        column: x => x.WorkflowNodeId,
                        principalTable: "engine_workflow_nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_workflow_dynamic_approvers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApproverStrategy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SpecificEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChainLevel = table.Column<int>(type: "integer", nullable: false),
                    FallbackStrategy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FallbackEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_workflow_dynamic_approvers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_workflow_dynamic_approvers_engine_workflow_nodes_Wor~",
                        column: x => x.WorkflowNodeId,
                        principalTable: "engine_workflow_nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_workflow_dynamic_conditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConditionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FieldPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Operator = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    LogicalOperator = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_workflow_dynamic_conditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_workflow_dynamic_conditions_engine_workflow_nodes_Wo~",
                        column: x => x.WorkflowNodeId,
                        principalTable: "engine_workflow_nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_workflow_edges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Condition = table.Column<string>(type: "jsonb", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_workflow_edges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_workflow_edges_engine_workflow_nodes_SourceNodeId",
                        column: x => x.SourceNodeId,
                        principalTable: "engine_workflow_nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_engine_workflow_edges_engine_workflow_nodes_TargetNodeId",
                        column: x => x.TargetNodeId,
                        principalTable: "engine_workflow_nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_engine_workflow_edges_engine_workflow_versions_WorkflowVers~",
                        column: x => x.WorkflowVersionId,
                        principalTable: "engine_workflow_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "engine_workflow_instance_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ActionTakenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    ActionType = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engine_workflow_instance_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_engine_workflow_instance_steps_engine_workflow_instances_Wo~",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "engine_workflow_instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_engine_workflow_instance_steps_engine_workflow_nodes_Workfl~",
                        column: x => x.WorkflowNodeId,
                        principalTable: "engine_workflow_nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "Id", "Description", "Module", "Name" },
                values: new object[,]
                {
                    { new Guid("002d68ec-3eb5-1997-551e-ac44d2dd6b2b"), "View permission for Dashboards", "Dashboards", "View" },
                    { new Guid("01f8f46d-2e40-5609-57b7-e86a07f7002a"), "Delete permission for Platform.Objects", "Platform.Objects", "Delete" },
                    { new Guid("03160966-f659-dc41-f780-cd5e40081b80"), "View permission for Platform.Reports", "Platform.Reports", "View" },
                    { new Guid("03489813-cd04-f369-e7c7-2f4b0bd9a5d5"), "Edit permission for Platform.Metadata", "Platform.Metadata", "Edit" },
                    { new Guid("03d78bdd-4ed5-61aa-899f-70e8150cb06a"), "Delete permission for Platform.CompanyConfig", "Platform.CompanyConfig", "Delete" },
                    { new Guid("090f0bed-b403-2198-9b10-ac5f773f33d6"), "View permission for Platform.Admin", "Platform.Admin", "View" },
                    { new Guid("0bd6605a-c110-7266-cf34-02b5e694f143"), "Create permission for ESS", "ESS", "Create" },
                    { new Guid("0e04a2eb-cd3a-aa49-6139-c2d1a962cd31"), "Delete permission for Attendance", "Attendance", "Delete" },
                    { new Guid("0fcc0ddf-08c5-c07c-0696-9b833f596c62"), "Delete permission for Platform.Metadata", "Platform.Metadata", "Delete" },
                    { new Guid("1432d3ed-2149-2b97-3d4d-45e062d80894"), "Generate permission for Platform.Documents", "Platform.Documents", "Generate" },
                    { new Guid("15bf460a-4e7f-494b-cc84-4bcf67bca253"), "Create permission for Platform.Workflows", "Platform.Workflows", "Create" },
                    { new Guid("173136ab-3c69-7518-3b38-a149bd7d73e5"), "Edit permission for Platform.Objects", "Platform.Objects", "Edit" },
                    { new Guid("17aa69c9-2a1f-1a9d-b26a-197a5ad923c5"), "View permission for Tasks", "Tasks", "View" },
                    { new Guid("184722f7-fbe6-dcc7-9b3c-7d7ebbb2b9e0"), "Edit permission for Tasks", "Tasks", "Edit" },
                    { new Guid("1a852488-23f9-2eae-a624-1ccf2869557a"), "EditUsers permission for Identity", "Identity", "EditUsers" },
                    { new Guid("21019747-9fdf-6ed3-20ad-5ce1460f353d"), "Edit permission for Employees", "Employees", "Edit" },
                    { new Guid("23203c02-4588-c209-984c-9aa7dd926b7b"), "View permission for Platform.CompanyConfig", "Platform.CompanyConfig", "View" },
                    { new Guid("2a221e6c-9e9f-13cc-10dc-586beaa24742"), "Create permission for Platform.Dashboards", "Platform.Dashboards", "Create" },
                    { new Guid("2a68c9ee-0e54-4dd9-b93f-7a6d18f08969"), "View permission for Documents", "Documents", "View" },
                    { new Guid("2f40767f-e7c4-e5cb-1a2a-d7db1f9e4cd1"), "ViewUsers permission for Identity", "Identity", "ViewUsers" },
                    { new Guid("32158fe2-3a90-7a18-3aec-f6bc65bf71bf"), "View permission for Loans", "Loans", "View" },
                    { new Guid("3812b54e-bf6f-3a00-0f3e-ea5eddb9fc52"), "View permission for Branches", "Branches", "View" },
                    { new Guid("3b6628ef-a21f-f630-b1eb-2023a87a529a"), "DeleteRoles permission for Identity", "Identity", "DeleteRoles" },
                    { new Guid("3bcab4a4-8712-b9b1-b6f7-b6d9d39a9704"), "Delete permission for Branches", "Branches", "Delete" },
                    { new Guid("3c18ab69-6032-4582-2222-f368ea007d27"), "Delete permission for Platform.Documents", "Platform.Documents", "Delete" },
                    { new Guid("3c827ceb-97b9-21fa-f806-37280c704e14"), "Export permission for Attendance", "Attendance", "Export" },
                    { new Guid("3d67e012-f94f-c0e7-260f-2226f0161621"), "ViewRoles permission for Identity", "Identity", "ViewRoles" },
                    { new Guid("3ff5daf9-5e6a-0896-5c5e-47636e2e40b8"), "Edit permission for Expenses", "Expenses", "Edit" },
                    { new Guid("4093213a-861e-bd27-fd89-b68176b98641"), "View permission for Notifications", "Notifications", "View" },
                    { new Guid("4819499a-969d-ac41-36d1-ea2fa4951ee2"), "Create permission for Branches", "Branches", "Create" },
                    { new Guid("48e41226-2c2a-200d-298b-f048a5db0967"), "View permission for Platform.Forms", "Platform.Forms", "View" },
                    { new Guid("4a76d6fc-6880-92bf-59a8-49eb35800349"), "Approve permission for Loans", "Loans", "Approve" },
                    { new Guid("4b18ca57-d3b1-e795-938c-d9a4d31baa65"), "CreateRoles permission for Identity", "Identity", "CreateRoles" },
                    { new Guid("4b1e834d-31de-f339-a5ed-c9766846a041"), "Create permission for Platform.Permissions", "Platform.Permissions", "Create" },
                    { new Guid("4c009696-69a1-5ee0-f7bb-d94f97297fea"), "Delete permission for Platform.Dashboards", "Platform.Dashboards", "Delete" },
                    { new Guid("4c807e9e-4b80-d1c5-935d-56beb4a98571"), "Create permission for Platform.MasterData", "Platform.MasterData", "Create" },
                    { new Guid("4cd05e42-e4e1-7e67-abbb-487027005c43"), "Edit permission for Payroll", "Payroll", "Edit" },
                    { new Guid("4d6e4c69-8313-4a5b-2eac-1fa5c9e024c7"), "Edit permission for Workflows", "Workflows", "Edit" },
                    { new Guid("55f57330-0b7c-6bec-e9cb-8d0467579c6a"), "Assign permission for Tasks", "Tasks", "Assign" },
                    { new Guid("57d420a5-84d0-5c06-5992-b2af8bea19b5"), "Edit permission for Departments", "Departments", "Edit" },
                    { new Guid("59eef9f4-be2d-26c6-ba73-086963de228d"), "View permission for Expenses", "Expenses", "View" },
                    { new Guid("59f9c3d2-27a7-ccf4-860a-1fc1c9d65926"), "Create permission for Platform.OrgGraph", "Platform.OrgGraph", "Create" },
                    { new Guid("617e3c87-7d9c-9acf-f20f-ffcd3c9e77ef"), "Create permission for Documents", "Documents", "Create" },
                    { new Guid("67e8b1b5-b3f5-f7fd-ab43-08f8299e0dca"), "Edit permission for Platform.Dashboards", "Platform.Dashboards", "Edit" },
                    { new Guid("67ff7733-dcd7-f2a6-3754-d24cf49647c9"), "Export permission for Payroll", "Payroll", "Export" },
                    { new Guid("7227bdcc-42e6-39f8-869c-22926227570f"), "Create permission for Platform.Reports", "Platform.Reports", "Create" },
                    { new Guid("74a86e3b-0c09-ddee-8ccd-18171d40f81f"), "View permission for Employees", "Employees", "View" },
                    { new Guid("7557d4f5-7cc7-918c-9ef8-396eba6fad26"), "Delete permission for Expenses", "Expenses", "Delete" },
                    { new Guid("78b6433f-7a84-1339-b70d-92f6ff410415"), "Delete permission for Platform.Workflows", "Platform.Workflows", "Delete" },
                    { new Guid("79d21a25-44ab-671b-cd6b-5304ed9eaf93"), "Edit permission for Attendance", "Attendance", "Edit" },
                    { new Guid("7a4534e6-7684-036a-0d45-9f9833c6bacf"), "Edit permission for Platform.MasterData", "Platform.MasterData", "Edit" },
                    { new Guid("7ad053a8-cfa5-6397-3f2f-bb94a98a86ef"), "Edit permission for Platform.Permissions", "Platform.Permissions", "Edit" },
                    { new Guid("7b9abc24-be0a-cd74-e03d-4dfa91c32abe"), "DeleteUsers permission for Identity", "Identity", "DeleteUsers" },
                    { new Guid("8153addd-9a67-bf96-9aef-eaf15f2f6d64"), "Delete permission for Platform.MasterData", "Platform.MasterData", "Delete" },
                    { new Guid("8289ac71-457c-a9ef-f0fe-a15f4c6d4a38"), "Edit permission for Platform.Forms", "Platform.Forms", "Edit" },
                    { new Guid("83199211-1f45-9443-b053-a88332856a96"), "Delete permission for Platform.Reports", "Platform.Reports", "Delete" },
                    { new Guid("8364b05a-af7d-5be3-d6ea-6ed0c22a7fb6"), "Edit permission for ESS", "ESS", "Edit" },
                    { new Guid("8366f01d-4592-0628-1032-986895785a02"), "Edit permission for Notifications", "Notifications", "Edit" },
                    { new Guid("841daf4d-a793-4513-3206-ed2335e0c64d"), "Delete permission for Platform.Permissions", "Platform.Permissions", "Delete" },
                    { new Guid("88a74bb5-b554-0b8f-cb10-29797245531a"), "Create permission for Payroll", "Payroll", "Create" },
                    { new Guid("899afdb5-1784-8414-894d-7869c91b04a8"), "Create permission for Expenses", "Expenses", "Create" },
                    { new Guid("8a5e2a7d-6d34-bb72-5fc8-b8aeac9f024b"), "Delete permission for Employees", "Employees", "Delete" },
                    { new Guid("8a8836d2-25a3-cd37-086f-38b13ff3807b"), "Delete permission for Notifications", "Notifications", "Delete" },
                    { new Guid("8bd3d8e1-2b7d-4a02-9e9b-ba668234deef"), "View permission for Platform.Objects", "Platform.Objects", "View" },
                    { new Guid("8e78290c-c119-8766-fd06-c6958a28d0a3"), "View permission for ESS", "ESS", "View" },
                    { new Guid("8edf4fa3-47f3-cf36-8bd3-00560134f937"), "View permission for Payroll", "Payroll", "View" },
                    { new Guid("90b50042-aa1a-69da-f997-6bfab3d87efa"), "Delete permission for Departments", "Departments", "Delete" },
                    { new Guid("91d69067-e940-2d2b-264f-5b4c6cc87cb4"), "View permission for Platform.Dashboards", "Platform.Dashboards", "View" },
                    { new Guid("982cab26-0383-4d41-31ec-a8d80bab13b1"), "View permission for Reports", "Reports", "View" },
                    { new Guid("99c8eb38-7b00-d78d-cdc7-27f6911cdf38"), "EditRoles permission for Identity", "Identity", "EditRoles" },
                    { new Guid("9b19ef31-0158-58f8-bec2-e21bd373f0c3"), "View permission for Platform.OrgGraph", "Platform.OrgGraph", "View" },
                    { new Guid("9d52047b-d351-758a-649b-32814a6f3646"), "View permission for Departments", "Departments", "View" },
                    { new Guid("9ea29543-54ea-31ba-cea7-fdb75b94bb7a"), "Edit permission for Platform.CompanyConfig", "Platform.CompanyConfig", "Edit" },
                    { new Guid("9ff7ad13-82fc-c832-8e1b-c2dd7f06f49f"), "View permission for Platform.Metadata", "Platform.Metadata", "View" },
                    { new Guid("a0be04f5-accf-7627-2730-754e8f68f227"), "Create permission for Platform.Forms", "Platform.Forms", "Create" },
                    { new Guid("a65dbd0d-2981-c4d2-a076-8468de7598f7"), "Create permission for Platform.Objects", "Platform.Objects", "Create" },
                    { new Guid("a9a57065-3d93-b733-e1c3-048faf12635d"), "Export permission for Platform.Reports", "Platform.Reports", "Export" },
                    { new Guid("ae3c45eb-276b-704f-6ce4-637a60f4f56c"), "View permission for Platform.Permissions", "Platform.Permissions", "View" },
                    { new Guid("ae97dc16-dfdc-cf96-2db6-6a50eba41f42"), "Create permission for Tasks", "Tasks", "Create" },
                    { new Guid("af6c0312-c104-0ee9-f16c-f6c6db0b97b9"), "Delete permission for Payroll", "Payroll", "Delete" },
                    { new Guid("b14fe1d2-487a-8f40-4f3c-2f05fd458fb3"), "CreateUsers permission for Identity", "Identity", "CreateUsers" },
                    { new Guid("b975787f-beb4-1355-f87f-54ca0d570da3"), "Delete permission for Platform.Forms", "Platform.Forms", "Delete" },
                    { new Guid("b9ada49a-2e05-3249-c25c-aa50c21632e0"), "Edit permission for Platform.OrgGraph", "Platform.OrgGraph", "Edit" },
                    { new Guid("ba467d2a-839e-bead-0ab3-0c47af262768"), "Export permission for Employees", "Employees", "Export" },
                    { new Guid("bbef3a30-b9fa-af5e-fd5f-46563c8f962b"), "Create permission for Employees", "Employees", "Create" },
                    { new Guid("c031f745-9ecd-0777-02d9-22923c8e4e1b"), "Edit permission for Platform.Reports", "Platform.Reports", "Edit" },
                    { new Guid("c03b1aae-bb48-d620-ff4c-2613c58e2c64"), "Edit permission for Platform.Documents", "Platform.Documents", "Edit" },
                    { new Guid("c34e33a8-e40d-50c0-f396-7ddc3cfcd553"), "Create permission for Departments", "Departments", "Create" },
                    { new Guid("c48525fd-ccfb-a029-8a89-eb56ac688b97"), "View permission for Attendance", "Attendance", "View" },
                    { new Guid("c6eb3ad0-de58-5fee-dfb3-0a7dc3c2f0d3"), "Delete permission for Platform.OrgGraph", "Platform.OrgGraph", "Delete" },
                    { new Guid("c83439bb-42fc-097c-841b-d0ed3c8c9b04"), "View permission for Platform.Workflows", "Platform.Workflows", "View" },
                    { new Guid("ca3d8525-36e0-eb82-d413-00ab853de13c"), "Create permission for Platform.Documents", "Platform.Documents", "Create" },
                    { new Guid("cd985f1c-7845-e4c7-eab9-df327894c577"), "Edit permission for Documents", "Documents", "Edit" },
                    { new Guid("cf47875d-c961-6560-37e6-42e772e552dd"), "Export permission for Reports", "Reports", "Export" },
                    { new Guid("d03523ab-d91f-1151-3282-ae421c908c90"), "Edit permission for Settings", "Settings", "Edit" },
                    { new Guid("d11ddac3-80e7-cad5-de14-184c495d2a00"), "Delete permission for Documents", "Documents", "Delete" },
                    { new Guid("d1e9f894-cdbd-40fe-86aa-285ffaf10e2b"), "View permission for Workflows", "Workflows", "View" },
                    { new Guid("d21db3c1-2974-7bb2-13e9-94a90302d29a"), "Approve permission for Payroll", "Payroll", "Approve" },
                    { new Guid("d5334c4d-5887-fb29-fc4a-e55aa481a94c"), "Create permission for Loans", "Loans", "Create" },
                    { new Guid("d74f8fa6-199a-3c69-5070-6495fef9e84f"), "Create permission for Attendance", "Attendance", "Create" },
                    { new Guid("dbc47eab-ec6e-3ad4-43fe-9dedcfa51b43"), "Edit permission for Branches", "Branches", "Edit" },
                    { new Guid("de816cf5-5b04-07ee-622a-a3ebc89f287a"), "Delete permission for Tasks", "Tasks", "Delete" },
                    { new Guid("e5f7222a-cd14-dc46-ea56-a51846548960"), "View permission for Platform.Documents", "Platform.Documents", "View" },
                    { new Guid("e6588f48-cd06-e976-313e-324b16c1ab98"), "Create permission for Platform.Metadata", "Platform.Metadata", "Create" },
                    { new Guid("e6cd5fd2-fa4c-af2a-e7cf-309efd1acb75"), "Delete permission for Loans", "Loans", "Delete" },
                    { new Guid("e7686870-8d0f-7393-2294-a6404ed5f03a"), "Edit permission for Loans", "Loans", "Edit" },
                    { new Guid("e776e0a0-caf1-90c5-cf7c-ed624c14fffa"), "View permission for Settings", "Settings", "View" },
                    { new Guid("e80b428e-da12-e8db-38db-957d01480277"), "Create permission for Notifications", "Notifications", "Create" },
                    { new Guid("ea559e27-925b-9d76-0a11-0a29d945074d"), "Create permission for Platform.CompanyConfig", "Platform.CompanyConfig", "Create" },
                    { new Guid("eae74b33-ead2-4900-5a0e-7a40b89269c3"), "Create permission for Workflows", "Workflows", "Create" },
                    { new Guid("ee2e523b-a628-3e23-1591-2b30f176e27f"), "Edit permission for Platform.Workflows", "Platform.Workflows", "Edit" },
                    { new Guid("fb91dae9-1200-8974-1b8d-e9fc84f6cc8b"), "Delete permission for Workflows", "Workflows", "Delete" },
                    { new Guid("fd34dd17-e5bf-ab06-c0ad-569c3dde176d"), "View permission for Platform.MasterData", "Platform.MasterData", "View" },
                    { new Guid("fd718738-e174-cb8e-5fbc-06225744bb1c"), "Approve permission for Expenses", "Expenses", "Approve" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_EntityType_EntityId",
                table: "audit_logs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_TenantId",
                table: "audit_logs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_branches_TenantId",
                table: "branches",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_company_settings_TenantId",
                table: "company_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_departments_ParentDepartmentId",
                table: "departments",
                column: "ParentDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_departments_TenantId",
                table: "departments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_employees_TenantId",
                table: "employees",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_employees_TenantId_Email",
                table: "employees",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employees_TenantId_EmployeeNumber",
                table: "employees",
                columns: new[] { "TenantId", "EmployeeNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_audit_configurations_EntityType",
                table: "engine_audit_configurations",
                column: "EntityType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_audit_entries_EntityType_EntityId",
                table: "engine_audit_entries",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_audit_entries_TenantId",
                table: "engine_audit_entries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_audit_entries_Timestamp",
                table: "engine_audit_entries",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_engine_automation_actions_AutomationRuleId",
                table: "engine_automation_actions",
                column: "AutomationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_automation_conditions_AutomationRuleId",
                table: "engine_automation_conditions",
                column: "AutomationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_automation_execution_logs_AutomationRuleId",
                table: "engine_automation_execution_logs",
                column: "AutomationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_automation_execution_logs_EntityType_EntityId",
                table: "engine_automation_execution_logs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_automation_rules_TenantId",
                table: "engine_automation_rules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_automation_triggers_AutomationRuleId",
                table: "engine_automation_triggers",
                column: "AutomationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_calendar_settings_TenantId_Code",
                table: "engine_calendar_settings",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_company_branding_TenantId_ElementType",
                table: "engine_company_branding",
                columns: new[] { "TenantId", "ElementType" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_company_profiles_TenantId",
                table: "engine_company_profiles",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_cost_centers_ParentCostCenterId",
                table: "engine_cost_centers",
                column: "ParentCostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_cost_centers_TenantId_Code",
                table: "engine_cost_centers",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_dashboard_categories_TenantId_Code",
                table: "engine_dashboard_categories",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_dashboard_definitions_CategoryId",
                table: "engine_dashboard_definitions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_dashboard_definitions_TemplateId",
                table: "engine_dashboard_definitions",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_dashboard_definitions_TenantId_Code",
                table: "engine_dashboard_definitions",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_dashboard_shares_DashboardDefinitionId",
                table: "engine_dashboard_shares",
                column: "DashboardDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_dashboard_templates_TenantId_Code",
                table: "engine_dashboard_templates",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_dashboard_widgets_DashboardDefinitionId",
                table: "engine_dashboard_widgets",
                column: "DashboardDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_dashboard_widgets_WidgetDefinitionId",
                table: "engine_dashboard_widgets",
                column: "WidgetDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_document_template_tokens_DocumentTemplateId",
                table: "engine_document_template_tokens",
                column: "DocumentTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_document_template_versions_DocumentTemplateId",
                table: "engine_document_template_versions",
                column: "DocumentTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_document_templates_TenantId_Code",
                table: "engine_document_templates",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_document_workflow_links_DocumentTemplateId",
                table: "engine_document_workflow_links",
                column: "DocumentTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_employee_reporting_lines_TenantId_EmployeeId_Manager~",
                table: "engine_employee_reporting_lines",
                columns: new[] { "TenantId", "EmployeeId", "ManagerId", "ReportingType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_fiscal_periods_TenantId_Year_PeriodNumber",
                table: "engine_fiscal_periods",
                columns: new[] { "TenantId", "Year", "PeriodNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_form_definitions_TenantId_Code",
                table: "engine_form_definitions",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_form_fields_FormDefinitionId",
                table: "engine_form_fields",
                column: "FormDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_form_submission_values_FormFieldId",
                table: "engine_form_submission_values",
                column: "FormFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_form_submission_values_FormSubmissionId",
                table: "engine_form_submission_values",
                column: "FormSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_form_submissions_FormDefinitionId",
                table: "engine_form_submissions",
                column: "FormDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_form_submissions_TenantId",
                table: "engine_form_submissions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_generated_documents_DocumentTemplateId",
                table: "engine_generated_documents",
                column: "DocumentTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_generated_documents_EntityType_EntityId",
                table: "engine_generated_documents",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_grades_TenantId_Code",
                table: "engine_grades",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_metadata_definitions_TenantId_Code",
                table: "engine_metadata_definitions",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_metadata_fields_MetadataDefinitionId",
                table: "engine_metadata_fields",
                column: "MetadataDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_metadata_options_MetadataFieldId",
                table: "engine_metadata_options",
                column: "MetadataFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_metadata_values_EntityType_EntityId",
                table: "engine_metadata_values",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_metadata_values_MetadataDefinitionId",
                table: "engine_metadata_values",
                column: "MetadataDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_metadata_values_TenantId",
                table: "engine_metadata_values",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_object_definitions_TenantId_Code",
                table: "engine_object_definitions",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_object_fields_ObjectDefinitionId",
                table: "engine_object_fields",
                column: "ObjectDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_object_permissions_ObjectDefinitionId",
                table: "engine_object_permissions",
                column: "ObjectDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_object_relationships_SourceObjectId",
                table: "engine_object_relationships",
                column: "SourceObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_object_relationships_TargetObjectId",
                table: "engine_object_relationships",
                column: "TargetObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_org_edges_SourceNodeId",
                table: "engine_org_edges",
                column: "SourceNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_org_edges_TargetNodeId",
                table: "engine_org_edges",
                column: "TargetNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_org_graph_layouts_TenantId_Code",
                table: "engine_org_graph_layouts",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_org_nodes_ParentNodeId",
                table: "engine_org_nodes",
                column: "ParentNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_org_nodes_TenantId_NodeType_EntityId",
                table: "engine_org_nodes",
                columns: new[] { "TenantId", "NodeType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_permission_template_items_PermissionTemplateId",
                table: "engine_permission_template_items",
                column: "PermissionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_permission_templates_TenantId",
                table: "engine_permission_templates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_positions_ParentPositionId",
                table: "engine_positions",
                column: "ParentPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_positions_TenantId_Code",
                table: "engine_positions",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_report_definitions_TemplateId",
                table: "engine_report_definitions",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_report_definitions_TenantId_Code",
                table: "engine_report_definitions",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_report_fields_ReportDefinitionId",
                table: "engine_report_fields",
                column: "ReportDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_report_filters_ReportDefinitionId",
                table: "engine_report_filters",
                column: "ReportDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_report_groupings_ReportDefinitionId",
                table: "engine_report_groupings",
                column: "ReportDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_report_relationships_ReportDefinitionId",
                table: "engine_report_relationships",
                column: "ReportDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_report_schedules_ReportDefinitionId",
                table: "engine_report_schedules",
                column: "ReportDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_report_shares_ReportDefinitionId",
                table: "engine_report_shares",
                column: "ReportDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_report_sortings_ReportDefinitionId",
                table: "engine_report_sortings",
                column: "ReportDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_report_templates_TenantId_Code",
                table: "engine_report_templates",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_timeline_events_EntityType_EntityId",
                table: "engine_timeline_events",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_timeline_events_OccurredAt",
                table: "engine_timeline_events",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_engine_timeline_events_TenantId",
                table: "engine_timeline_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_timeline_subscriptions_TenantId",
                table: "engine_timeline_subscriptions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_timeline_subscriptions_UserId_EntityType_EntityId",
                table: "engine_timeline_subscriptions",
                columns: new[] { "UserId", "EntityType", "EntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_token_categories_Code",
                table: "engine_token_categories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_token_definitions_CategoryId",
                table: "engine_token_definitions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_token_definitions_Code",
                table: "engine_token_definitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_user_permission_overrides_TenantId_UserId",
                table: "engine_user_permission_overrides",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_user_permission_scopes_UserId",
                table: "engine_user_permission_scopes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_widget_data_sources_WidgetDefinitionId",
                table: "engine_widget_data_sources",
                column: "WidgetDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_widget_definitions_TenantId_Code",
                table: "engine_widget_definitions",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_widget_drilldowns_DashboardWidgetId",
                table: "engine_widget_drilldowns",
                column: "DashboardWidgetId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_widget_filters_DashboardWidgetId",
                table: "engine_widget_filters",
                column: "DashboardWidgetId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_widget_layouts_DashboardWidgetId",
                table: "engine_widget_layouts",
                column: "DashboardWidgetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_widget_permissions_WidgetDefinitionId",
                table: "engine_widget_permissions",
                column: "WidgetDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_workflow_actions_WorkflowNodeId",
                table: "engine_workflow_actions",
                column: "WorkflowNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_workflow_approver_rules_WorkflowNodeId",
                table: "engine_workflow_approver_rules",
                column: "WorkflowNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_workflow_conditions_WorkflowNodeId",
                table: "engine_workflow_conditions",
                column: "WorkflowNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_workflow_definitions_TenantId_Code",
                table: "engine_workflow_definitions",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engine_workflow_dynamic_approvers_WorkflowNodeId",
                table: "engine_workflow_dynamic_approvers",
                column: "WorkflowNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_workflow_dynamic_conditions_WorkflowNodeId",
                table: "engine_workflow_dynamic_conditions",
                column: "WorkflowNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_workflow_edges_SourceNodeId",
                table: "engine_workflow_edges",
                column: "SourceNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_workflow_edges_TargetNodeId",
                table: "engine_workflow_edges",
                column: "TargetNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_workflow_edges_WorkflowVersionId",
                table: "engine_workflow_edges",
                column: "WorkflowVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_workflow_instance_steps_AssignedToId",
                table: "engine_workflow_instance_steps",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_workflow_instance_steps_WorkflowInstanceId",
                table: "engine_workflow_instance_steps",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_workflow_instance_steps_WorkflowNodeId",
                table: "engine_workflow_instance_steps",
                column: "WorkflowNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_workflow_instances_EntityType_EntityId",
                table: "engine_workflow_instances",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_engine_workflow_instances_TenantId",
                table: "engine_workflow_instances",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_workflow_instances_WorkflowDefinitionId",
                table: "engine_workflow_instances",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_workflow_instances_WorkflowVersionId",
                table: "engine_workflow_instances",
                column: "WorkflowVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_workflow_nodes_WorkflowVersionId",
                table: "engine_workflow_nodes",
                column: "WorkflowVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_workflow_simulations_WorkflowVersionId",
                table: "engine_workflow_simulations",
                column: "WorkflowVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_engine_workflow_versions_WorkflowDefinitionId",
                table: "engine_workflow_versions",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_hr_task_activities_TaskId",
                table: "hr_task_activities",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_hr_task_checklists_TaskId",
                table: "hr_task_checklists",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_hr_task_comments_TaskId",
                table: "hr_task_comments",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_hr_tasks_AssigneeId",
                table: "hr_tasks",
                column: "AssigneeId");

            migrationBuilder.CreateIndex(
                name: "IX_hr_tasks_Status",
                table: "hr_tasks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_hr_tasks_TenantId",
                table: "hr_tasks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_permissions_Module_Name",
                table: "permissions",
                columns: new[] { "Module", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_Token",
                table: "refresh_tokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UserId",
                table: "refresh_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_PermissionId",
                table: "role_permissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_RoleId_PermissionId",
                table: "role_permissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_TenantId_Name",
                table: "roles",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_master_data_items_TenantId_ObjectType_Code",
                table: "tenant_master_data_items",
                columns: new[] { "TenantId", "ObjectType", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_master_data_items_TenantId_ObjectType_IsActive_SortO~",
                table: "tenant_master_data_items",
                columns: new[] { "TenantId", "ObjectType", "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_tenants_Domain",
                table: "tenants",
                column: "Domain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_permissions_PermissionId",
                table: "user_permissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_user_permissions_UserId_PermissionId",
                table: "user_permissions",
                columns: new[] { "UserId", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_RoleId",
                table: "user_roles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_UserId_RoleId",
                table: "user_roles",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissionTemplates_PermissionTemplateId",
                table: "UserPermissionTemplates",
                column: "PermissionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissionTemplates_UserId_PermissionTemplateId",
                table: "UserPermissionTemplates",
                columns: new[] { "UserId", "PermissionTemplateId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_TenantId",
                table: "users",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "branches");

            migrationBuilder.DropTable(
                name: "company_settings");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "employees");

            migrationBuilder.DropTable(
                name: "engine_audit_configurations");

            migrationBuilder.DropTable(
                name: "engine_audit_entries");

            migrationBuilder.DropTable(
                name: "engine_automation_actions");

            migrationBuilder.DropTable(
                name: "engine_automation_conditions");

            migrationBuilder.DropTable(
                name: "engine_automation_execution_logs");

            migrationBuilder.DropTable(
                name: "engine_automation_triggers");

            migrationBuilder.DropTable(
                name: "engine_calendar_settings");

            migrationBuilder.DropTable(
                name: "engine_company_branding");

            migrationBuilder.DropTable(
                name: "engine_company_profiles");

            migrationBuilder.DropTable(
                name: "engine_cost_centers");

            migrationBuilder.DropTable(
                name: "engine_dashboard_shares");

            migrationBuilder.DropTable(
                name: "engine_document_template_tokens");

            migrationBuilder.DropTable(
                name: "engine_document_template_versions");

            migrationBuilder.DropTable(
                name: "engine_document_workflow_links");

            migrationBuilder.DropTable(
                name: "engine_employee_reporting_lines");

            migrationBuilder.DropTable(
                name: "engine_fiscal_periods");

            migrationBuilder.DropTable(
                name: "engine_form_submission_values");

            migrationBuilder.DropTable(
                name: "engine_generated_documents");

            migrationBuilder.DropTable(
                name: "engine_grades");

            migrationBuilder.DropTable(
                name: "engine_metadata_options");

            migrationBuilder.DropTable(
                name: "engine_metadata_values");

            migrationBuilder.DropTable(
                name: "engine_object_fields");

            migrationBuilder.DropTable(
                name: "engine_object_permissions");

            migrationBuilder.DropTable(
                name: "engine_object_relationships");

            migrationBuilder.DropTable(
                name: "engine_org_edges");

            migrationBuilder.DropTable(
                name: "engine_org_graph_layouts");

            migrationBuilder.DropTable(
                name: "engine_permission_template_items");

            migrationBuilder.DropTable(
                name: "engine_positions");

            migrationBuilder.DropTable(
                name: "engine_report_fields");

            migrationBuilder.DropTable(
                name: "engine_report_filters");

            migrationBuilder.DropTable(
                name: "engine_report_groupings");

            migrationBuilder.DropTable(
                name: "engine_report_relationships");

            migrationBuilder.DropTable(
                name: "engine_report_schedules");

            migrationBuilder.DropTable(
                name: "engine_report_shares");

            migrationBuilder.DropTable(
                name: "engine_report_sortings");

            migrationBuilder.DropTable(
                name: "engine_timeline_events");

            migrationBuilder.DropTable(
                name: "engine_timeline_subscriptions");

            migrationBuilder.DropTable(
                name: "engine_token_definitions");

            migrationBuilder.DropTable(
                name: "engine_user_permission_overrides");

            migrationBuilder.DropTable(
                name: "engine_user_permission_scopes");

            migrationBuilder.DropTable(
                name: "engine_widget_data_sources");

            migrationBuilder.DropTable(
                name: "engine_widget_drilldowns");

            migrationBuilder.DropTable(
                name: "engine_widget_filters");

            migrationBuilder.DropTable(
                name: "engine_widget_layouts");

            migrationBuilder.DropTable(
                name: "engine_widget_permissions");

            migrationBuilder.DropTable(
                name: "engine_workflow_actions");

            migrationBuilder.DropTable(
                name: "engine_workflow_approver_rules");

            migrationBuilder.DropTable(
                name: "engine_workflow_conditions");

            migrationBuilder.DropTable(
                name: "engine_workflow_dynamic_approvers");

            migrationBuilder.DropTable(
                name: "engine_workflow_dynamic_conditions");

            migrationBuilder.DropTable(
                name: "engine_workflow_edges");

            migrationBuilder.DropTable(
                name: "engine_workflow_instance_steps");

            migrationBuilder.DropTable(
                name: "engine_workflow_simulations");

            migrationBuilder.DropTable(
                name: "hr_task_activities");

            migrationBuilder.DropTable(
                name: "hr_task_checklists");

            migrationBuilder.DropTable(
                name: "hr_task_comments");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "tenant_master_data_items");

            migrationBuilder.DropTable(
                name: "tenants");

            migrationBuilder.DropTable(
                name: "user_permissions");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "UserPermissionTemplates");

            migrationBuilder.DropTable(
                name: "engine_automation_rules");

            migrationBuilder.DropTable(
                name: "engine_form_fields");

            migrationBuilder.DropTable(
                name: "engine_form_submissions");

            migrationBuilder.DropTable(
                name: "engine_document_templates");

            migrationBuilder.DropTable(
                name: "engine_metadata_fields");

            migrationBuilder.DropTable(
                name: "engine_object_definitions");

            migrationBuilder.DropTable(
                name: "engine_org_nodes");

            migrationBuilder.DropTable(
                name: "engine_report_definitions");

            migrationBuilder.DropTable(
                name: "engine_token_categories");

            migrationBuilder.DropTable(
                name: "engine_dashboard_widgets");

            migrationBuilder.DropTable(
                name: "engine_workflow_instances");

            migrationBuilder.DropTable(
                name: "engine_workflow_nodes");

            migrationBuilder.DropTable(
                name: "hr_tasks");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "engine_permission_templates");

            migrationBuilder.DropTable(
                name: "engine_form_definitions");

            migrationBuilder.DropTable(
                name: "engine_metadata_definitions");

            migrationBuilder.DropTable(
                name: "engine_report_templates");

            migrationBuilder.DropTable(
                name: "engine_dashboard_definitions");

            migrationBuilder.DropTable(
                name: "engine_widget_definitions");

            migrationBuilder.DropTable(
                name: "engine_workflow_versions");

            migrationBuilder.DropTable(
                name: "engine_dashboard_categories");

            migrationBuilder.DropTable(
                name: "engine_dashboard_templates");

            migrationBuilder.DropTable(
                name: "engine_workflow_definitions");
        }
    }
}
