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
}
