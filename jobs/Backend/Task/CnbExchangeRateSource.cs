using System.Collections.Generic;
using ExchangeRateUpdater;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;

namespace ExchangeRateUpdater
{
    public class CnbExchangeRateSource : IExchangeRateSource
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<CnbOptions> _options;

        public CnbExchangeRateSource(HttpClient httpClient, IOptions<CnbOptions> options)
        {
            _httpClient = httpClient;
            _options = options;
        }

        public async Task<IEnumerable<ExchangeRate>> GetExchangeRates(IEnumerable<Currency> currencies)
        {
            var response = await _httpClient.GetAsync(_options.Value.DailyKurzUrl).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return new List<ExchangeRate>();    
        }
    }
}