using Microsoft.Extensions.Logging.Abstractions;
using TechMoveLogisticsApplication.Services.Currency;

namespace TechMoveLogisticsApplication.Tests;

public class CurrencyConversionServiceTests
{
    [Fact]
    public void ConvertUsdToZar_ReturnsRoundedLocalCost()
    {
        var service = new CurrencyConversionService(new HttpClient(), NullLogger<CurrencyConversionService>.Instance);

        var result = service.ConvertUsdToZar(125.25m, 18.75m);

        Assert.Equal(2348.44m, result);
    }

    [Fact]
    public async Task GetUsdToZarRateAsync_ReturnsRateFromApiPayload()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler("""{"rates":{"ZAR":19.8765}}"""));
        var service = new CurrencyConversionService(httpClient, NullLogger<CurrencyConversionService>.Instance);

        var result = await service.GetUsdToZarRateAsync();

        Assert.Equal(19.8765m, result);
    }

    [Fact]
    public async Task GetUsdToZarRateAsync_FallsBackWhenApiFails()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(null));
        var service = new CurrencyConversionService(httpClient, NullLogger<CurrencyConversionService>.Instance);

        var result = await service.GetUsdToZarRateAsync();

        Assert.Equal(18.50m, result);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly string? _responseJson;

        public StubHttpMessageHandler(string? responseJson)
        {
            _responseJson = responseJson;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_responseJson is null)
            {
                throw new HttpRequestException("Simulated exchange-rate outage.");
            }

            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(_responseJson)
            });
        }
    }
}
