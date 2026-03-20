# LeaveFlow Architecture – Plain English Guide

> Read this before touching any code. It explains **what each layer does, why it exists, and what lives inside it.**

---

## The Big Picture

Think of the system like a restaurant:

```
Customer sitting at a table  →  Blazor UI        (https://localhost:5002)
Waiter taking the order      →  API Controllers  (https://localhost:5001)
Kitchen Manager              →  Application / CQRS Handlers
Recipe Rulebook              →  Domain Entities & Enums
Fridge & Suppliers           →  Infrastructure (DB, Email, JWT, BCrypt)
```

Each layer only talks to the layer directly below it.
No layer skips levels. No layer knows too much about another.

---

## Layer 1 — Domain (`LeaveFlow.Domain`)

### What is it?
The **core business rules** of the system. Pure C#. No frameworks. No database. No HTTP.

### Think of it as:
The rulebook. "A leave request must have a start date before the end date." "A user has a role." These facts live here forever, regardless of what database or UI you use.

### What's inside:

| File | What it does |
|------|-------------|
| `Entities/User.cs` | Represents an employee/manager/admin — name, email, role, team |
| `Entities/LeaveRequest.cs` | A single leave request — dates, type, status, who reviewed it |
| `Entities/LeaveBalance.cs` | How many leave days an employee has left per type per year |
| `Entities/AuditLog.cs` | A record of every important action (who did what, when) |
| `Entities/IdempotencyKey.cs` | Prevents the same request being processed twice |
| `Enums/Enums.cs` | `LeaveType` (Vacation/Sick/Personal/Maternity/Paternity/Unpaid), `LeaveStatus` (Pending/Approved/Rejected/Canceled), `UserRole` (Employee/Manager/Admin) |

### Rules:
- ✅ Can reference other Domain files
- ❌ Cannot reference Application, Infrastructure, or any NuGet package

---

## Layer 2 — Application (`LeaveFlow.Application`)

### What is it?
The **use-case layer**. It orchestrates what happens when a user does something — "submit a leave request", "approve a request", "login". It does NOT know about HTTP or databases directly.

### Think of it as:
The kitchen manager who takes an order, checks the rules, tells the fridge what to fetch, and coordinates the response. They don't cook (that's Infrastructure) and they don't talk to customers (that's the API).

### Key concepts:

#### CQRS (Command Query Responsibility Segregation)
Split every operation into two types:
- **Command** = changes data (Create, Approve, Cancel, Login)
- **Query** = reads data (GetMyLeaves, GetBalances, GetPendingRequests)

Each has its own handler class. They never mix.

#### MediatR
A messenger library. The API sends a `Command` or `Query` object to MediatR, and MediatR finds the right `Handler` class to process it. The API never calls handlers directly.

```
API → mediator.Send(new CreateLeaveRequestCommand(...))
                        ↓
              CreateLeaveRequestHandler.Handle(...)
```

#### Result\<T\>
Every handler returns `Result<T>` — either `Result.Success(value)` or `Result.Failure("error message")`. No exceptions thrown for business errors.

### What's inside:

| Folder/File | What it does |
|-------------|-------------|
| `Common/Result.cs` | The success/failure wrapper returned by every handler |
| `Interfaces/IRepositories.cs` | Contracts for all database operations — Application defines WHAT it needs, Infrastructure provides HOW |
| `Interfaces/IServices.cs` | Contracts for JWT, Email, and Password hashing |
| `DTOs/Dtos.cs` | Data shapes passed between API and Application (LoginRequest, LeaveRequestDto, etc.) |
| `Features/Auth/LoginHandler.cs` | Handles login: checks credentials, returns JWT token |
| `Features/Auth/LoginValidator.cs` | FluentValidation rules for login input |
| `Features/LeaveRequests/Commands/CreateLeaveRequestHandler.cs` | Creates a leave request: validates balance, checks overlap, saves, sends email |
| `Features/LeaveRequests/Commands/ReviewLeaveRequestHandler.cs` | Manager approves or rejects: updates status, deducts balance, sends email |
| `Features/LeaveRequests/Commands/CancelLeaveRequestHandler.cs` | Employee cancels a pending request |
| `Features/LeaveRequests/Queries/LeaveRequestQueries.cs` | Read-only queries: my requests, pending team requests, balances |
| `Features/LeaveRequests/LeaveRequestValidator.cs` | Validates leave request input |
| `DependencyInjection.cs` | Registers MediatR and FluentValidation with the DI container |

### Rules:
- ✅ Can reference Domain
- ❌ Cannot reference Infrastructure or API
- ❌ Cannot use `DbContext`, `BCrypt`, `MailKit` directly

