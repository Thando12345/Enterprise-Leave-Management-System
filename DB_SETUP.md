# Database Setup Guide — LeaveFlow Enterprise

> Complete guide to create, seed, clone, and migrate the LeaveFlowDB database.

---

## Option 1 — Fresh Setup with EF Core Migrations (Recommended)

### Step 1: Install EF Core CLI tool

```bash
dotnet tool install --global dotnet-ef
```

Verify:
```bash
dotnet ef --version
```

### Step 2: Set your connection string

Edit `src/LeaveFlow.API/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LeaveFlowDB;Trusted_Connection=True;"
}
```

For SQL Server Express:
```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=LeaveFlowDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

### Step 3: Run migrations

```bash
cd src/LeaveFlow.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../LeaveFlow.API
dotnet ef database update --startup-project ../LeaveFlow.API
```

### Step 4: Verify tables in SSMS

Connect to your server → `LeaveFlowDB` → Tables:
- `dbo.Users`
- `dbo.LeaveRequests`
- `dbo.LeaveBalances`
- `dbo.AuditLogs`
- `dbo.IdempotencyKeys`
- `dbo.__EFMigrationsHistory`

---

## Option 2 — Restore from .bak Backup

1. Open **SQL Server Management Studio (SSMS)**
2. Right-click **Databases** → **Restore Database...**
3. Source: **Device** → click `...` → Add → select `LeaveFlowDB.bak`
4. Destination database name: `LeaveFlowDB`
5. Click **OK**

---

## Option 3 — Run SQL Script

If you have a `.sql` schema+data script:

```bash
sqlcmd -S (localdb)\mssqllocaldb -i LeaveFlowDB_script.sql
```

Or open in SSMS and press **F5** to execute.

---

## Seed Data Script

Run this in SSMS after the database is created.

> Generate real BCrypt hashes first:
> ```csharp
> // Run in a .NET console app or LINQPad
> Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("Admin@123"));
> Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("Manager@123"));
> Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("Employee@123"));
> ```
> Replace the hash placeholders below with the real output.

```sql
USE LeaveFlowDB;
GO

-- =============================================
-- USERS
-- Role: 0=Employee, 1=Manager, 2=Admin
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@leaveflow.com')
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Role, TeamId)
VALUES ('admin@leaveflow.com', '<HASH_FOR_Admin@123>', 'Admin', 'User', 2, NULL);

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'manager@leaveflow.com')
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Role, TeamId)
VALUES ('manager@leaveflow.com', '<HASH_FOR_Manager@123>', 'Jane', 'Manager', 1, 1);

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'employee@leaveflow.com')
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Role, TeamId)
VALUES ('employee@leaveflow.com', '<HASH_FOR_Employee@123>', 'John', 'Employee', 0, 1);

-- =============================================
-- LEAVE BALANCES (for employee, adjust Id if needed)
-- LeaveType: 0=Vacation, 1=Sick, 2=Personal, 3=Maternity, 4=Paternity, 5=Unpaid
-- =============================================
DECLARE @EmpId INT = (SELECT Id FROM Users WHERE Email = 'employee@leaveflow.com');

IF NOT EXISTS (SELECT 1 FROM LeaveBalances WHERE EmployeeId = @EmpId AND LeaveType = 0 AND Year = 2025)
INSERT INTO LeaveBalances (EmployeeId, LeaveType, TotalDays, UsedDays, Year)
VALUES (@EmpId, 0, 20, 0, 2025);

IF NOT EXISTS (SELECT 1 FROM LeaveBalances WHERE EmployeeId = @EmpId AND LeaveType = 1 AND Year = 2025)
INSERT INTO LeaveBalances (EmployeeId, LeaveType, TotalDays, UsedDays, Year)
VALUES (@EmpId, 1, 10, 0, 2025);

IF NOT EXISTS (SELECT 1 FROM LeaveBalances WHERE EmployeeId = @EmpId AND LeaveType = 2 AND Year = 2025)
INSERT INTO LeaveBalances (EmployeeId, LeaveType, TotalDays, UsedDays, Year)
VALUES (@EmpId, 2, 5, 0, 2025);

-- Manager balances
DECLARE @MgrId INT = (SELECT Id FROM Users WHERE Email = 'manager@leaveflow.com');

IF NOT EXISTS (SELECT 1 FROM LeaveBalances WHERE EmployeeId = @MgrId AND LeaveType = 0 AND Year = 2025)
INSERT INTO LeaveBalances (EmployeeId, LeaveType, TotalDays, UsedDays, Year)
VALUES (@MgrId, 0, 20, 0, 2025);

IF NOT EXISTS (SELECT 1 FROM LeaveBalances WHERE EmployeeId = @MgrId AND LeaveType = 1 AND Year = 2025)
INSERT INTO LeaveBalances (EmployeeId, LeaveType, TotalDays, UsedDays, Year)
VALUES (@MgrId, 1, 10, 0, 2025);

PRINT 'Seed data inserted successfully.';
GO
```

---

## Adding Future Migrations

When you change a Domain entity:

```bash
cd src/LeaveFlow.Infrastructure

# Create migration
dotnet ef migrations add <Name> --startup-project ../LeaveFlow.API

# Apply
dotnet ef database update --startup-project ../LeaveFlow.API

# Rollback last migration (if needed)
dotnet ef migrations remove --startup-project ../LeaveFlow.API
```

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| `No project was found` | Make sure you're in `src/LeaveFlow.Infrastructure` |
| `Unable to create migrations` | Check `AppDbContext` constructor uses `DbContextOptions<AppDbContext>` |
| `Login failed for user` | Check connection string, ensure SQL Server is running |
| `Table already exists` | Database already has migrations — run `dotnet ef database update` only |
| `dotnet ef not found` | Run `dotnet tool install --global dotnet-ef` |
| LocalDB not starting | Run `sqllocaldb start mssqllocaldb` in terminal |
