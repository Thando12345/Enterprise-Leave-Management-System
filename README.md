# LeaveFlow Enterprise

Production-grade HR Leave Management System built with **.NET 8**, **Clean Architecture**, **CQRS + MediatR**, and **ASP.NET Core Web API**.

---

## Tech Stack

| Layer          | Technology                              |
|----------------|-----------------------------------------|
| API            | ASP.NET Core 8 Web API                  |
| Application    | MediatR (CQRS), FluentValidation        |
| Domain         | Plain C# entities & enums               |
| Infrastructure | EF Core 8 + SQL Server, MailKit, BCrypt |
| Auth           | JWT Bearer (HS256)                      |
| Logging        | Serilog                                 |

---

## Project Structure

```
src/
├── LeaveFlow.API/            # Controllers, middleware, Program.cs
├── LeaveFlow.Application/    # CQRS handlers, DTOs, validators, interfaces
├── LeaveFlow.Domain/         # Entities, enums (no dependencies)
└── LeaveFlow.Infrastructure/ # EF Core DbContext, repositories, JWT, email
```

**Dependency flow:** API → Application + Infrastructure → Domain

---

## Quick Start

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

---

## API Endpoints

| Method | Route                          | Role            | Description              |
|--------|--------------------------------|-----------------|--------------------------|
| POST   | /api/auth/login                | Public          | Login, returns JWT       |
| GET    | /api/leaverequests/my          | Employee+       | My leave requests        |
| GET    | /api/leaverequests/balances    | Employee+       | My leave balances        |
| POST   | /api/leaverequests             | Employee+       | Submit leave request     |
| PUT    | /api/leaverequests/{id}/cancel | Employee+       | Cancel pending request   |
| GET    | /api/leaverequests/pending     | Manager, Admin  | Team pending requests    |
| PUT    | /api/leaverequests/{id}/review | Manager, Admin  | Approve or reject        |
| GET    | /api/admin/auditlogs           | Admin           | Paginated audit logs     |

All protected routes require `Authorization: Bearer <token>` header.

Write endpoints accept an optional `Idempotency-Key` header to prevent duplicate submissions.

---

## Roles & Permissions

| Role     | Permissions                                              |
|----------|----------------------------------------------------------|
| Employee | Submit, view, cancel own requests; view own balances     |
| Manager  | All Employee permissions + review team requests          |
| Admin    | All Manager permissions + audit logs + user management   |

---

## Architecture

```
Request → Controller → MediatR Command/Query
                            ↓
                       Handler (Application)
                            ↓
                    Domain rules enforced
                            ↓
                  Repository (Infrastructure)
                            ↓
                       EF Core → SQL Server
```

- **CQRS**: Commands mutate state; Queries read state — separate handler classes
- **Repository + Unit of Work**: All DB access abstracted; `SaveChangesAsync` called once per command
- **Result<T>**: Typed success/failure wrapper — no exceptions for business errors
- **Idempotency**: `Idempotency-Key` header checked before processing write commands

---

## Database Schema

**Users** — `Id, Email (unique), PasswordHash, FirstName, LastName, Role, TeamId`

**LeaveRequests** — `Id, EmployeeId (FK), StartDate, EndDate, LeaveType, Status, RequestDate, ReviewedBy, ReviewDate, Comments`

**LeaveBalances** — `Id, EmployeeId (FK), LeaveType, TotalDays, UsedDays, Year` (unique per employee+type+year)

**AuditLogs** — `Id, UserId, Action, Timestamp, Details`

**IdempotencyKeys** — `Key (PK), Response, CreatedAt`

---

## Deployment

- **API**: Azure App Service or IIS
- **Database**: Azure SQL / SQL Server
- **CI/CD**: GitHub Actions — `dotnet build` → `dotnet test` → publish

---

## Future Enhancements

- Blazor UI frontend
- Refresh token rotation
- SMS notifications (Twilio)
- Advanced reporting dashboard
- Mobile app (MAUI)
