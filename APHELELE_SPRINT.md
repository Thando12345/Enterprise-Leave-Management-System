# Aphelele – Developer Onboarding, Sprint Map & Implementation Guide

> **Branch:** `feature/aphelele`
> **Role:** Full-Stack .NET Developer
> **Stack:** ASP.NET Core 8 API · Blazor Server UI · EF Core 8 · SQL Server

---

## 1. First-Time Setup (Do This Once)

### 1.1 Prerequisites

Install these before anything else:

| Tool | Download |
|------|----------|
| .NET 8 SDK | https://dotnet.microsoft.com/download/dotnet/8.0 |
| SQL Server Express or LocalDB | https://www.microsoft.com/en-us/sql-server/sql-server-downloads |
| SQL Server Management Studio (SSMS) | https://aka.ms/ssmsfullsetup |
| Visual Studio 2022 (Community is free) | https://visualstudio.microsoft.com/ |
| Git | https://git-scm.com/downloads |
| Node.js (optional, for tooling) | https://nodejs.org |

---

### 1.2 Clone the Repository

```bash
git clone https://github.com/Thando12345/Enterprise-Leave-Management-System.git
cd Enterprise-Leave-Management-System
```

---

### 1.3 Checkout Your Branch

```bash
git checkout feature/aphelele
```

---

### 1.4 Configure the Database

Open `src/LeaveFlow.API/appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LeaveFlowDB;Trusted_Connection=True;"
  }
}
```

> If you're using a named SQL Server instance instead of LocalDB, replace with:
> `"Server=YOUR_PC_NAME\\SQLEXPRESS;Database=LeaveFlowDB;Trusted_Connection=True;TrustServerCertificate=True;"`

---

### 1.5 Apply EF Core Migrations (Creates the Database)

Run these commands from the repo root:

```bash
cd src/LeaveFlow.Infrastructure

dotnet ef migrations add InitialCreate --startup-project ../LeaveFlow.API

dotnet ef database update --startup-project ../LeaveFlow.API
```

This will:
- Create the `LeaveFlowDB` database automatically
- Create all tables: `Users`, `LeaveRequests`, `LeaveBalances`, `AuditLogs`, `IdempotencyKeys`

> **Verify in SSMS:** Connect to `(localdb)\mssqllocaldb` → expand Databases → you should see `LeaveFlowDB`

---

### 1.6 Seed Test Data (Manual SQL — run in SSMS)

```sql
USE LeaveFlowDB;

-- Admin user (password: Admin@123)
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Role, TeamId)
VALUES (
  'admin@leaveflow.com',
  '$2a$11$examplehashforadmin000000000000000000000000000000000',
  'Admin', 'User', 2, NULL
);

-- Manager (password: Manager@123)
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Role, TeamId)
VALUES (
  'manager@leaveflow.com',
  '$2a$11$examplehashformanager00000000000000000000000000000000',
  'Jane', 'Manager', 1, 1
);

-- Employee (password: Employee@123)
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Role, TeamId)
VALUES (
  'employee@leaveflow.com',
  '$2a$11$examplehashforemployee0000000000000000000000000000000',
  'John', 'Employee', 0, 1
);

-- Leave balances for employee (UserId = 3, adjust if different)
INSERT INTO LeaveBalances (EmployeeId, LeaveType, TotalDays, UsedDays, Year)
VALUES
  (3, 0, 20, 0, 2025),  -- Vacation
  (3, 1, 10, 0, 2025),  -- Sick
  (3, 2, 5,  0, 2025);  -- Personal
```

> **Note:** Replace the `PasswordHash` values with real BCrypt hashes.
> Generate them with this small C# snippet or use an online BCrypt tool:
> ```csharp
> Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("Admin@123"));
> ```

---

### 1.7 Configure JWT & Email

In `src/LeaveFlow.API/appsettings.json`:

