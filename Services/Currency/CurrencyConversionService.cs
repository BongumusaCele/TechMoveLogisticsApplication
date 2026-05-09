using System.Text.Json;

namespace TechMoveLogisticsApplication.Services.Currency;

public class CurrencyConversionService : ICurrencyConversionService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CurrencyConversionService> _logger;
    private const decimal FallbackUsdToZarRate = 18.50m;

    public CurrencyConversionService(HttpClient httpClient, ILogger<CurrencyConversionService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<decimal> GetUsdToZarRateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync("https://open.er-api.com/v6/latest/USD", cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (document.RootElement.TryGetProperty("rates", out var rates)
                && rates.TryGetProperty("ZAR", out var zarRate)
                && zarRate.TryGetDecimal(out var rate))
            {
                return rate;
            }
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Unable to fetch live USD/ZAR rate. Falling back to configured prototype rate.");
        }

        return FallbackUsdToZarRate;
    }

    public decimal ConvertUsdToZar(decimal amountUsd, decimal exchangeRate)
    {
        return Math.Round(amountUsd * exchangeRate, 2, MidpointRounding.AwayFromZero);
    }
}
