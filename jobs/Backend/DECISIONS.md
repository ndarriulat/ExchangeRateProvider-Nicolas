# Engineering Decision Log

This file records lightweight technical decisions made during this task.

## Project docs and AI collaboration defaults

### Context

We want consistent planning and decision history, and a predictable way to collaborate with AI assistants (step-by-step discussion, no silent architectural choices).

### Decision

- Keep the **versioned implementation plan** in `[PLAN.md](PLAN.md)` under `jobs/Backend` and update it when the plan changes.
- Record **substantive technical choices** (libraries, new types, major tradeoffs) in this file, `[DECISIONS.md](DECISIONS.md)`.
- Encode collaboration preferences for this repo in `[.cursor/rules/collaboration-and-docs.mdc](../../.cursor/rules/collaboration-and-docs.mdc)` (`alwaysApply: true`): one step at a time; do not decide libraries or structure without explicit maintainer agreement.

### Why this choice

- `PLAN.md` is reviewable in git and independent of IDE-generated plan files.
- `DECISIONS.md` stays the single place for “why we chose X.”
- Cursor rules give stable defaults for future sessions without repeating instructions.

### Consequences

- When scope or design shifts, edit `PLAN.md` and add a short entry here if the shift reflects a real decision.
- Optional Cursor plans under `.cursor/plans/` may still exist; treat `PLAN.md` as authoritative unless the team agrees otherwise.

## No root `AGENTS.md` (for now)

### Context

We considered adding a root-level `AGENTS.md` for discoverability and cross-tool conventions, versus relying only on Cursor project rules and backend docs.

### Decision

- **Do not add** a repository root `AGENTS.md` at this stage.
- Treat `**[.cursor/rules/collaboration-and-docs.mdc](../../.cursor/rules/collaboration-and-docs.mdc)`**, `**[PLAN.md](PLAN.md)**`, and this `**[DECISIONS.md](DECISIONS.md)**` as the authoritative places for agent/collaboration defaults and task planning.

### Why this choice

- **Avoid duplication and drift** between `AGENTS.md` and `.cursor/rules` if both repeated the same instructions.
- **Cursor’s primary hook** for persistent guidance is `.cursor/rules/`; a second root file does not add much for Cursor-only workflows on a small repo.
- **Optional later:** a **thin** root `AGENTS.md` that only **links** to the files above is still valid if we want better visibility for people or tools that expect that filename—without copying rule text.

### Consequences

- Anyone looking for “how agents should work here” should open `.cursor/rules/` and `jobs/Backend/PLAN.md` / `DECISIONS.md`.
- If we add `AGENTS.md` later, keep it as a short index (links only) unless we explicitly deprecate overlap with `.cursor/rules`.

## CNB daily rates URL in `appsettings.json` and HTTP via `IHttpClientFactory`

### Context

`CnbExchangeRateSource` needs the public CNB daily rates document URL. We also want to avoid creating a new `HttpClient` on every call (socket churn and related issues).

### Decision

- Store the CNB **daily kurz** URL in `**appsettings.json`** (under the Task project, e.g. `[Task/appsettings.json](Task/appsettings.json)`), read through `**IConfiguration**`, so the endpoint can change without editing code.
- Use `**IHttpClientFactory**` (e.g. `AddHttpClient` / typed or named client registration) to obtain `**HttpClient**` instances with correct **lifetime and handler pooling**, instead of `new HttpClient()` inside the source.

### Why this choice

- **Config file:** separates environment-specific or future URL changes from compiled logic; matches common .NET hosting patterns.
- **HttpClient factory:** recommended default for production-style .NET apps; addresses lifetime concerns that a per-call `new HttpClient()` does not.

### Consequences

- The console app composition root (`[Task/Program.cs](Task/Program.cs)`) should build configuration (JSON), register HTTP clients, and pass settings into `CnbExchangeRateSource` (or bind options) as part of the user’s implementation.
- Additional **NuGet** packages are expected for configuration + HTTP client extensions (e.g. `Microsoft.Extensions.Configuration.Json`, `Microsoft.Extensions.Http`, and hosting/DI primitives as needed)—exact packages follow the chosen bootstrap style (`Host`, `ServiceCollection`, etc.).

## Generic host for console bootstrap (`Host.CreateApplicationBuilder`)

### Context

The Task executable needs JSON configuration (e.g. CNB URL), options binding (`Configure<CnbOptions>`), and `AddHttpClient` registration. The main alternative is a **manual** stack: `ConfigurationBuilder` + `ServiceCollection` without a host.

### Decision

