public interface IExchangeRateSource
{
    public IEnumerable<ExchangeRate> GetExchangeRates(IEnumerable<Currency> currencies);
}