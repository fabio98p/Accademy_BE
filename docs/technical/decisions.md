# Decisioni

## 2026-11-05 — Test dei controller e documentazione tecnica

Piano: [2026-11-05-test-e-doc-tecnica-chiamate](../func-history/2026-11-05-test-e-doc-tecnica-chiamate.md)

| Decisione | Motivazione | Alternative scartate |
|-----------|-------------|----------------------|
| xUnit v3 + NSubstitute, assert nativi di xUnit | FluentAssertions 8 ha licenza commerciale | FluentAssertions |
| Unit test con `IMapper` reale (Profile del progetto) | Il contratto dipende da controller e mapping | `IMapper` mockato: test tautologici |
| Test HTTP con `WebApplicationFactory<Startup>` e servizi mockati in DI | Coprono routing, binding, status reali, JSON, `Location`, validazione `[ApiController]` senza DB | Test e2e con DB |
| Servizi e repository EF senza test | InMemory/SQLite non replicano Postgres e NodaTime | Test EF con InMemory/SQLite |
| Un solo progetto `CodeAcademyWeb.Tests/`, cartelle `Controllers/` e `Http/`, trait `Category` | Configurazione e dipendenze in un solo posto; i livelli si lanciano separatamente | Due progetti di test |
| I test verificano il contratto atteso: quelli sugli endpoint `BROKEN` falliscono | Rendono visibili i bug senza modificare il codice | Test allineati al comportamento attuale |
| `StudentController` costruito con il costruttore attuale, senza iniettare `IEnrollmentsService` | Il bug del servizio mai assegnato resta visibile | Iniezione via reflection |
| `global.json` con runner Microsoft.Testing.Platform | xUnit v3 4.x non supporta più VSTest con `dotnet test` sull'SDK .NET 10 | Downgrade a xUnit v3 3.x |
| Riferimento esplicito a `Microsoft.EntityFrameworkCore.Relational` 10.0.14 nel progetto di test | Npgsql porta EF Core 10.0.4, ma `CodeAcademyWeb` è compilato sulla 10.0.14 | Allineare le versioni in `AcademyPersistance` (fuori ambito, registrato in `docs/consigli/`) |