```json
{
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

> For local dev, you can disable email sending by wrapping `EmailService.SendAsync` in a try/catch and logging instead of throwing.

---

### 1.8 Run the API

```bash
cd src/LeaveFlow.API
dotnet run
```

Open Swagger: **https://localhost:5001/swagger**

Test login:
```json
POST /api/auth/login
{
  "email": "admin@leaveflow.com",
  "password": "Admin@123"
}
```

Copy the `accessToken` from the response → click **Authorize** in Swagger → paste `Bearer <token>`.

---

## 2. Project Structure (Your Working Files)

```
src/
├── LeaveFlow.Domain/
│   ├── Entities/          ← User, LeaveRequest, LeaveBalance, AuditLog
│   └── Enums/             ← LeaveType, LeaveStatus, UserRole
│
├── LeaveFlow.Application/
│   ├── Common/            ← Result<T>
│   ├── DTOs/              ← Request/response shapes
│   ├── Interfaces/        ← IRepository contracts, IServices
│   └── Features/
│       ├── Auth/          ← LoginCommand + Handler + Validator
│       └── LeaveRequests/
│           ├── Commands/  ← Create, Review, Cancel
│           └── Queries/   ← GetMy, GetPending, GetBalances
│
├── LeaveFlow.Infrastructure/
│   ├── Persistence/       ← AppDbContext (EF Core)
│   ├── Repositories/      ← Concrete DB implementations
│   └── Services/          ← JWT, Email, PasswordHasher
│
├── LeaveFlow.API/
│   ├── Controllers/       ← AuthController, LeaveRequestsController, AdminController
│   ├── Middleware/        ← ExceptionMiddleware
│   ├── Program.cs         ← App wiring
│   └── appsettings.json   ← Config
│
└── LeaveFlow.BlazorUI/    ← YOUR MAIN BUILD TARGET (create this project)
    ├── Pages/
    ├── Services/
    ├── Shared/
    └── wwwroot/
```

---

## 3. Sprint Map

### Sprint 0 — Environment & Boilerplate ✅ (Done by Thando)
- [x] Solution structure created
- [x] Domain entities and enums
- [x] Application CQRS handlers (Auth, LeaveRequests)
- [x] Infrastructure (DbContext, Repos, JWT, Email, BCrypt)
- [x] API Controllers wired up
- [x] Swagger + JWT auth configured
- [x] README and ARCHITECTURE docs

---

### Sprint 1 — Database & API Verification (Week 1)

**Goal:** Get the API running locally with real data.

| Task | File(s) | Notes |
|------|---------|-------|
| Run EF migrations | `LeaveFlow.Infrastructure` | Creates all tables |
| Seed test users | SSMS / SQL script | Admin, Manager, Employee |
| Seed leave balances | SSMS / SQL script | Per user per type per year |
| Test all endpoints in Swagger | `LeaveFlow.API` | Login → copy token → test each route |
| Fix any migration issues | `AppDbContext.cs` | Add missing configs if needed |
| Add `dotnet-ef` tool if missing | Terminal | `dotnet tool install --global dotnet-ef` |

**Done when:** All 8 API endpoints return correct responses in Swagger.

---

### Sprint 2 — Blazor UI Project Setup (Week 1–2)

**Goal:** Create the Blazor Server project and wire it to the API.

| Task | File(s) | Notes |
|------|---------|-------|
| Create Blazor Server project | `src/LeaveFlow.BlazorUI` | `dotnet new blazorserver -n LeaveFlow.BlazorUI` |
| Add to solution | `LeaveFlow.sln` | `dotnet sln add src/LeaveFlow.BlazorUI/LeaveFlow.BlazorUI.csproj` |
| Add HttpClient + base URL config | `Program.cs` (Blazor) | Point to `https://localhost:5001` |
| Create `ApiService.cs` | `Services/ApiService.cs` | Typed HttpClient wrapper for all API calls |
| Create `AuthService.cs` | `Services/AuthService.cs` | Stores JWT in session, exposes current user/role |
| Add `appsettings.json` | `LeaveFlow.BlazorUI` | `"ApiBaseUrl": "https://localhost:5001"` |
| Add layout shell | `Shared/MainLayout.razor` | Sidebar + topbar skeleton |
| Add route guard | `App.razor` | Redirect to `/login` if not authenticated |

**Done when:** Blazor app starts, shows login page, redirects unauthenticated users.

---

### Sprint 3 — Authentication UI (Week 2)

**Goal:** Working login flow end-to-end.

| Task | File(s) | Notes |
|------|---------|-------|
| Login page | `Pages/Login.razor` | Email + password form |
| Call `POST /api/auth/login` | `ApiService.cs` | On form submit |
| Store JWT in `ProtectedSessionStorage` | `AuthService.cs` | Secure browser storage |
| Parse JWT claims (role, name) | `AuthService.cs` | Use `System.IdentityModel.Tokens.Jwt` |
| Redirect to `/dashboard` on success | `Login.razor` | `NavigationManager.NavigateTo` |
| Show error on bad credentials | `Login.razor` | Display `result.Error` |
| Logout button | `Shared/MainLayout.razor` | Clear storage, redirect to `/login` |
| Role-based menu visibility | `Shared/NavMenu.razor` | Hide Approvals if Employee, hide Admin if not Admin |

**Done when:** Login works, JWT stored, menu shows correct items per role, logout clears session.

---

### Sprint 4 — Employee Features (Week 2–3)

**Goal:** Employees can manage their own leave.

