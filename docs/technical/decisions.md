# Decisions

## 2026-11-05 — Controller tests and technical documentation

Plan: [2026-11-05-test-e-doc-tecnica-chiamate](../func-history/2026-11-05-test-e-doc-tecnica-chiamate.md)

| Decision | Rationale | Rejected alternatives |
|----------|-----------|-----------------------|
| xUnit v3 + NSubstitute, native xUnit asserts | FluentAssertions 8 has a commercial license | FluentAssertions |
| Unit tests with the real `IMapper` (project Profiles) | The contract depends on both controller and mapping | Mocked `IMapper`: tautological tests |
| HTTP tests with `WebApplicationFactory<Startup>` and services mocked in DI | They cover routing, binding, real status codes, JSON, `Location`, `[ApiController]` validation without a DB | End-to-end tests with a DB |
| No tests for EF services and repositories | InMemory/SQLite do not replicate Postgres and NodaTime | EF tests with InMemory/SQLite |
| A single `CodeAcademyWeb.Tests/` project, `Controllers/` and `Http/` folders, `Category` trait | Configuration and dependencies in one place; levels can be run separately | Two test projects |
| Tests check the expected contract: those on `BROKEN` endpoints fail | Bugs become visible without changing the code | Tests aligned to the current behaviour |
| `StudentController` built with its current constructor, without injecting `IEnrollmentsService` | The never-assigned service bug stays visible | Injection via reflection |
| `global.json` with the Microsoft.Testing.Platform runner | xUnit v3 4.x no longer supports VSTest with `dotnet test` on the .NET 10 SDK | Downgrade to xUnit v3 3.x |
| Explicit reference to `Microsoft.EntityFrameworkCore.Relational` 10.0.14 in the test project | Npgsql brings EF Core 10.0.4, but `CodeAcademyWeb` is compiled against 10.0.14 | Aligning versions in `AcademyPersistance` (out of scope, recorded in `docs/suggestions/`) |
