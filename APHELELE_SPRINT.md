# Aphelele – Developer Onboarding & Sprint Map

> **Branch:** `feature/aphelele`
> **Role:** Full-Stack .NET Developer
> **Stack:** ASP.NET Core 8 API · Blazor Server UI · EF Core 8 · SQL Server

---

## What's Already Built (Do Not Rebuild)

The following is **fully implemented and committed** on both `main` and `feature/aphelele`:

### Backend — `LeaveFlow.API` + `LeaveFlow.Application` + `LeaveFlow.Infrastructure` + `LeaveFlow.Domain`
- ✅ All 5 domain entities + enums
- ✅ All CQRS handlers: Login, CreateLeave, ReviewLeave, CancelLeave
- ✅ All queries: GetMyRequests, GetPendingTeam, GetBalances
- ✅ All repositories + UnitOfWork
- ✅ JWT service, Email service (MailKit), Password hasher (BCrypt)
- ✅ All 8 API endpoints wired and working
- ✅ Swagger with Bearer auth at `https://localhost:5001/swagger`
- ✅ CORS configured for `https://localhost:5002`
- ✅ Serilog structured logging

### Frontend — `LeaveFlow.BlazorUI`
- ✅ Full Blazor Server project at `https://localhost:5002`
- ✅ Layout: MainLayout (auth guard), NavMenu (role-filtered), TopBar, EmptyLayout
- ✅ Pages: Login, Dashboard, MyLeaves, CreateLeave, Approvals, Admin/Users, Admin/AuditLogs, Profile
- ✅ Services: ApiService (all 8 endpoints), AuthService (JWT + LocalStorage)
- ✅ Full design system in `wwwroot/app.css` (blue/teal theme, Inter font)
- ✅ Added to `LeaveFlow.sln`

---

## 1. First-Time Setup (Do This Once)

### 1.1 Prerequisites

| Tool | Download |
|------|----------|
| .NET 8 SDK | https://dotnet.microsoft.com/download/dotnet/8.0 |
| SQL Server Express or LocalDB | https://www.microsoft.com/en-us/sql-server/sql-server-downloads |
| SQL Server Management Studio (SSMS) | https://aka.ms/ssmsfullsetup |
| Visual Studio 2022 (Community is free) | https://visualstudio.microsoft.com/ |
| Git | https://git-scm.com/downloads |

---

### 1.2 Clone & Checkout Your Branch

```bash
git clone https://github.com/Thando12345/Enterprise-Leave-Management-System.git
cd Enterprise-Leave-Management-System
git checkout feature/aphelele
```

---

### 1.3 Configure the Database

Edit `src/LeaveFlow.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LeaveFlowDB;Trusted_Connection=True;"
  },
  "Jwt": {
    "Secret": "LeaveFlow_SuperSecret_Key_32Chars!!",
    "Issuer": "LeaveFlow.API",
    "Audience": "LeaveFlow.Client"
  },
  "Email": {
    "From": "noreply@leaveflow.com",
    "Host": "smtp.gmail.com",
    "Port": "587",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  },
  "Cors": {
    "AllowedOrigin": "https://localhost:5002"
  }
}
```

> For SQL Server Express instead of LocalDB:
> `"Server=.\\SQLEXPRESS;Database=LeaveFlowDB;Trusted_Connection=True;TrustServerCertificate=True;"`

---

### 1.4 Apply EF Core Migrations

```bash
dotnet tool install --global dotnet-ef

cd src/LeaveFlow.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../LeaveFlow.API
dotnet ef database update --startup-project ../LeaveFlow.API
```

Verify in SSMS: connect to `(localdb)\mssqllocaldb` → `LeaveFlowDB` → Tables should show:
`Users`, `LeaveRequests`, `LeaveBalances`, `AuditLogs`, `IdempotencyKeys`

---

### 1.5 Seed Test Data

Generate real BCrypt hashes first (run in any .NET console app or LINQPad):

```csharp
Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("Admin@123"));
Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("Manager@123"));
Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("Employee@123"));
```

Then run this in SSMS (replace `<HASH_...>` with the real output above):