Bootstrap the console app with `**Host.CreateApplicationBuilder(args)`** (generic host): use `**builder.Configuration**` and `**builder.Services**` for options, HTTP clients, and other DI registrations, then `**Build()**` and resolve services.

### Why this choice

- **Single, conventional pipeline:** default `appsettings` loading (and environment-specific files) plus DI in one place, matching current .NET guidance for small executables.
- **Less boilerplate** than assembling `ConfigurationBuilder`, `Build()`, and `ServiceCollection` by hand for the same features.

### Alternatives considered

- **Manual `ServiceCollection` + `ConfigurationBuilder` only:** valid when minimizing dependencies or when another host already owns configuration/DI; not chosen for this Task project.

### Consequences

- Add `**Microsoft.Extensions.Hosting`** (and `**Microsoft.Extensions.Http**` for `AddHttpClient`) to the Task project; implement `[Task/Program.cs](Task/Program.cs)` around the host builder instead of only `new`ing types in `Main`.
- `Configure<CnbOptions>(builder.Configuration.GetSection(…))` and related registrations live next to other `builder.Services` calls in the composition root.

## Use `Microsoft.Extensions.Options` (`IOptions<CnbOptions>`), not a custom `IOptions` type

### Context

It is tempting to add a small project-local interface (e.g. a non-generic `IOptions` with `DailyKurzUrl`) to abstract configuration for `[CnbExchangeRateSource](Task/CnbExchangeRateSource.cs)`.

### Decision

- **Do not** introduce a custom `**IOptions`** (or similarly named) interface in the Task project for this purpose.
- Inject `**IOptions<CnbOptions>**` from `**Microsoft.Extensions.Options**` (pulled in via the hosting/options stack), bind settings with `**Configure<CnbOptions>(…)**`, and read the URL via `**options.Value**` (e.g. `**options.Value.DailyKurzUrl**`).

### Why this choice

- The name `**IOptions**` and the `**IOptions<T>**` pattern are **standard in .NET**; a local `**IOptions`** type **collides conceptually** with `**IOptions<T>`** and confuses readers, docs, and `using` resolution.
- `**Configure<CnbOptions>**` is already the chosen wiring; `**IOptions<CnbOptions>**` is the **intended** consumer API for that binding.

### Alternatives considered

- **Custom interface** (e.g. `ICnbSettings`) if we ever need an extra indirection for tests—use a **distinct name** that does not overlap `**IOptions<T>`**.

### Consequences

- `[CnbExchangeRateSource](Task/CnbExchangeRateSource.cs)` should take `**IOptions<CnbOptions>**` (plus `**HttpClient**` from `**IHttpClientFactory**`), not a hand-rolled options interface.
- Remove any obsolete `**IOptions.cs**` in `[Task](Task)` that duplicates this role.

## Composition root in `Program.cs` (no default `ExchangeRateProvider` ctor)

### Context

`ExchangeRateProvider` depends on `IExchangeRateSource`. A common shortcut is a parameterless constructor that chains to `new CnbExchangeRateSource()` (or similar) so callers can write `new ExchangeRateProvider()` without wiring.

### Decision

- **Do not** add a parameterless `ExchangeRateProvider()` that internally `new`s a concrete source.
- Treat `**[Program.cs](Task/Program.cs)`** as the **composition root**: the executable constructs the concrete `IExchangeRateSource` implementation and passes it to `new ExchangeRateProvider(IExchangeRateSource)`.

### Why this choice

- Dependencies stay **visible** at the application entry point.
- `ExchangeRateProvider` avoids **hard-coding** a single concrete source implementation.
- Aligns with explicit DI-style composition without pulling in a full container for this small task.

### Alternatives considered

- Parameterless ctor delegating to `new CnbExchangeRateSource()`: fewer lines in `Program`, but hides the dependency and couples the provider to one default implementation.

### Consequences

- Every runnable entry point must create or receive an `IExchangeRateSource` before constructing `ExchangeRateProvider`.
- Unit tests continue to inject fakes via the same constructor; no convenience ctor is required for production.

## CNB source owns CNB-specific parsing

### Context

`IExchangeRateSource` could either return the raw CNB daily rates text (for example `Task<string>`) and leave parsing to `ExchangeRateProvider`, or return normalized `ExchangeRate` objects after handling the source-specific document format.

### Decision

- `IExchangeRateSource` returns parsed `ExchangeRate` objects.
- `CnbExchangeRateSource` owns both fetching the CNB daily document and parsing that CNB-specific text format.
- `ExchangeRateProvider` should not depend on raw CNB text or a `Task<string>` fetch contract; it receives already-normalized rates from `IExchangeRateSource`.
- Keep parser helpers such as `ParseCnbDailyKurz` non-public; do not make them public only to call them from tests.
- Do **not** introduce a parser interface for now.

