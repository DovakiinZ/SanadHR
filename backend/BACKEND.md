# HR Cloud — Backend Foundation

## Overview

This is the **Phase 1 backend foundation** for the Saudi HR SaaS Platform, built with **C# / .NET 8 / ASP.NET Core Web API**. It follows a **Modular Monolith** architecture with full multi-tenancy, CQRS, and Arabic localization support.

## Tech Stack

| Technology | Purpose |
|---|---|
| .NET 8 / ASP.NET Core | Web API framework |
| PostgreSQL | Primary database |
| Entity Framework Core | ORM with global query filters |
| Dapper | Raw SQL queries |
| Redis | Caching layer |
| Cloudflare R2 (S3) | File storage |
| MediatR | CQRS command/query separation |
| FluentValidation | Request validation pipeline |
| AutoMapper | Entity-to-DTO mapping |
| JWT Bearer | Authentication |
| BCrypt | Password hashing |
| Swagger / OpenAPI | API documentation |

## Architecture

```
backend/
├── HR.sln                          # Solution file (20 projects)
└── src/
    ├── HR.Api/                     # Web API host, middleware, DI
    ├── HR.Application/             # Interfaces, DTOs, CQRS models, behaviors
    ├── HR.Domain/                  # Base entities, enums, value objects
    ├── HR.Infrastructure/          # EF DbContext, Redis, R2, audit, Dapper
    └── HR.Modules/
        ├── Core/                   # Department, Branch — full CRUD
        ├── Tenancy/                # Tenant entity
        ├── Identity/               # User, Role, Permission, JWT auth — full
        ├── Employees/              # Full CQRS — reference module
        ├── Tasks/                  # Full CQRS — matches frontend
        ├── Settings/               # CompanySettings — full
        ├── ESS/                    # Skeleton (501)
        ├── Workflows/              # Skeleton (501)
        ├── Attendance/             # Skeleton (501)
        ├── Payroll/                # Skeleton (501)
        ├── Expenses/               # Skeleton (501)
        ├── Loans/                  # Skeleton (501)
        ├── Documents/              # Skeleton (501)
        ├── Reports/                # Skeleton (501)
        ├── Dashboards/             # Skeleton (501)
        └── Notifications/          # Skeleton (501)
```

## What Was Built

### Layer 1: Domain (`HR.Domain`)
- **Base entities**: `BaseEntity`, `AuditableEntity` (with soft delete), `TenantEntity` (multi-tenant)
- **Enums**: Gender, ContractType, EmployeeStatus, HrTaskStatus, TaskPriority, TaskSource, PermissionScope, RequestStatus
- **Value objects**: Money (SAR default), Address (SA default), DateRange, PhoneNumber (+966 default)

### Layer 2: Application (`HR.Application`)
- **Interfaces**: `ICurrentUserService`, `ICacheService`, `IAuditLogService`, `IFileStorageService`, `IApplicationDbContext`
- **Models**: `ApiResponse<T>` (standardized API responses), `PaginatedList<T>`, `PaginationQuery`
- **Exceptions**: `NotFoundException`, `ForbiddenException`, `ValidationException`, `ConflictException`
- **Behaviors**: `ValidationBehavior` — MediatR pipeline that runs FluentValidation before handlers

### Layer 3: Infrastructure (`HR.Infrastructure`)
- **ApplicationDbContext**: Single shared context with global query filters for tenant isolation and soft delete, auto-audit on SaveChanges
- **Services**: RedisCacheService, AuditLogService, R2FileStorageService
- **DapperContext**: For raw SQL queries via Dapper
- **EF Configurations**: Table mappings, indexes, relationships for all entities
- **Seed data**: 70+ permissions across 16 modules with deterministic GUIDs

### Layer 4: API Host (`HR.Api`)
- **Program.cs**: Full DI composition root — JWT auth, CORS, Swagger, all 16 modules registered
- **ExceptionHandlingMiddleware**: Maps domain exceptions to HTTP status codes (400/403/404/409/500)
- **CurrentUserService**: Reads UserId, TenantId, Email, Permissions from JWT claims
- **BaseApiController**: MediatR helper with standardized response wrappers
- **RequirePermissionAttribute**: Authorization filter checking user permissions
- **appsettings.json**: PostgreSQL, Redis, JWT, R2, CORS configuration

### Module: Identity (Full Implementation)
- **Entities**: User, Role, Permission, UserRole, RolePermission, UserPermission, RefreshToken
- **Services**: JwtTokenService (access + refresh tokens), AuthService (register/login/refresh), PasswordHasher (BCrypt), PermissionService
- **Controllers**:
  - `POST /api/auth/register` — Creates tenant + admin user + default roles + all permissions
  - `POST /api/auth/login` — Authenticates and returns JWT + refresh token
  - `POST /api/auth/refresh` — Refreshes expired access token
  - `GET /api/auth/me` — Returns current user info
  - `GET /api/users` — List users (permission-gated)
  - `GET /api/roles` — List roles with permissions
  - `POST /api/roles` — Create role

