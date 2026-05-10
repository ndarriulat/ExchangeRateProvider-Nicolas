using System.Collections.Generic;
using ExchangeRateUpdater;

namespace ExchangeRateUpdater
{
    public class CnbExchangeRateSource : IExchangeRateSource
    {
        public IEnumerable<ExchangeRate> GetExchangeRates(IEnumerable<Currency> currencies)
        {
            return new List<ExchangeRate>();
        }
    }
}