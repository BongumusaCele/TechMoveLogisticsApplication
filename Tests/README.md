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
- Exchange-rate API success and fallback handling
- Strict PDF file validation, including fake PDF content and traversal attempts
- Service request validation against expired, on-hold, future-dated, and malformed requests
- End-to-end service-request workflow behavior from approval to invoice and audit logging
- Automated GitHub Actions test execution through `.github/workflows/dotnet-tests.yml`

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