```sql
USE LeaveFlowDB;
GO

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@leaveflow.com')
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Role, TeamId)
VALUES ('admin@leaveflow.com', '<HASH_FOR_Admin@123>', 'Admin', 'User', 2, NULL);

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'manager@leaveflow.com')
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Role, TeamId)
VALUES ('manager@leaveflow.com', '<HASH_FOR_Manager@123>', 'Jane', 'Manager', 1, 1);

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'employee@leaveflow.com')
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Role, TeamId)
VALUES ('employee@leaveflow.com', '<HASH_FOR_Employee@123>', 'John', 'Employee', 0, 1);

DECLARE @EmpId INT = (SELECT Id FROM Users WHERE Email = 'employee@leaveflow.com');
DECLARE @MgrId INT = (SELECT Id FROM Users WHERE Email = 'manager@leaveflow.com');

INSERT INTO LeaveBalances (EmployeeId, LeaveType, TotalDays, UsedDays, Year)
SELECT @EmpId, 0, 20, 0, 2025 WHERE NOT EXISTS (SELECT 1 FROM LeaveBalances WHERE EmployeeId = @EmpId AND LeaveType = 0 AND Year = 2025);
INSERT INTO LeaveBalances (EmployeeId, LeaveType, TotalDays, UsedDays, Year)
SELECT @EmpId, 1, 10, 0, 2025 WHERE NOT EXISTS (SELECT 1 FROM LeaveBalances WHERE EmployeeId = @EmpId AND LeaveType = 1 AND Year = 2025);
INSERT INTO LeaveBalances (EmployeeId, LeaveType, TotalDays, UsedDays, Year)
SELECT @EmpId, 2, 5,  0, 2025 WHERE NOT EXISTS (SELECT 1 FROM LeaveBalances WHERE EmployeeId = @EmpId AND LeaveType = 2 AND Year = 2025);
INSERT INTO LeaveBalances (EmployeeId, LeaveType, TotalDays, UsedDays, Year)
SELECT @MgrId, 0, 20, 0, 2025 WHERE NOT EXISTS (SELECT 1 FROM LeaveBalances WHERE EmployeeId = @MgrId AND LeaveType = 0 AND Year = 2025);
INSERT INTO LeaveBalances (EmployeeId, LeaveType, TotalDays, UsedDays, Year)
SELECT @MgrId, 1, 10, 0, 2025 WHERE NOT EXISTS (SELECT 1 FROM LeaveBalances WHERE EmployeeId = @MgrId AND LeaveType = 1 AND Year = 2025);
GO
```

---

### 1.6 Run Both Projects

Open two terminals:

```bash
# Terminal 1 — API
cd src/LeaveFlow.API
dotnet run
# → https://localhost:5001/swagger

# Terminal 2 — Blazor UI
cd src/LeaveFlow.BlazorUI
dotnet run
# → https://localhost:5002
```

Test login in Swagger:
```json
POST /api/auth/login
{ "email": "admin@leaveflow.com", "password": "Admin@123" }
```
Copy `accessToken` → click **Authorize** → paste `Bearer <token>`.

Then open `https://localhost:5002` in your browser and log in with the same credentials.

---

## 2. Project Structure

```
src/
├── LeaveFlow.Domain/
│   ├── Entities/          ← User, LeaveRequest, LeaveBalance, AuditLog, IdempotencyKey
│   └── Enums/             ← LeaveType, LeaveStatus, UserRole
│
├── LeaveFlow.Application/
│   ├── Common/            ← Result<T>
│   ├── DTOs/              ← All request/response shapes
│   ├── Interfaces/        ← IRepository contracts, IServices
│   └── Features/
│       ├── Auth/          ← LoginCommand + Handler + Validator
│       └── LeaveRequests/
│           ├── Commands/  ← Create, Review, Cancel handlers
│           └── Queries/   ← GetMy, GetPending, GetBalances handlers
│
├── LeaveFlow.Infrastructure/
│   ├── Persistence/       ← AppDbContext (EF Core)
│   ├── Repositories/      ← All 5 repository implementations + UnitOfWork
│   └── Services/          ← JwtService, EmailService, PasswordHasher
│
├── LeaveFlow.API/
│   ├── Controllers/       ← AuthController, LeaveRequestsController, AdminController
│   ├── Middleware/        ← ExceptionMiddleware
│   ├── Program.cs         ← Full wiring: JWT, Swagger, CORS, Serilog
│   └── appsettings.json   ← All config keys
│
└── LeaveFlow.BlazorUI/
    ├── Components/
    │   ├── Layout/        ← MainLayout, NavMenu, TopBar, EmptyLayout
    │   └── Pages/
    │       ├── Login.razor, Dashboard.razor, MyLeaves.razor
    │       ├── CreateLeave.razor, Approvals.razor, Profile.razor
    │       └── Admin/     ← AuditLogs.razor, Users.razor
    ├── Services/          ← ApiService.cs, AuthService.cs
    ├── wwwroot/app.css    ← Full design system
    └── Program.cs         ← Blazor wiring: HttpClient, LocalStorage, AuthService
```

