using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ExchangeRateUpdater
{
    public static class Program
    {
        private static IEnumerable<Currency> currencies = new[]
        {
            new Currency("USD"),
            new Currency("EUR"),
            new Currency("CZK"),
            new Currency("JPY"),
            new Currency("KES"),
            new Currency("RUB"),
            new Currency("THB"),
            new Currency("TRY"),
            new Currency("XYZ")
        };

        public static void Main(string[] args)
        {
            try
            {
                var builder = Host.CreateApplicationBuilder(args);

                builder.Services.Configure<CnbOptions>(
                    builder.Configuration.GetSection(CnbOptions.SectionName)); 
                builder.Services.AddHttpClient<IExchangeRateSource, CnbExchangeRateSource>();
                builder.Services.AddTransient<ExchangeRateProvider>();
                using var host = builder.Build();
                var exchangeRateProvider = host.Services.GetRequiredService<ExchangeRateProvider>();
                var rates = exchangeRateProvider.GetExchangeRates(currencies);

                Console.WriteLine($"Successfully retrieved {rates.Count()} exchange rates:");
                foreach (var rate in rates) Console.WriteLine(rate.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not retrieve exchange rates: '{e.Message}'.");
            }

            Console.ReadLine();
        }
    }
}
