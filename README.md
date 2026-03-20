# LeaveFlow Enterprise

Production-grade HR Leave Management System built with **.NET 8**, **Clean Architecture**, **CQRS + MediatR**, **ASP.NET Core Web API**, and **Blazor Server UI**.

---

## Tech Stack

| Layer          | Technology                                        |
|----------------|---------------------------------------------------|
| UI             | Blazor Server (.NET 8) — `https://localhost:5002` |
| API            | ASP.NET Core 8 Web API — `https://localhost:5001` |
| Application    | MediatR (CQRS), FluentValidation                  |
| Domain         | Plain C# entities & enums — zero dependencies     |
| Infrastructure | EF Core 8 + SQL Server, MailKit, BCrypt           |
| Auth           | JWT Bearer (HS256) + Blazored.LocalStorage        |
| Logging        | Serilog (console + rolling file)                  |

---

## Project Structure

```
src/
├── LeaveFlow.API/            # Controllers, middleware, Program.cs
├── LeaveFlow.Application/    # CQRS handlers, DTOs, validators, interfaces
├── LeaveFlow.Domain/         # Entities, enums (no external dependencies)
├── LeaveFlow.Infrastructure/ # EF Core DbContext, repositories, JWT, email, BCrypt
└── LeaveFlow.BlazorUI/       # Blazor Server UI — pages, layout, services, CSS
    ├── Components/
    │   ├── Layout/           # MainLayout, NavMenu, TopBar, EmptyLayout
    │   └── Pages/            # Login, Dashboard, MyLeaves, CreateLeave,
    │       └── Admin/        # Approvals, Profile, AuditLogs, Users
    ├── Services/             # ApiService (HttpClient), AuthService (JWT storage)
    └── wwwroot/app.css       # Full design system — blue/teal theme
```

**Dependency flow:** BlazorUI → API (HTTP) | API → Application + Infrastructure → Domain

---

## Quick Start

### Prerequisites
- .NET 8 SDK
- SQL Server / LocalDB
- `dotnet tool install --global dotnet-ef`

### 1. Configure

Edit `src/LeaveFlow.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LeaveFlowDB;Trusted_Connection=True;"
  },
  "Jwt": {
    "Secret": "CHANGE_ME_TO_A_32_CHAR_SECRET_KEY!!",
    "Issuer": "LeaveFlow.API",
    "Audience": "LeaveFlow.Client"
  },
  "Email": {
    "From": "noreply@leaveflow.com",
    "Host": "smtp.example.com",
    "Port": "587",
    "Username": "smtp_user",
    "Password": "smtp_password"
  },
  "Cors": {
    "AllowedOrigin": "https://localhost:5002"
  }
}
```

### 2. Apply Migrations

```bash
cd src/LeaveFlow.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../LeaveFlow.API
dotnet ef database update --startup-project ../LeaveFlow.API
```

### 3. Run API

```bash
cd src/LeaveFlow.API
dotnet run
# Swagger: https://localhost:5001/swagger
```

### 4. Run Blazor UI

```bash
cd src/LeaveFlow.BlazorUI
dotnet run
# UI: https://localhost:5002
```

---

## API Endpoints

| Method | Route                          | Role           | Description            |
|--------|--------------------------------|----------------|------------------------|
| POST   | /api/auth/login                | Public         | Login, returns JWT     |
| GET    | /api/leaverequests/my          | Employee+      | My leave requests      |
| GET    | /api/leaverequests/balances    | Employee+      | My leave balances      |
| POST   | /api/leaverequests             | Employee+      | Submit leave request   |
| PUT    | /api/leaverequests/{id}/cancel | Employee+      | Cancel pending request |
| GET    | /api/leaverequests/pending     | Manager, Admin | Team pending requests  |
| PUT    | /api/leaverequests/{id}/review | Manager, Admin | Approve or reject      |
| GET    | /api/admin/auditlogs           | Admin          | Paginated audit logs   |

All protected routes require `Authorization: Bearer <token>` header.
Write endpoints accept an optional `Idempotency-Key` header to prevent duplicate submissions.

---

## Blazor UI Pages

| Route               | Page            | Roles          |
|---------------------|-----------------|----------------|
| `/login`            | Login           | Public         |
| `/`                 | Dashboard       | All            |
| `/my-leaves`        | My Leaves       | All            |
| `/create-leave`     | Request Leave   | All            |
| `/approvals`        | Approvals       | Manager, Admin |
| `/admin/users`      | User Management | Admin          |
| `/admin/audit-logs` | Audit Logs      | Admin          |
| `/profile`          | My Profile      | All            |

---

## Roles & Permissions

| Role     | Permissions                                            |
|----------|--------------------------------------------------------|
| Employee | Submit, view, cancel own requests; view own balances   |
| Manager  | All Employee permissions + review team requests        |
| Admin    | All Manager permissions + audit logs + user management |

---

## Architecture

```
Blazor UI (https://localhost:5002)
    ↓  HTTP + Bearer JWT
API Controllers (https://localhost:5001)
    ↓  MediatR
Application Layer — CQRS Handlers
    ↓  Interfaces
Infrastructure Layer — EF Core, JWT, Email, BCrypt
    ↓
SQL Server (LeaveFlowDB)
```

- **CQRS** — Commands mutate state; Queries read state — separate handler classes
- **Repository + Unit of Work** — All DB access abstracted; one `SaveChangesAsync` per command
- **Result\<T\>** — Typed success/failure wrapper — no exceptions for business errors
- **Idempotency** — `Idempotency-Key` header checked before processing write commands
- **Auth Guard** — `MainLayout.razor` checks JWT on every render; redirects to `/login` if expired

---

## Database Schema

**Users** — `Id, Email (unique), PasswordHash, FirstName, LastName, Role, TeamId`

**LeaveRequests** — `Id, EmployeeId (FK), StartDate, EndDate, LeaveType, Status, RequestDate, ReviewedBy, ReviewDate, Comments`

**LeaveBalances** — `Id, EmployeeId (FK), LeaveType, TotalDays, UsedDays, Year` (unique per employee+type+year)

**AuditLogs** — `Id, UserId, Action, Timestamp, Details`

**IdempotencyKeys** — `Key (PK), Response, CreatedAt`

---

## Deployment

| Component  | Target                                    |
|------------|-------------------------------------------|
| API        | Azure App Service / IIS                   |
| Blazor UI  | Azure App Service (Server-side rendering) |
| Database   | Azure SQL / SQL Server                    |
| CI/CD      | GitHub Actions — build → test → publish   |

---

## Future Enhancements

- Refresh token rotation
- SMS notifications (Twilio)
- Advanced reporting dashboard
- Mobile app (MAUI)
- bUnit UI tests
