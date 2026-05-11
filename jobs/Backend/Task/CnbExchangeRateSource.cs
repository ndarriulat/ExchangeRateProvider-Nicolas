using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace ExchangeRateUpdater
{
    public class CnbExchangeRateSource : IExchangeRateSource
    {
        private const int CnbHeaderLineCount = 2;

        private readonly HttpClient _httpClient;
        private readonly IOptions<CnbOptions> _options;

        public CnbExchangeRateSource(HttpClient httpClient, IOptions<CnbOptions> options)
        {
            _httpClient = httpClient;
            _options = options;
        }

        public async Task<IEnumerable<ExchangeRate>> GetExchangeRates(IEnumerable<Currency> currencies)
        {
            _ = currencies;
            var response = await _httpClient.GetAsync(_options.Value.DailyKurzUrl).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return ParseCnbDailyKurz(content).ToList();
        }

        private static IEnumerable<ExchangeRate> ParseCnbDailyKurz(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                yield break;
            }

            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length <= CnbHeaderLineCount)
            {
                yield break;
            }

            for (var i = CnbHeaderLineCount; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                var parts = line.Split('|');
                if (parts.Length < 5)
                {
                    continue;
                }

                var amountText = parts[2].Trim();
                var code = parts[3].Trim();
                var rateText = parts[4].Trim();

                if (string.IsNullOrEmpty(code))
                {
                    continue;
                }

                if (!decimal.TryParse(amountText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var amount)
                    || amount <= 0)
                {
                    continue;
                }

                if (!TryParseCnbDecimal(rateText, out var rate))
                {
                    continue;
                }

                var value = rate / amount;
                yield return new ExchangeRate(new Currency(code), new Currency("CZK"), value);
            }
        }

        private static bool TryParseCnbDecimal(string text, out decimal value)
        {
            // Avoid AllowThousands: CNB English files use "." as decimal; a comma must not be read as a thousands separator.
            const NumberStyles rateStyles = NumberStyles.Number & ~NumberStyles.AllowThousands;

            if (decimal.TryParse(text, rateStyles, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }

            // Czech-published files often use "," as the decimal separator in this column.
            var withDotDecimal = text.Replace(',', '.');
            if (decimal.TryParse(withDotDecimal, rateStyles, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }

            return decimal.TryParse(text, NumberStyles.Number, CultureInfo.GetCultureInfo("cs-CZ"), out value);
        }
    }
}