---

## Layer 3 — Infrastructure (`LeaveFlow.Infrastructure`)

### What is it?
The **technical implementation layer**. It talks to the database, sends emails, hashes passwords, and generates JWT tokens. It implements the interfaces defined in Application.

### Think of it as:
The fridge, the suppliers, and the delivery drivers. The kitchen manager (Application) says "get me the user by email" — Infrastructure goes to the actual database and fetches it.

### What's inside:

| Folder/File | What it does |
|-------------|-------------|
| `Persistence/AppDbContext.cs` | EF Core database context — maps entities to SQL tables, defines unique indexes |
| `Repositories/UserRepository.cs` | DB operations for Users (find by email, find by id, get team) |
| `Repositories/LeaveRequestRepository.cs` | DB operations for leave requests (overlap check, pending by team, etc.) |
| `Repositories/LeaveBalanceRepository.cs` | DB operations for leave balances |
| `Repositories/MiscRepositories.cs` | AuditLog and IdempotencyKey DB operations |
| `Repositories/UnitOfWork.cs` | Wraps `SaveChangesAsync` — all changes save together or not at all |
| `Services/JwtService.cs` | Generates JWT access tokens and refresh tokens |
| `Services/EmailService.cs` | Sends emails via SMTP using MailKit |
| `Services/PasswordHasher.cs` | Hashes and verifies passwords using BCrypt |
| `DependencyInjection.cs` | Registers all repos, services, and DbContext with DI |

### Rules:
- ✅ Can reference Domain and Application
- ❌ API should not bypass Application and call Infrastructure directly (except DI registration)

---

## Layer 4 — API (`LeaveFlow.API`)

### What is it?
The **HTTP entry point** of the backend. It receives HTTP requests, validates auth, calls MediatR, and returns HTTP responses. It knows nothing about business rules.

### Think of it as:
The waiter. Takes the order from the customer (Blazor UI), passes it to the kitchen (Application), brings back the result. Doesn't cook anything.

### What's inside:

| File | What it does |
|------|-------------|
| `Controllers/AuthController.cs` | `POST /api/auth/login` — public endpoint, returns JWT |
| `Controllers/LeaveRequestsController.cs` | All leave request endpoints (create, cancel, review, balances) |
| `Controllers/AdminController.cs` | `GET /api/admin/auditlogs` — Admin only |
| `Middleware/ExceptionMiddleware.cs` | Catches any unhandled exception and returns a clean JSON error |
| `Program.cs` | Wires everything: JWT auth, Swagger + Bearer UI, CORS, Serilog, DI |
| `appsettings.json` | Config: DB connection string, JWT secret, email settings, CORS origin |

### Rules:
- ✅ Can reference Application and Infrastructure (for DI only)
- ❌ Should not contain business logic
- ❌ Should not call repositories directly

---

## Layer 5 — Blazor UI (`LeaveFlow.BlazorUI`)

### What is it?
The **web frontend**. A Blazor Server application that calls the API over HTTP, stores the JWT in browser local storage, and renders role-aware pages. It runs on `https://localhost:5002`.

### Think of it as:
The customer sitting at the table. They see a menu (the UI), place an order (click a button), and the waiter (API) handles the rest. The customer never goes into the kitchen.

### What's inside:

| Folder/File | What it does |
|-------------|-------------|
| `Components/Layout/MainLayout.razor` | Shell layout — checks auth on every render, shows sidebar + topbar, redirects to `/login` if JWT is missing or expired |
| `Components/Layout/NavMenu.razor` | Sidebar navigation — filters menu items by role (Employee/Manager/Admin), highlights active page, logout button |
| `Components/Layout/TopBar.razor` | Top bar — shows current page title, user avatar with initials, role badge |
| `Components/Layout/EmptyLayout.razor` | Bare layout used only by the Login page (no sidebar/topbar) |
| `Components/Pages/Login.razor` | Email + password form, calls API, stores JWT, redirects to dashboard |
| `Components/Pages/Dashboard.razor` | Stat cards (vacation days, sick days, pending, approved), balance progress bars, recent requests table |
| `Components/Pages/MyLeaves.razor` | Filterable table of all leave requests, cancel button for pending ones |
| `Components/Pages/CreateLeave.razor` | Leave request form with real-time balance sidebar, day count, overlap/balance validation |
| `Components/Pages/Approvals.razor` | Manager view — pending team requests table, approve/reject modal with comment |
| `Components/Pages/Admin/AuditLogs.razor` | Paginated audit log table with page size selector |
| `Components/Pages/Admin/Users.razor` | User stats cards, user table, add user modal |
| `Components/Pages/Profile.razor` | Edit name/email, change password with mismatch validation |
| `Services/AuthService.cs` | Stores/retrieves JWT via Blazored.LocalStorage, checks token expiry, extracts role and name |
| `Services/ApiService.cs` | Typed HttpClient wrapper — auto-injects Bearer header, handles all 8 API endpoints, adds Idempotency-Key on POST |
| `wwwroot/app.css` | Full design system — CSS custom properties, sidebar, stat cards, tables, forms, badges, modals, toasts, spinner |