### Module: Core (Full CRUD)
- **Entities**: Department (self-referencing hierarchy), Branch
- **Controllers**: `DepartmentsController`, `BranchesController` — full GET/POST/PUT/DELETE with permission-based auth

### Module: Employees (Full CQRS)
- **Entity**: Employee with 30+ fields (personal, employment, banking, organizational)
- **CQRS**: CreateEmployee, UpdateEmployee, DeleteEmployee commands; GetEmployees (paginated + filtered), GetEmployeeById queries
- **AutoMapper**: Arabic enum labels — ذكر/أنثى (Gender), نشط/موقوف/مستقيل (Status), دوام كامل/جزئي (ContractType)
- **Controller**: `GET/POST/PUT/DELETE /api/employees`

### Module: Tasks (Full CQRS)
- **Entities**: HrTask, HrTaskChecklist, HrTaskComment, HrTaskActivity
- **CQRS**: CreateTask (with checklists + activity log), UpdateTask (status tracking), DeleteTask, AddTaskComment, GetTasks (filtered), GetTaskById
- **AutoMapper**: Arabic labels — لم يبدأ/قيد التنفيذ/مكتمل (Status), منخفض/متوسط/عالي/عاجل (Priority)
- **Tags**: Stored as PostgreSQL jsonb, deserialized to `List<string>` in DTO
- **Controller**: `GET/POST/PUT/DELETE /api/tasks`, `POST /api/tasks/{id}/comments`

### Module: Settings (Full)
- **Entity**: CompanySettings with Saudi defaults (SAR, Asia/Riyadh, Sunday week start, 21 annual leave days)
- **Controller**: `GET/PUT /api/settings`

### Module: Tenancy
- **Entity**: Tenant with CompanyName, Domain, SubscriptionPlan

### Skeleton Modules (10)
ESS, Workflows, Attendance, Payroll, Expenses, Loans, Documents, Reports, Dashboards, Notifications — each has:
- Stub entity with TODO
- Placeholder controller returning HTTP 501
- Empty DI registration

## Key Design Decisions

1. **Single DbContext** — All modules share one context. Global query filters handle tenant isolation + soft delete automatically.
2. **Enum → Arabic mapping** — Enums stored as int in DB, AutoMapper converts to Arabic labels for frontend DTOs.
3. **Tags as jsonb** — PostgreSQL jsonb column for task tags, no separate junction table.
4. **Module DI pattern** — Each module exposes `Add{Module}Module()` extension method.
5. **CQRS via MediatR** — Commands and queries separated, validation pipeline built-in.
6. **Deterministic seed GUIDs** — Permission GUIDs generated from MD5 hash of `{Module}.{Name}` for consistent seeding.

## API Endpoints

| Method | Endpoint | Module | Auth |
|---|---|---|---|
| POST | `/api/auth/register` | Identity | Public |
| POST | `/api/auth/login` | Identity | Public |
| POST | `/api/auth/refresh` | Identity | Public |
| GET | `/api/auth/me` | Identity | JWT |
| GET/POST | `/api/users` | Identity | JWT + Permission |
| GET/POST | `/api/roles` | Identity | JWT + Permission |
| CRUD | `/api/departments` | Core | JWT + Permission |
| CRUD | `/api/branches` | Core | JWT + Permission |
| CRUD | `/api/employees` | Employees | JWT + Permission |
| CRUD | `/api/tasks` | Tasks | JWT + Permission |
| POST | `/api/tasks/{id}/comments` | Tasks | JWT + Permission |
| GET/PUT | `/api/settings` | Settings | JWT + Permission |
| GET | `/api/ess` | ESS | 501 |
| GET | `/api/workflows` | Workflows | 501 |
| GET | `/api/attendance` | Attendance | 501 |
| GET | `/api/payroll` | Payroll | 501 |
| GET | `/api/expenses` | Expenses | 501 |
| GET | `/api/loans` | Loans | 501 |
| GET | `/api/documents` | Documents | 501 |
| GET | `/api/reports` | Reports | 501 |
| GET | `/api/dashboards` | Dashboards | 501 |
| GET | `/api/notifications` | Notifications | 501 |

## Getting Started

### Prerequisites
- .NET 8 SDK
- PostgreSQL
- Redis (optional)

### Setup
```bash
cd backend
dotnet restore
dotnet build
```

### Run
```bash
cd src/HR.Api
dotnet run
```

API will be available at `https://localhost:5001` with Swagger UI at `/swagger`.

### Database Migration
```bash
cd src/HR.Infrastructure
dotnet ef migrations add Initial --startup-project ../HR.Api
dotnet ef database update --startup-project ../HR.Api
```

## File Count
- **20 projects** (.csproj)
- **139 C# source files**
- **3 JSON config files**
- **1 solution file**
- **163 total files**
