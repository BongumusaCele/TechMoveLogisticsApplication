# TechMove Logistics GLMS

## Run The Application

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
2. Set `TechMoveLogisticsApplication` as the startup project.
3. Press `F5` or click Run.
4. Open the URL shown by Visual Studio.

### 3. Run From The Command Line

From the project root:

```powershell
dotnet restore
dotnet run --project TechMoveLogisticsApplication.csproj
```

Then open the URL shown in the terminal.

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