### Rules:
- ✅ Can reference Application (for DTOs and enums only)
- ✅ Communicates with API via HTTP — never calls repositories or handlers directly
- ❌ No business logic — all rules enforced in Application/Domain

---

## How a Request Flows End-to-End

### Example: Employee submits a leave request

```
1. Blazor UI — CreateLeave.razor
   User fills form, clicks "Submit Leave Request"
   ApiService generates Guid idempotency key
   POST https://localhost:5001/api/leaverequests
   Headers: Authorization: Bearer <jwt>, Idempotency-Key: <uuid>
   Body: { startDate, endDate, leaveType, comments }

2. LeaveRequestsController (API)
   Extracts UserId from JWT claims
   Checks idempotency key — already processed? return cached response
   Calls: mediator.Send(new CreateLeaveRequestCommand(userId, dto))

3. CreateLeaveRequestHandler (Application)
   Validates dates (end >= start)
   Checks for overlapping requests via ILeaveRequestRepository
   Checks balance via ILeaveBalanceRepository
   Creates LeaveRequest entity
   Adds AuditLog entry
   Calls IUnitOfWork.SaveChangesAsync() → one DB transaction
   Calls IEmailService.SendAsync() → sends confirmation email
   Returns Result<int>.Success(newRequestId)

4. Controller
   Result.IsSuccess → return 201 Created with the new ID

5. Blazor UI
   Shows success alert, refreshes balance sidebar
   User navigates to /my-leaves to see the new request
```

---

## Dependency Rules (who can talk to who)

```
LeaveFlow.Domain         ← no dependencies
LeaveFlow.Application    ← Domain only
LeaveFlow.Infrastructure ← Domain + Application
LeaveFlow.API            ← Application + Infrastructure
LeaveFlow.BlazorUI       ← Application (DTOs only) + API (via HTTP)
```

**Never go backwards. Never skip layers.**

---

## Key Patterns Explained Simply

### Repository Pattern
Instead of writing `db.LeaveRequests.Where(...)` in your handler, you call `_leaveRequestRepo.GetByEmployeeAsync(id)`. The handler doesn't know or care if it's SQL Server, PostgreSQL, or an in-memory list. Easy to test, easy to swap.

### Unit of Work
All your database changes (add request + add audit log) are saved in **one single call** to `SaveChangesAsync`. If one fails, nothing saves. Keeps data consistent.

### CQRS
Commands change data. Queries read data. They never share a handler. This keeps code small, focused, and easy to test individually.

### Result\<T\>
No `throw new Exception("balance too low")`. Instead: `return Result<int>.Failure("Insufficient leave balance.")`. The controller checks `result.IsSuccess` and returns the right HTTP status. Clean, predictable, testable.

### Idempotency
If the network drops after the server processes a request but before the client gets the response, the client might retry. The `Idempotency-Key` header ensures the server returns the same cached response instead of creating a duplicate record.

### Auth Guard (Blazor)
`MainLayout.razor` runs `Auth.IsAuthenticatedAsync()` on every `OnAfterRenderAsync`. If the JWT is missing or expired, it immediately redirects to `/login`. No page ever renders for unauthenticated users.

---

## Quick Reference: Where to Add New Features

| What you're adding | Where it goes |
|--------------------|---------------|
| New entity / enum | `LeaveFlow.Domain` |
| New business rule | `LeaveFlow.Domain` entity or `LeaveFlow.Application` handler |
| New API endpoint | `LeaveFlow.API/Controllers` + new Command/Query in `Application` |
| New DB table | `Domain` entity → `Infrastructure/AppDbContext` → EF migration |
| New email template | `Infrastructure/Services/EmailService` |
| New validation rule | `Application/Features/.../Validator` |
| New UI page | `LeaveFlow.BlazorUI/Components/Pages` + route in `@page` directive |
| New nav menu item | `LeaveFlow.BlazorUI/Components/Layout/NavMenu.razor` |
| New API service method | `LeaveFlow.BlazorUI/Services/ApiService.cs` |
