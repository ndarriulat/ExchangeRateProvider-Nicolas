using System.Collections.Generic;
using ExchangeRateUpdater;

namespace ExchangeRateUpdater
{

    public interface IExchangeRateSource
    {
        public IEnumerable<ExchangeRate> GetExchangeRates(IEnumerable<Currency> currencies);
    }
}