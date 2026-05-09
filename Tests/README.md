# GLMS Test Project

This folder contains the xUnit tests for the TechMove Logistics GLMS application.

The test project is named `Tests.csproj` and is stored in this short folder path to avoid the Windows 260-character path limit that can affect Visual Studio when a project is nested deeply.

## Run Tests

From the repository root:

```powershell
dotnet restore Tests\Tests.csproj
dotnet test Tests\Tests.csproj
```

## Current Test Coverage

- Currency conversion math from USD to ZAR
- PDF file validation
- Rejection of restricted file types
- Service request validation against invalid contract workflow states

## Visual Studio

Open the main solution:

```text
TechMoveLogisticsApplication.sln
```

The correct test project should appear as:

```text
Tests
```

If the old `TechMoveLogisticsApplication.Tests` project still appears as load failed, remove it from Solution Explorer and reopen the solution.

