# Backend Solution Guide

This guide is the starting point for reading the implemented .NET backend task. The original assignment files are still present; this file explains what was built, where the supporting documents live, and how to run the solution locally.

## What This Implements

The solution implements an `ExchangeRateProvider` backed by the Czech National Bank daily exchange-rate feed. The concrete CNB source downloads the published text document, parses it into normalized `ExchangeRate` objects, and the provider filters those rates for requested source currencies.

CNB publishes rates as foreign currency against CZK. For example, requesting `USD` returns the source-provided `USD/CZK` rate; the caller does not need to request `CZK` separately.

## Where To Start

1. [`DotNet.md`](DotNet.md) - the original .NET assignment.
2. [`SOLUTION.md`](SOLUTION.md) - this guide and local setup instructions.
3. [`docs/PLAN.md`](docs/PLAN.md) - the implementation plan and high-level flow.
4. [`docs/DECISIONS.md`](docs/DECISIONS.md) - the engineering decisions and tradeoffs.
5. [`docs/TEST_CASES.md`](docs/TEST_CASES.md) - the documented test matrix.

## Document Map

| Document | Purpose |
| --- | --- |
| [`Readme.md`](Readme.md) | Backend task index that links to the available backend exercises. |
| [`SOLUTION.md`](SOLUTION.md) | Reader-oriented guide for this implementation and local commands. |
| [`DotNet.md`](DotNet.md) | Original task description for the .NET implementation. |
| [`docs/PLAN.md`](docs/PLAN.md) | Current implementation plan, flow, checklist, and scope notes. |
| [`docs/DECISIONS.md`](docs/DECISIONS.md) | Decision log explaining the main design choices and consequences. |
| [`docs/TEST_CASES.md`](docs/TEST_CASES.md) | Test coverage matrix for provider, filtering, and CNB source behavior. |

## Code Structure

| Path | Role |
| --- | --- |
| [`Task/ExchangeRateProvider.cs`](Task/ExchangeRateProvider.cs) | Orchestrates source retrieval, filtering, and return of exchange rates. |
| [`Task/IExchangeRateSource.cs`](Task/IExchangeRateSource.cs) | Source abstraction returning parsed exchange-rate objects. |
| [`Task/CnbExchangeRateSource.cs`](Task/CnbExchangeRateSource.cs) | Fetches and parses the CNB daily exchange-rate text document. |
| [`Task/Currency.cs`](Task/Currency.cs) | Currency value object with case-insensitive code equality. |
| [`Task/ExchangeRate.cs`](Task/ExchangeRate.cs) | Exchange-rate model with source currency, target currency, and value. |
| [`Task/Program.cs`](Task/Program.cs) | Console composition root: configuration, DI, HTTP client, and provider execution. |
| [`Task.Tests`](Task.Tests) | xUnit tests for provider behavior, filtering, and CNB source parsing. |

## Run Locally

Required tooling:

- .NET SDK 9.0 or newer, because both projects target `net9.0`.
- Internet access for the first NuGet restore and for running the console app against the live CNB endpoint.

From the repository root:

```bash
dotnet restore "jobs/Backend/Task/ExchangeRateUpdater.csproj"
dotnet run --project "jobs/Backend/Task/ExchangeRateUpdater.csproj"
```

`dotnet run` also performs restore/build automatically, but running `dotnet restore` explicitly makes missing SDK or package issues easier to see.

## Run Tests

From the repository root:

```bash
dotnet test "jobs/Backend/Task.Tests/Task.Tests.csproj"
```

The tests use xUnit. Provider tests use fake `IExchangeRateSource` implementations, while CNB source tests use stubbed HTTP responses so they do not depend on the live CNB website.

## Behavior Notes

- CNB rows are parsed as foreign currency to CZK, e.g. `USD/CZK`.
- Requested currencies are interpreted as requested source currencies.
- Unknown requested currencies are ignored.
- The provider does not synthesize inverse rates such as `CZK/USD`.
- CNB amounts greater than one, such as `100 JPY`, are normalized to a per-unit `ExchangeRate.Value`.

## Production Notes

The main production-oriented choices are documented in [`docs/DECISIONS.md`](docs/DECISIONS.md): `HttpClient` creation through `IHttpClientFactory`, configuration through `appsettings.json` and options binding, source-specific parsing inside `CnbExchangeRateSource`, and a deliberate sync-over-async boundary to preserve the assignment-facing provider API.
