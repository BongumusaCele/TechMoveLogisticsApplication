# Tests

## Run Tests

From the project root:

```powershell
dotnet restore Tests\Tests.csproj
dotnet test Tests\Tests.csproj
```

If the app is running and locks the build output, run:

```powershell
dotnet test Tests\Tests.csproj --configuration Release -p:UseSharedCompilation=false
```

## Visual Studio

Open:

```text
TechMoveLogisticsApplication.sln
```

Then run the `Tests` project from Test Explorer.
