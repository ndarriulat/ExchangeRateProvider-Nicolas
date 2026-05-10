using System.Collections.Generic;
using System.Linq;
using ExchangeRateUpdater;
using Xunit;

public class ExchangeRateProviderTests
{
    [Fact]
    public void GetExchangeRates_returns_empty_when_source_returns_no_rates()
    {
        var provider = new ExchangeRateProvider();
        var rates = provider.GetExchangeRates(new List<Currency> { new Currency("USD") });

        Assert.Empty(rates);
    }

    [Fact]
    public void GetExchangeRates_returns_rates_when_source_returns_rates()
    {
        var provider = new ExchangeRateProvider();
        var rates = provider.GetExchangeRates(new List<Currency> { new Currency("USD") });

        Assert.NotEmpty(rates);
    }

}