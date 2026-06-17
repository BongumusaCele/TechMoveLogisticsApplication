using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Tests;

public class ApiIntegrationTests
{
    private const string RunIntegrationTestsVariable = "TECHMOVE_RUN_API_INTEGRATION_TESTS";

    [Fact]
    public async Task GetContracts_WhenApiIsRunning_ReturnsOkAndJson()
    {
        if (!ShouldRunIntegrationTests())
        {
            return;
        }

        using var client = CreateClient();

        using var response = await client.GetAsync("api/contracts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, json.ValueKind);
    }

    [Fact]
    public async Task AuthToken_WithDefaultAdminCredentials_ReturnsBearerToken()
    {
        if (!ShouldRunIntegrationTests())
        {
            return;
        }

        using var client = CreateClient(includeAuthentication: false);

        using var response = await client.PostAsJsonAsync("api/auth/token", new
        {
            Email = "musa@admin.co.za",
            Password = "Admin@12345"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("token", out var token));
        Assert.False(string.IsNullOrWhiteSpace(token.GetString()));
    }

    [Fact]
    public async Task CreateClient_ThenReadClient_ReturnsPersistedClient()
    {
        if (!ShouldRunIntegrationTests())
        {
            return;
        }

        using var client = CreateClient();
        var uniqueName = $"Integration Client {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

        using var createResponse = await client.PostAsJsonAsync("api/clients", new
        {
            Name = uniqueName,
            ContactDetails = "integration-test@techmove.example | +27 10 555 0199",
            Region = "Integration"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdJson = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var clientId = createdJson.GetProperty("clientId").GetInt32();
        Assert.True(clientId > 0);

        using var readResponse = await client.GetAsync($"api/clients/{clientId}");

        Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);
        var readJson = await readResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(uniqueName, readJson.GetProperty("name").GetString());
        Assert.Equal("Integration", readJson.GetProperty("region").GetString());
    }

    private static HttpClient CreateClient(bool includeAuthentication = true)
    {
        var baseUrl = Environment.GetEnvironmentVariable("TECHMOVE_API_BASE_URL") ?? "http://localhost:5014/";
        var client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(10)
        };

        if (includeAuthentication)
        {
            var apiKey = Environment.GetEnvironmentVariable("TECHMOVE_API_KEY") ?? "dev-techmove-api-key";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        return client;
    }

    private static bool ShouldRunIntegrationTests()
    {
        return string.Equals(
            Environment.GetEnvironmentVariable(RunIntegrationTestsVariable),
            "true",
            StringComparison.OrdinalIgnoreCase);
    }
}
