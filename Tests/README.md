# Tests

## Run Tests

From the project root:

```powershell
dotnet restore Tests\Tests.csproj
dotnet test Tests\Tests.csproj
```

## API Integration Tests

The integration tests call the running Web API over HTTP. Start the API first, then opt in to the tests:

```powershell
$env:TECHMOVE_RUN_API_INTEGRATION_TESTS="true"
$env:TECHMOVE_API_BASE_URL="http://localhost:5014/"
$env:TECHMOVE_API_KEY="dev-techmove-api-key"
dotnet test Tests\Tests.csproj
```

When `TECHMOVE_RUN_API_INTEGRATION_TESTS` is not set to `true`, the integration tests are skipped so local unit test runs stay fast and reliable.

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
