using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExchangeRateUpdater
{
    public class ExchangeRateProvider
    {
        private readonly IExchangeRateSource _exchangeRateSource;

        public ExchangeRateProvider(IExchangeRateSource exchangeRateSource)
        {
            _exchangeRateSource = exchangeRateSource;
        }
        /// <summary>
        /// Should return exchange rates among the specified currencies that are defined by the source. But only those defined
        /// by the source, do not return calculated exchange rates. E.g. if the source contains "CZK/USD" but not "USD/CZK",
        /// do not return exchange rate "USD/CZK" with value calculated as 1 / "CZK/USD". If the source does not provide
        /// some of the currencies, ignore them.
        /// </summary>
        public IEnumerable<ExchangeRate> GetExchangeRates(IEnumerable<Currency> currencies)
        {
            var cnbRates = GetSourceExchangeRates(currencies);
            var filteredRates = GetFilteredRates(cnbRates, currencies);
            return filteredRates;
        }

        private IEnumerable<ExchangeRate> GetSourceExchangeRates(IEnumerable<Currency> currencies)
        {
            // Public API stays synchronous per assignment; the source uses async HTTP.
            var task = _exchangeRateSource.GetExchangeRates(currencies);
            return task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private IEnumerable<ExchangeRate> GetFilteredRates(IEnumerable<ExchangeRate> cnbRates, IEnumerable<Currency> currencies)
        {
            return cnbRates.Where(rate => currencies.Contains(rate.SourceCurrency) || currencies.Contains(rate.TargetCurrency));
        }
    }
}

// 1) Hit the CNB public URL and download the daily rates text file
// & 2) Parse it — skip the header lines, split by |, normalise Rate/Amount

// 3) Filter down to only the currencies the caller asked for

// 4) Return them as ExchangeRate objects in the format the skeleton defines
