# Engineering Decision Log

This file records lightweight technical decisions made during this task.

## 2026-05-10 - Project docs and AI collaboration defaults

### Context
We want consistent planning and decision history, and a predictable way to collaborate with AI assistants (step-by-step discussion, no silent architectural choices).

### Decision
- Keep the **versioned implementation plan** in [`PLAN.md`](PLAN.md) under `jobs/Backend` and update it when the plan changes.
- Record **substantive technical choices** (libraries, new types, major tradeoffs) in this file, [`DECISIONS.md`](DECISIONS.md).
- Encode collaboration preferences for this repo in [`.cursor/rules/collaboration-and-docs.mdc`](../../.cursor/rules/collaboration-and-docs.mdc) (`alwaysApply: true`): one step at a time; do not decide libraries or structure without explicit maintainer agreement.

### Why this choice
- `PLAN.md` is reviewable in git and independent of IDE-generated plan files.
- `DECISIONS.md` stays the single place for “why we chose X.”
- Cursor rules give stable defaults for future sessions without repeating instructions.

### Consequences
- When scope or design shifts, edit `PLAN.md` and add a short entry here if the shift reflects a real decision.
- Optional Cursor plans under `.cursor/plans/` may still exist; treat `PLAN.md` as authoritative unless the team agrees otherwise.

## 2026-05-10 - No root `AGENTS.md` (for now)

### Context
We considered adding a root-level `AGENTS.md` for discoverability and cross-tool conventions, versus relying only on Cursor project rules and backend docs.

### Decision
- **Do not add** a repository root `AGENTS.md` at this stage.
- Treat **[`.cursor/rules/collaboration-and-docs.mdc`](../../.cursor/rules/collaboration-and-docs.mdc)**, **[`PLAN.md`](PLAN.md)**, and this **[`DECISIONS.md`](DECISIONS.md)** as the authoritative places for agent/collaboration defaults and task planning.

### Why this choice
- **Avoid duplication and drift** between `AGENTS.md` and `.cursor/rules` if both repeated the same instructions.
- **Cursor’s primary hook** for persistent guidance is `.cursor/rules/`; a second root file does not add much for Cursor-only workflows on a small repo.
- **Optional later:** a **thin** root `AGENTS.md` that only **links** to the files above is still valid if we want better visibility for people or tools that expect that filename—without copying rule text.

### Consequences
- Anyone looking for “how agents should work here” should open `.cursor/rules/` and `jobs/Backend/PLAN.md` / `DECISIONS.md`.
- If we add `AGENTS.md` later, keep it as a short index (links only) unless we explicitly deprecate overlap with `.cursor/rules`.

## 2026-05-08 - Use xUnit for unit tests

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