---

## 3. Sprint Map

### Sprint 0 — Boilerplate ✅ Complete
- [x] Solution with 5 projects created and building
- [x] Domain entities and enums
- [x] Application CQRS handlers (Auth, LeaveRequests)
- [x] Infrastructure (DbContext, Repos, JWT, Email, BCrypt)
- [x] API Controllers, Swagger, JWT auth, CORS, Serilog
- [x] Blazor UI — all pages, layout, services, design system
- [x] All docs: README, ARCHITECTURE, APHELELE_SPRINT, DB_SETUP

---

### Sprint 1 — Local Environment Verification (Your First Task)

**Goal:** Get both projects running locally with real data.

| Task | Where | Done? |
|------|-------|-------|
| Install .NET 8 SDK | Prerequisites | ☐ |
| Install dotnet-ef tool | Terminal | ☐ |
| Clone repo + checkout `feature/aphelele` | Git | ☐ |
| Update connection string in `appsettings.json` | `LeaveFlow.API` | ☐ |
| Run EF migrations | `LeaveFlow.Infrastructure` | ☐ |
| Seed test users + balances | SSMS | ☐ |
| Run API — test all 8 endpoints in Swagger | `LeaveFlow.API` | ☐ |
| Run Blazor UI — login + navigate all pages | `LeaveFlow.BlazorUI` | ☐ |

**Done when:** You can log in at `https://localhost:5002` as all 3 roles and see the correct pages.

---

### Sprint 2 — Bug Fixes & UI Polish

**Goal:** Fix any issues found during Sprint 1 verification.

| Task | Where | Notes |
|------|-------|-------|
| Fix any API errors found in Swagger | `LeaveFlow.API` | Check 401/403/500 responses |
| Fix any Blazor rendering issues | `LeaveFlow.BlazorUI/Components/Pages` | Check browser console |
| Add loading state to all pages that are missing it | All pages | Use `lf-spinner` CSS class |
| Test role-based nav — Employee sees no Approvals/Admin | `NavMenu.razor` | Log in as each role |
| Test cancel leave — only shows on Pending requests | `MyLeaves.razor` | Verify button visibility |
| Test approve/reject modal — comment saves correctly | `Approvals.razor` | Check API response |
| Verify balance updates after approval | `Dashboard.razor` | Check `UsedDays` increments |

---

### Sprint 3 — GET /api/admin/users Endpoint

**Goal:** Wire up the Users admin page to real API data.

The `Admin/Users.razor` page currently shows placeholder data. Add the real endpoint:

| Task | Where | Notes |
|------|-------|-------|
| Add `GET /api/admin/users` endpoint | `AdminController.cs` | Returns all users |
| Add `GetAllUsersQuery` + handler | `Application/Features` | Query pattern, no command |
| Add `GetAllUsersAsync()` to `IUserRepository` | `IRepositories.cs` | Simple `ToListAsync()` |
| Implement in `UserRepository.cs` | `Infrastructure/Repositories` | |
| Update `ApiService.cs` | `BlazorUI/Services` | Add `GetUsersAsync()` method |
| Update `Admin/Users.razor` | `BlazorUI/Components/Pages/Admin` | Replace placeholder with real data |

---

### Sprint 4 — Testing

**Goal:** Unit and integration test coverage for critical paths.

