namespace TechMoveLogisticsApplication.Services.Currency;

public interface ICurrencyConversionService
{
    Task<decimal> GetUsdToZarRateAsync(CancellationToken cancellationToken = default);
    decimal ConvertUsdToZar(decimal amountUsd, decimal exchangeRate);
}
