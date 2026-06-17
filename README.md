# TechMove Logistics GLMS

## Run The Application

## Part 3 Service-Oriented Setup

This solution now has three parts:

- `TechMoveLogisticsApplication` - MVC presentation layer.
- `TechMoveLogisticsAPI` - ASP.NET Core Web API service layer.
- `Tests` - unit tests plus opt-in HTTP integration tests.

The contracts screens in the MVC app call the Web API through `HttpClient`. The API owns the contract database operations and exposes:

```text
GET   /api/contracts
GET   /api/contracts/{id}
POST  /api/contracts
PUT   /api/contracts/{id}
PATCH /api/contracts/{id}/status
GET   /api/clients
GET   /api/clients/{id}
POST  /api/clients
GET   /api/service-requests
POST  /api/service-requests
GET   /api/invoices
GET   /api/audit
GET   /api/dashboard
POST  /api/auth/token
```

OpenAPI is available from the API at:

```text
http://localhost:5014/swagger
http://localhost:5014/openapi/v1.json
```

### 1. Prepare The Database

The app uses this database name:

```text
TechMoveDB
```

To create the database manually, run this script in SQL Server Management Studio:

```text
DatabaseScripts/001_CreateTechMoveDB_Schema.sql
```

The connection string is in:

```text
appsettings.json
appsettings.Development.json
```

If you want to run the app without SQL Server for a quick demo, set this in `appsettings.json`:

```json
"UsePrototypeMemoryStore": true
```

### 2. Run In Visual Studio

1. Open `TechMoveLogisticsApplication.sln`.
2. Set both `TechMoveLogisticsAPI` and `TechMoveLogisticsApplication` as startup projects.
3. Start the API first, then the MVC app.
4. Open the MVC URL shown by Visual Studio.

### 3. Run From The Command Line

From the project root:

```powershell
dotnet restore
dotnet run --project ..\TechMoveLogisticsAPI\TechMoveLogisticsAPI\TechMoveLogisticsAPI.csproj
```

In a second terminal from the project root:

```powershell
dotnet run --project TechMoveLogisticsApplication.csproj
```

Then open the URL shown in the terminal.

## Run With Docker

From the MVC project root:

```powershell
docker compose up --build
```

Then open:

```text
MVC: http://localhost:8080
API: http://localhost:5014/openapi/v1.json
SQL Server: localhost,14333
```

The compose setup runs the required three containers on an internal Docker bridge network:

```text
sql-server-db       -> SQL Server database
glms-backend-api    -> ASP.NET Core Web API, connects to sql-server-db
glms-frontend-web   -> MVC frontend, connects to glms-backend-api
```

Internal service communication uses Docker DNS names:

```text
API database connection: sql-server-db:1433
MVC API connection:      http://glms-backend-api:8080/
```

### 4. Login Details

Default admin account:

```text
Email: musa@admin.co.za
Password: Admin@12345
```

The admin account is created automatically when the app starts.

## Run Tests

From the project root:

```powershell
dotnet restore Tests\Tests.csproj
dotnet test Tests\Tests.csproj
```

To run the HTTP integration tests against a running API:

```powershell
$env:TECHMOVE_RUN_API_INTEGRATION_TESTS="true"
$env:TECHMOVE_API_BASE_URL="http://localhost:5014/"
$env:TECHMOVE_API_KEY="dev-techmove-api-key"
dotnet test Tests\Tests.csproj
```

If a running app locks the build output, use:

```powershell
dotnet test Tests\Tests.csproj --configuration Release -p:UseSharedCompilation=false
```

## Optional Demo Reset

To clear demo data and allow the app to seed fresh records again, run:

```text
DatabaseScripts/002_ResetTechMoveDB_Data_Optional.sql
```

Only run this script when you intentionally want to delete existing demo data.