| Task | Where | Notes |
|------|-------|-------|
| Create `tests/LeaveFlow.UnitTests` project | `tests/` | `dotnet new xunit` |
| Test `CreateLeaveRequestHandler` — happy path | Unit test | Mock repos, verify save called |
| Test `CreateLeaveRequestHandler` — insufficient balance | Unit test | Verify `Result.Failure` returned |
| Test `ReviewLeaveRequestHandler` — approve deducts balance | Unit test | Verify `UsedDays` incremented |
| Test `LoginHandler` — invalid password | Unit test | Verify `Result.Failure` returned |
| Create `tests/LeaveFlow.IntegrationTests` project | `tests/` | `dotnet new xunit` + WebApplicationFactory |
| Integration test `POST /api/auth/login` | Integration test | Real DB, real JWT |
| Integration test `POST /api/leaverequests` | Integration test | Full flow |

---

### Sprint 5 — Polish & Production Readiness

**Goal:** Production-ready quality before PR to main.

| Task | Notes |
|------|-------|
| Add error boundary to `Routes.razor` | Catch Blazor render errors gracefully |
| Add toast notification component | Reusable `Toast.razor` with auto-dismiss |
| Test responsive layout on mobile viewport | Check sidebar collapses correctly |
| Review all `appsettings.json` — no real secrets committed | Use environment variables or secrets manager |
| Update `APHELELE_SPRINT.md` — mark completed sprints | Keep the doc current |
| Open Pull Request: `feature/aphelele` → `main` | GitHub PR with description |

---

## 4. API ↔ Blazor UI Wiring Map

| Blazor Page | Method | API Endpoint | Handler |
|-------------|--------|--------------|---------|
| `Login.razor` | POST | `/api/auth/login` | `LoginHandler` |
| `Dashboard.razor` | GET | `/api/leaverequests/balances` | `GetLeaveBalancesHandler` |
| `Dashboard.razor` | GET | `/api/leaverequests/my` | `GetMyLeaveRequestsHandler` |
| `MyLeaves.razor` | GET | `/api/leaverequests/my` | `GetMyLeaveRequestsHandler` |
| `MyLeaves.razor` | PUT | `/api/leaverequests/{id}/cancel` | `CancelLeaveRequestHandler` |
| `CreateLeave.razor` | GET | `/api/leaverequests/balances` | `GetLeaveBalancesHandler` |
| `CreateLeave.razor` | POST | `/api/leaverequests` | `CreateLeaveRequestHandler` |
| `Approvals.razor` | GET | `/api/leaverequests/pending` | `GetPendingTeamRequestsHandler` |
| `Approvals.razor` | PUT | `/api/leaverequests/{id}/review` | `ReviewLeaveRequestHandler` |
| `Admin/AuditLogs.razor` | GET | `/api/admin/auditlogs?page=1&pageSize=20` | `AuditLogRepository` |
| `Admin/Users.razor` | GET | `/api/admin/users` *(Sprint 3)* | `GetAllUsersHandler` |

---

## 5. Git Workflow

```bash
# Always work on your branch
git checkout feature/aphelele

# Pull latest from main before starting work
git fetch origin
git merge origin/main

# Stage and commit your work
git add .
git commit -m "feat(blazor): fix balance display on dashboard"

# Push your branch
git push origin feature/aphelele

# When ready → open Pull Request on GitHub: feature/aphelele → main
```

### Commit Message Format

```
<type>(<scope>): <short description>

Types: feat | fix | refactor | test | docs | chore
Scope: api | blazor | domain | infra | auth | leaves | admin
```

---

## 6. Definition of Done

A task is only **done** when:

- [ ] Code compiles with 0 errors, 0 warnings
- [ ] Feature works end-to-end (UI → API → DB or API → DB)
- [ ] No hardcoded secrets or connection strings
- [ ] Committed to `feature/aphelele` with a meaningful message
- [ ] Tested manually (Swagger for API, browser for UI)
- [ ] Unit test written if it's a handler or service (Sprint 4)

---

## 7. Resources

| Resource | Link |
|----------|------|
| Swagger (local) | https://localhost:5001/swagger |
| Blazor UI (local) | https://localhost:5002 |
| EF Core Docs | https://learn.microsoft.com/en-us/ef/core/ |
| MediatR Docs | https://github.com/jbogard/MediatR |
| FluentValidation Docs | https://docs.fluentvalidation.net |
| Blazor Docs | https://learn.microsoft.com/en-us/aspnet/core/blazor |
| Blazored.LocalStorage | https://github.com/Blazored/LocalStorage |
| BCrypt NuGet | https://www.nuget.org/packages/BCrypt.Net-Next |
| JWT Debugger | https://jwt.io |