### Why this choice

- The CNB document shape (header rows, pipe-delimited columns, comma/dot decimal handling, amount/rate normalization) is specific to the CNB implementation.
- Keeping parsing in the concrete source keeps `ExchangeRateProvider` generic: it can orchestrate source -> filter -> return without knowing about CNB text files.
- Avoiding `Task<string>` in `ExchangeRateProvider` prevents the provider from becoming coupled to one source's transport format and keeps its responsibility focused on application-level orchestration/filtering.
- Public members should describe supported production behavior, not expose implementation details solely for test access.
- A parser interface would add ceremony before there are multiple parsers or a need to test parsing independently from the source class.

### Consequences

- Provider tests should fake `IExchangeRateSource` by returning `ExchangeRate` instances, not raw text snippets.
- CNB parsing tests belong with `CnbExchangeRateSource` and can use stubbed HTTP responses.
- Parsing coverage should exercise `CnbExchangeRateSource.GetExchangeRates(...)` with controlled HTTP responses instead of calling `ParseCnbDailyKurz` directly.
- If another bank/source is added later, it should get its own implementation that fetches and parses its own format into `ExchangeRate` objects.

## Use xUnit for unit tests

### Context

We need a first unit-test setup for the .NET backend task and must choose a test framework.

### Decision

Use `xUnit` as the default unit testing framework for `Task.Tests`.

### Why this choice

- Strong and common ecosystem support in modern .NET projects.
- Simple test model with `[Fact]` and `[Theory]` for readable test cases.
- Works smoothly with `dotnet test` and CI runners.
- Minimal setup overhead for a small task codebase.

### Detailed comparison vs NUnit and MSTest

#### xUnit

Pros

- Default-first mindset for modern .NET OSS templates and examples.
- Strong parameterized test support via `[Theory]` + data attributes.
- Constructor-based setup encourages explicit dependencies and cleaner tests.
- Good parallelization defaults, typically helping test runtime.

Cons

- If a team is historically NUnit/MSTest-heavy, migration has small friction.
- Setup/teardown style differs from classic attribute lifecycle patterns.

#### NUnit

Pros

- Very mature and feature-rich framework.
- Familiar attribute model (`[Test]`, `[SetUp]`, `[TestCase]`) for many teams.
- Great parameterized testing ergonomics and broad extension ecosystem.

Cons

- Adds another style divergence if the repo already aligns with xUnit examples.
- Attribute-heavy lifecycle can encourage broader fixture coupling in some codebases.
- For this small task, extra framework features are not required.

#### MSTest

Pros

- Microsoft-native option, recognized in enterprise .NET environments.
- Straightforward for teams already standardized on Visual Studio test conventions.
- Good integration with Azure DevOps pipelines and enterprise templates.

Cons

- Less commonly preferred in modern community samples compared with xUnit/NUnit.
- Typical test style can be more verbose for simple behavior-driven unit tests.
- Parameterized/data-driven tests are solid, but usually less ergonomic than xUnit/NUnit.

### Alternatives considered

- `NUnit`: also mature and good, but no strong project-specific reason to prefer it here.
- `MSTest`: built-in option, but often less preferred for concise, modern test ergonomics.

### Consequences

- Test examples and conventions in this repository should assume xUnit attributes and assertions.
- If we later need richer fluent assertions or mocking, we can add packages without changing the test framework.

## Keep `ExchangeRateProvider.GetExchangeRates` synchronous

### Context

The assignment-facing API already exposes `ExchangeRateProvider.GetExchangeRates(...)` as a synchronous method returning `IEnumerable<ExchangeRate>`. The concrete CNB source uses HTTP, where the natural .NET API is asynchronous.

### Decision

- Keep `ExchangeRateProvider.GetExchangeRates(...)` synchronous for this task.
- Keep `IExchangeRateSource.GetExchangeRates(...)` asynchronous so the source can use async HTTP APIs.
- Use a deliberate sync-over-async boundary inside `ExchangeRateProvider` when calling the source.

### Why this choice

- It preserves the public provider API expected by the exercise and existing callers.
- It keeps HTTP-specific async behavior inside the source abstraction instead of pushing it through the whole console app for this small task.
- The boundary is explicit and localized, so it can be changed later if the public API moves to async.

### Consequences

- `ExchangeRateProvider` blocks while waiting for the source result.
- This is acceptable for the current console assignment, but a server or high-concurrency app should prefer an async provider API such as `GetExchangeRatesAsync`.
- If the task evolves to support a fully async public surface, update `Program.cs`, provider tests, and this decision.

