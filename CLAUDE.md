# hephaisto-fixture-dotnet

A **test fixture**, not a product. This repository exists so that Hephaisto's coder - the
Claude Code runner that proposes and implements fixes for incidents Hephaisto has diagnosed -
has a small, real .NET service with known bugs to work on, and so that its output can be
graded against a known-correct answer.

`Shop.Api` is a tiny ASP.NET Core minimal API: an in-memory order store, `/orders`,
`/healthz`, and a `ShopOptions` class bound from the `Shop` configuration section. `main` is
healthy. Each `fixture/<id>-<name>` branch is `main` plus one commit that introduces a bug and
the regression test that catches it; `fixtures.yaml` lists them.

## Build and test

```sh
dotnet build Shop.sln
dotnet test Shop.sln                 # xunit 2.9 on VSTest - a plain `dotnet test` works
scripts/check-fixture.sh             # asserts this branch is what fixtures.yaml says it is
docker build -t shop-api:dev .       # multi-stage, non-root, port 8080
```

Tests are the arbiter of a fix. A change is done when `dotnet test Shop.sln` is green with
every test running - skipping, deleting or weakening a test is never a fix.

## Where a fix belongs

The bugs are in the application code under `src/`. A correct fix is small, changes the code
that is wrong, and keeps the existing behaviour everywhere else.

- **`deploy/**` is not the fix.** `deploy/shop.yaml` is a sample manifest. Adding missing
  configuration to it, or raising its memory limit, hides a defect in the code rather than
  removing it; the service must behave correctly with the configuration it is given.
- **`.github/**` and the test project files are not the fix.**
- No catch-all `try`/`catch` around startup, no `Environment.Exit`, no
  `#pragma warning disable`, no `[Fact(Skip = ...)]`.