| Task | File(s) | Notes |
|------|---------|-------|
| Dashboard page | `Pages/Dashboard.razor` | Welcome card, balance summary, recent requests |
| My Leaves page | `Pages/MyLeaves.razor` | Table with filter + pagination |
| Cancel button | `Pages/MyLeaves.razor` | `PUT /api/leaverequests/{id}/cancel` |
| Create Leave page | `Pages/CreateLeave.razor` | Form: type dropdown, date pickers, comments |
| Real-time balance display | `Pages/CreateLeave.razor` | Fetch `GET /api/leaverequests/balances`, show remaining days |
| Form validation | `CreateLeave.razor` | End ≥ Start, type required |
| Idempotency key on submit | `ApiService.cs` | Generate `Guid.NewGuid()` per form submit |
| Success/error toast | `Shared/Toast.razor` | Reusable toast component |

**Done when:** Employee can view, create, and cancel leave requests with live balance feedback.

---

### Sprint 5 — Manager Features (Week 3)

**Goal:** Managers can review team requests.

| Task | File(s) | Notes |
|------|---------|-------|
| Approvals page | `Pages/Approvals.razor` | List of pending team requests |
| Approve button | `Pages/Approvals.razor` | `PUT /api/leaverequests/{id}/review` with `approve: true` |
| Reject button + comment | `Pages/Approvals.razor` | Modal with optional comment field |
| Route guard | `Pages/Approvals.razor` | `[Authorize(Roles = "Manager,Admin")]` |
| Refresh list after action | `Pages/Approvals.razor` | Re-fetch after approve/reject |

**Done when:** Manager can approve and reject requests, list refreshes, emails sent.

---

### Sprint 6 — Admin Panel (Week 3–4)

**Goal:** Admins can view audit logs and manage users.

| Task | File(s) | Notes |
|------|---------|-------|
| Admin panel page | `Pages/Admin/AdminPanel.razor` | Tabbed: Users / Audit Logs |
| Audit logs table | `Pages/Admin/AuditLogs.razor` | Paginated, `GET /api/admin/auditlogs` |
| User list | `Pages/Admin/Users.razor` | List all users (add endpoint if missing) |
| Add user form | `Pages/Admin/Users.razor` | Create user with role assignment |
| Route guard | All admin pages | Redirect non-admins |

**Done when:** Admin can view paginated audit logs and manage users.

---

### Sprint 7 — Polish & Testing (Week 4)

**Goal:** Production-ready quality.

| Task | Notes |
|------|-------|
| Loading spinners on all async calls | Show spinner while awaiting API |
| Error boundary component | Catch Blazor render errors gracefully |
| Responsive layout | Test on mobile viewport |
| Unit tests for Application handlers | `tests/LeaveFlow.UnitTests` |
| Integration tests for API endpoints | `tests/LeaveFlow.IntegrationTests` |
| Blazor UI tests with bUnit | `tests/LeaveFlow.BlazorUI.Tests` |
| `.gitignore` cleanup | Ensure no secrets committed |
| Final README update | Document Blazor setup steps |

---

## 4. API ↔ Blazor UI Wiring Map

Every Blazor page maps to one or more API calls. Use this as your implementation checklist.

| Blazor Page | HTTP Method | API Endpoint | Handler Called |
|-------------|-------------|--------------|----------------|
| `Login.razor` | POST | `/api/auth/login` | `LoginHandler` |
| `Dashboard.razor` | GET | `/api/leaverequests/balances` | `GetLeaveBalancesHandler` |
| `Dashboard.razor` | GET | `/api/leaverequests/my` | `GetMyLeaveRequestsHandler` |
| `MyLeaves.razor` | GET | `/api/leaverequests/my` | `GetMyLeaveRequestsHandler` |
| `MyLeaves.razor` | PUT | `/api/leaverequests/{id}/cancel` | `CancelLeaveRequestHandler` |
| `CreateLeave.razor` | GET | `/api/leaverequests/balances` | `GetLeaveBalancesHandler` |
| `CreateLeave.razor` | POST | `/api/leaverequests` | `CreateLeaveRequestHandler` |
| `Approvals.razor` | GET | `/api/leaverequests/pending` | `GetPendingTeamRequestsHandler` |
| `Approvals.razor` | PUT | `/api/leaverequests/{id}/review` | `ReviewLeaveRequestHandler` |
| `Admin/AuditLogs.razor` | GET | `/api/admin/auditlogs?page=1&pageSize=20` | `AuditLogRepository.GetAllAsync` |

---

## 5. ApiService.cs — Suggested Structure

Create `src/LeaveFlow.BlazorUI/Services/ApiService.cs`:

