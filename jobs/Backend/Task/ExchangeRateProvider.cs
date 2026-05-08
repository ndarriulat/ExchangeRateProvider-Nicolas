using System.Collections.Generic;
using System.Linq;

namespace ExchangeRateUpdater
{
    public class ExchangeRateProvider
    {
        /// <summary>
        /// Should return exchange rates among the specified currencies that are defined by the source. But only those defined
        /// by the source, do not return calculated exchange rates. E.g. if the source contains "CZK/USD" but not "USD/CZK",
        /// do not return exchange rate "USD/CZK" with value calculated as 1 / "CZK/USD". If the source does not provide
        /// some of the currencies, ignore them.
        /// </summary>
        public IEnumerable<ExchangeRate> GetExchangeRates(IEnumerable<Currency> currencies)
        {
            var cnbRates = GetCnbRates();
            var filteredRates = GetFilteredRates(cnbRates, currencies);
            return filteredRates;
        }

        private IEnumerable<ExchangeRate> GetCnbRates()
        {
            var cnbRates = new List<ExchangeRate>();
            return cnbRates;
        }

        private IEnumerable<ExchangeRate> GetFilteredRates(IEnumerable<ExchangeRate> cnbRates, IEnumerable<Currency> currencies)
        {
            return cnbRates.Where(rate => currencies.Contains(rate.SourceCurrency) || currencies.Contains(rate.TargetCurrency));
        }
    }
}

// Hit the CNB public URL and download the daily rates text file

// Parse it — skip the header lines, split by |, normalise Rate/Amount

// Filter down to only the currencies the caller asked for

// Return them as ExchangeRate objects in the format the skeleton defines
