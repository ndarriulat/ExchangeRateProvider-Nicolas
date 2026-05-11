# Test Cases

This file documents the current test coverage for the backend exchange-rate task.

## Test Files


| File                                                               | Scope                  | Purpose                                                                                                            |
| ------------------------------------------------------------------ | ---------------------- | ------------------------------------------------------------------------------------------------------------------ |
| `jobs/Backend/Task.Tests/ExchangeRateProviderTests.cs`             | Unit                   | Public `ExchangeRateProvider.GetExchangeRates(...)` behavior with a fake `IExchangeRateSource`.                    |
| `jobs/Backend/Task.Tests/ExchangeRateProviderFilteringTests.cs`    | Unit                   | Private `GetFilteredRates(...)` filtering behavior, invoked through reflection.                                    |
| `jobs/Backend/Task.Tests/CnbExchangeRateSourceIntegrationTests.cs` | Integration-style unit | `CnbExchangeRateSource` HTTP response handling and CNB daily document parsing with a stubbed `HttpMessageHandler`. |


## Provider Unit Test Matrix

These tests exercise `ExchangeRateProvider.GetExchangeRates(...)` through its public API.


| Test case                                           | Method under test                            | Source rates                   | Requested currencies           | Expected result                                                |
| --------------------------------------------------- | -------------------------------------------- | ------------------------------ | ------------------------------ | -------------------------------------------------------------- |
| Source returns no rates                             | `ExchangeRateProvider.GetExchangeRates(...)` | None                           | `USD`                          | Empty result.                                                  |
| Both currencies requested                           | `ExchangeRateProvider.GetExchangeRates(...)` | `USD/CZK = 25`                 | `USD`, `CZK`                   | Returns the `USD/CZK` rate.                                    |
| Requested currency has same code as source currency | `ExchangeRateProvider.GetExchangeRates(...)` | `new Currency("USD")/CZK = 25` | separate `USD`, `CZK`          | Returns the rate because `Currency` equality is based on code. |
| No rate matches request                             | `ExchangeRateProvider.GetExchangeRates(...)` | `GBP/JPY = 150`                | `USD`                          | Empty result.                                                  |
| Only unknown requested currencies                   | `ExchangeRateProvider.GetExchangeRates(...)` | `USD/CZK = 25`                 | `XYZ`                          | Empty result.                                                  |
| Multiple source rates, one complete requested pair  | `ExchangeRateProvider.GetExchangeRates(...)` | `USD/CZK = 25`, `EUR/CZK = 24` | `USD`, `CZK`                   | Returns only `USD/CZK`.                                        |
| Target currency requested with some sources         | `ExchangeRateProvider.GetExchangeRates(...)` | `USD/CZK`, `EUR/CZK`, `JPY/CZK`| `USD`, `EUR`, `CZK`            | Returns `USD/CZK` and `EUR/CZK`; excludes `JPY/CZK`.           |
| Mix of known and unknown requested currencies       | `ExchangeRateProvider.GetExchangeRates(...)` | `USD/CZK = 25`                 | `USD`, `CZK`, `XYZ`            | Returns `USD/CZK`; ignores `XYZ`.                              |
| Source publishes single direction                   | `ExchangeRateProvider.GetExchangeRates(...)` | `USD/CZK = 25`                 | `USD`, `CZK`                   | Returns only `USD/CZK`; does not synthesize `CZK/USD`.         |
| Empty request                                       | `ExchangeRateProvider.GetExchangeRates(...)` | `USD/CZK = 25`                 | None                           | Empty result.                                                  |


## Filtering Unit Test Matrix

These tests exercise the provider's private `GetFilteredRates(...)` method directly through reflection.


| Test case                                        | Method under test                            | Input rates                    | Requested currencies | Expected result                                           |
| ------------------------------------------------ | -------------------------------------------- | ------------------------------ | -------------------- | --------------------------------------------------------- |
| Returns only rates whose both currency codes are requested | `ExchangeRateProvider.GetFilteredRates(...)` | `USD/CZK = 25`, `EUR/CZK = 24` | `USD`, `CZK`         | Returns only `USD/CZK`.                                   |
| Ignores unknown requested currencies             | `ExchangeRateProvider.GetFilteredRates(...)` | `USD/CZK = 25`                 | `XYZ`                | Empty result.                                             |
| Does not create inverse pairs                    | `ExchangeRateProvider.GetFilteredRates(...)` | `USD/CZK = 25`                 | `USD`, `CZK`         | Returns only the source-provided `USD/CZK`; no `CZK/USD`. |


## CNB Source Integration Test Matrix

These tests exercise `CnbExchangeRateSource.GetExchangeRates(...)` using controlled HTTP responses.
Filtering by requested currencies is intentionally not covered here; the source returns parsed rows and the provider filters them.


| Scenario                                     | Method under test                             | HTTP body shape                                | Expected parsed rates                  |
| -------------------------------------------- | --------------------------------------------- | ---------------------------------------------- | -------------------------------------- |
| `single_row_amount_one_dot_decimal`          | `CnbExchangeRateSource.GetExchangeRates(...)` | CNB headers plus `USA|dollar|1|USD|20.648`     | `USD/CZK = 20.648`                     |
| `amount_100_normalises_to_per_unit`          | `CnbExchangeRateSource.GetExchangeRates(...)` | CNB headers plus `Japan|yen|100|JPY|13.204`    | `JPY/CZK = 0.13204`                    |
| `multiple_rows`                              | `CnbExchangeRateSource.GetExchangeRates(...)` | CNB headers plus USD and EUR rows              | `USD/CZK = 20.648`, `EUR/CZK = 24.305` |
| `comma_decimal_czech_style`                  | `CnbExchangeRateSource.GetExchangeRates(...)` | CNB headers plus `USA|dollar|1|USD|20,648`     | `USD/CZK = 20.648`                     |
| `malformed_pipe_row_skipped_valid_rows_kept` | `CnbExchangeRateSource.GetExchangeRates(...)` | Malformed row, valid USD row, empty pipe row   | Keeps only `USD/CZK = 1`.              |
| `empty_body`                                 | `CnbExchangeRateSource.GetExchangeRates(...)` | Empty HTTP body                                | Empty result.                          |
| `headers_only_no_data_rows`                  | `CnbExchangeRateSource.GetExchangeRates(...)` | CNB headers without data rows                  | Empty result.                          |
| `whitespace_trimmed_in_fields`               | `CnbExchangeRateSource.GetExchangeRates(...)` | CNB headers plus fields padded with whitespace | `USD/CZK = 10`                         |


## CNB Source Error Handling Matrix


| Test case          | Method under test                             | HTTP status     | Expected result                |
| ------------------ | --------------------------------------------- | --------------- | ------------------------------ |
| HTTP request fails | `CnbExchangeRateSource.GetExchangeRates(...)` | `404 Not Found` | Throws `HttpRequestException`. |