```csharp
public class ApiService(HttpClient http, AuthService auth)
{
    // Auth
    Task<LoginResponse?> LoginAsync(LoginRequest req);

    // Leave Requests
    Task<List<LeaveRequestDto>> GetMyRequestsAsync();
    Task<List<LeaveBalanceDto>> GetBalancesAsync(int year);
    Task<int> CreateLeaveRequestAsync(CreateLeaveRequestDto dto, string idempotencyKey);
    Task CancelLeaveRequestAsync(int id);

    // Manager
    Task<List<LeaveRequestDto>> GetPendingRequestsAsync();
    Task ReviewLeaveRequestAsync(int id, ReviewLeaveRequestDto dto);

    // Admin
    Task<List<AuditLog>> GetAuditLogsAsync(int page, int pageSize);
}
```

Each method:
1. Adds `Authorization: Bearer <token>` header from `AuthService`
2. Calls the API
3. Returns the deserialized response or throws on non-2xx

---

## 6. AuthService.cs — Suggested Structure

```csharp
public class AuthService(ProtectedSessionStorage storage, NavigationManager nav)
{
    Task LoginAsync(LoginResponse response);   // stores token + role
    Task LogoutAsync();                        // clears storage, redirects
    Task<string?> GetTokenAsync();             // returns stored JWT
    Task<string?> GetRoleAsync();              // "Employee" / "Manager" / "Admin"
    Task<bool> IsAuthenticatedAsync();         // checks token exists + not expired
    Task<string?> GetUserNameAsync();          // from stored FullName
}
```

---

## 7. Database Clone / Restore Guide

### Option A — Let EF Create It (Recommended for Dev)

```bash
dotnet ef database update --startup-project ../LeaveFlow.API
```

This creates a fresh empty `LeaveFlowDB`. Then run the seed SQL from Section 1.6.

### Option B — Restore from a Backup (.bak file)

If Thando shares a `.bak` file:

1. Open SSMS
2. Right-click **Databases** → **Restore Database**
3. Select **Device** → browse to the `.bak` file
4. Set database name to `LeaveFlowDB`
5. Click OK

Then update your connection string to match.

### Option C — Script the Schema + Data

Thando can generate a script from SSMS:
- Right-click `LeaveFlowDB` → **Tasks** → **Generate Scripts**
- Include schema + data
- Share the `.sql` file

You run it in SSMS:
```sql
-- Open the .sql file in SSMS and execute against your local server
```

---

## 8. Adding a New Migration (When Entities Change)

Whenever you change a Domain entity (add a property, new table, etc.):

```bash
cd src/LeaveFlow.Infrastructure

# Create the migration
dotnet ef migrations add <DescriptiveName> --startup-project ../LeaveFlow.API

# Apply it
dotnet ef database update --startup-project ../LeaveFlow.API
```

Example names: `AddRefreshTokenToUser`, `AddLeaveTypePersonal`, `AddTeamTable`

---

## 9. Git Workflow

```bash
# Always work on your branch
git checkout feature/aphelele

# Pull latest from main before starting work
git fetch origin
git merge origin/main

# Stage and commit your work
git add .
git commit -m "feat(blazor): add login page and AuthService"

# Push your branch
git push origin feature/aphelele

# When a feature is complete, open a Pull Request on GitHub:
# feature/aphelele → main
```

### Commit Message Format

```
<type>(<scope>): <short description>

Types: feat | fix | refactor | test | docs | chore
Scope: api | blazor | domain | infra | auth | leaves | admin
```

Examples:
```
feat(blazor): add create leave form with balance validation
feat(api): add GET /api/users endpoint for admin panel
fix(infra): fix overlap check query including canceled requests
test(app): add unit tests for CreateLeaveRequestHandler
docs: update APHELELE_SPRINT with completed sprint 1 tasks
```

---

## 10. Definition of Done (Per Task)

A task is only **done** when:

- [ ] Code compiles with 0 errors
- [ ] Feature works end-to-end (API → DB or UI → API → DB)
- [ ] No hardcoded secrets or connection strings
- [ ] Committed to `feature/aphelele` with a meaningful message
- [ ] Tested manually (Swagger for API, browser for UI)
- [ ] Unit test written if it's a handler or service (Sprint 7)

---

## 11. Contacts & Resources

| Resource | Link |
|----------|------|
| Swagger (local) | https://localhost:5001/swagger |
| Blazor UI (local) | https://localhost:5002 |
| EF Core Docs | https://learn.microsoft.com/en-us/ef/core/ |
| MediatR Docs | https://github.com/jbogard/MediatR |
| FluentValidation Docs | https://docs.fluentvalidation.net |
| Blazor Docs | https://learn.microsoft.com/en-us/aspnet/core/blazor |
| BCrypt NuGet | https://www.nuget.org/packages/BCrypt.Net-Next |
| JWT Debugger | https://jwt.io |
