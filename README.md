# TechMove Logistics GLMS

Global Logistics Management System (GLMS) is an ASP.NET Core MVC prototype for TechMove Logistics. The system centralises client contracts, signed agreement files, service requests, invoices, audit events, and USD-to-ZAR cost conversion in one SQL Server-backed web application.

## Project Purpose

TechMove Logistics previously relied on spreadsheets, emails, phone calls, and fragmented manual processes. This prototype replaces that workflow with a structured web application that supports:

- Contract management with client details, service levels, status tracking, and signed PDF agreements.
- Service request processing linked to valid contracts.
- Currency conversion for international service request costs.
- Invoice generation from service requests.
- Audit tracking for important contract and request events.
- Unit tests for business logic and file validation.

## Technology Stack

- ASP.NET Core MVC on .NET 10
- Entity Framework Core 10
- SQL Server / SQL Server Express
- xUnit for unit testing
- Visual Studio / .NET CLI
- HTML, CSS, Razor views
- HttpClient for exchange-rate API consumption

## NuGet Packages

Main MVC project:

- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.InMemory`

Test project:

- `xunit`
- `xunit.runner.visualstudio`
- `Microsoft.NET.Test.Sdk`
- `coverlet.collector`

## Solution Structure

```text
TechMoveLogisticsApplication/
  Controllers/              MVC controllers
  Data/                     EF Core DbContext and seed data
  Models/                   Domain entities and enums
  Services/                 Business logic, patterns, file storage, currency
  ViewModels/               View-specific models
  Views/                    Razor UI pages
  wwwroot/                  CSS, static assets, uploaded files
  Tests/                    xUnit test project
  TechMoveLogisticsApplication.csproj
  TechMoveLogisticsApplication.sln
```

## Features

### Dashboard

- Displays operational totals for clients, contracts, service requests, invoices, and recent activity.
- Uses a modern logistics-style sidebar layout inspired by fleet/shipment dashboards.

### Client Management

- Create and view client records.
- Store name, contact details, and region.
- Link each client to one or more contracts.

### Contract Management

- Create standard, international, and premium contracts.
- Store contract start date, end date, status, service level, and type-specific fields.
- Upload a signed agreement PDF for each contract.
- Download uploaded agreement files from the contract details page.
- Search and filter contracts by status and date range using LINQ.
- Edit existing contracts while keeping the selected contract type stable.

### Service Request Processing

- Create service requests against contracts.
- Prevent requests when the parent contract is expired, on hold, or outside the valid date range.
- Convert USD amounts to ZAR and save both values.
- Create related invoices from approved request data.

### Financial Integration

- Uses `HttpClient` to call an external USD exchange-rate API.
- Converts USD service request costs to ZAR.
- Falls back to a safe configured default rate if the API is unavailable.

### Audit and Notifications

- Records important business events such as contract creation, status changes, and service request activity.
- Uses observer-style services for audit, notification, and SLA monitoring behaviour.

## Design Patterns Used

### Factory Method

Contract creation is handled through contract factories instead of directly creating all contract types inside controllers.

Used for:

- `StandardContract`
- `InternationalContract`
- `PremiumContract`

This keeps contract creation logic cleaner and makes it easier to add future contract types.

### Strategy

Validation and invoicing rules are separated into strategy classes.

Used for:

- Active contract validation
- SLA validation
- International request validation
- Local and international invoice rules

This allows the system to choose business rules based on contract or request context.

### Observer

Contract and workflow events are published to observer services.

Used for:

- Audit logging
- Notification behaviour
- SLA monitoring

This keeps event reactions separate from the main business workflow.

## Database Setup

The project currently uses SQL Server with this database name:

```text
TechMoveDB
```

The connection string is stored in `appsettings.json` and `appsettings.Development.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=BONGUMUSA\\SQLEXPRESS;Initial Catalog=TechMoveDB;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Application Name=SQL Server Management Studio;Command Timeout=0"
}
```

The app uses Entity Framework Core `EnsureCreatedAsync`, so the database tables are created automatically when the app starts if the database is reachable.

If you want to run without SQL Server temporarily, set this in `appsettings.json`:

```json
"UsePrototypeMemoryStore": true
```

That switches the app to an in-memory EF Core database for prototype testing.

## How To Run The Web App

### Option 1: Visual Studio

1. Open `TechMoveLogisticsApplication.sln`.
2. Make sure SQL Server Express is running.
3. Confirm the `TechMoveDB` database exists in SQL Server Management Studio.
4. Set `TechMoveLogisticsApplication` as the startup project.
5. Press `F5` or click Run.

### Option 2: .NET CLI

From the project root, run:

```powershell
dotnet restore
dotnet run --project TechMoveLogisticsApplication.csproj
```

The terminal will show the local URL, usually similar to:

```text
https://localhost:5001
http://localhost:5000
```

Open the shown URL in your browser.

## How To Run Tests

The test project was intentionally placed in the short `Tests` folder to avoid the Windows 260-character path limit in Visual Studio.

From the project root, run:

```powershell
dotnet restore Tests\Tests.csproj
dotnet test Tests\Tests.csproj
```

Expected result:

```text
Passed: 4
Failed: 0
Total: 4
```

Tests currently cover:

- USD-to-ZAR currency calculation
- PDF-only file validation
- Invalid file type rejection
- Service request validation for contract workflow rules

## Visual Studio Test Project Note

If Visual Studio shows an old failed project called `TechMoveLogisticsApplication.Tests`, remove that failed project from Solution Explorer. The correct test project is:

```text
Tests\Tests.csproj
```

Then reopen `TechMoveLogisticsApplication.sln`.

## Common Issues

### The contract date range is not currently valid

This means the selected contract is not active for today's date. A service request can only be created when:

- The contract status allows requests.
- The current date is between the contract start date and end date.

Edit the contract dates or choose another active contract.

### Cannot connect to SQL Server

Check that:

- SQL Server Express is running.
- The server name is `BONGUMUSA\SQLEXPRESS`.
- The database `TechMoveDB` exists.
- Windows Authentication is allowed.
- `TrustServerCertificate=True` is still present in the connection string.

### Uploaded agreement file fails

Only `.pdf` files are allowed for signed agreements. Files such as `.exe`, `.docx`, or images are rejected by the file validation service.

## Important Paths

- Main solution: `TechMoveLogisticsApplication.sln`
- Main project: `TechMoveLogisticsApplication.csproj`
- Test project: `Tests\Tests.csproj`
- Uploaded contract PDFs: `wwwroot/uploads/contracts`
- Data protection keys: `App_Data/DataProtectionKeys`

