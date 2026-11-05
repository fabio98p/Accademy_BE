# Documentazione tecnica

Backend REST dell'Accademy: gestisce aree, corsi, edizioni, docenti, studenti, iscrizioni e lezioni su PostgreSQL.

## Indice

- [Architettura](architecture.md): progetti, dipendenze, DI, flusso di una richiesta, gestione errori, livelli di test
- [Decisioni](decisions.md): decisioni architetturali con link ai piani in `docs/func-history/`
- Controller (`modules/`):
  - [AreaController](modules/area-controller.md)
  - [InstructorController](modules/instructor-controller.md)
  - [LessonController](modules/lesson-controller.md)
  - [EnrollmentController](modules/enrollment-controller.md)
  - [EditionController](modules/edition-controller.md)
  - [CourseController](modules/course-controller.md)
  - [StudentController](modules/student-controller.md)

Il contratto delle API (route, body, status code) è nello Swagger: `/openapi/v1.json`, UI su `/swagger` (solo in ambiente Development). Questa documentazione non lo duplica.

## Stack

| Ambito | Tecnologia |
|--------|------------|
| Runtime | .NET 10, ASP.NET Core (controller MVC) |
| Persistenza | EF Core 10 + Npgsql, NodaTime per le date |
| Database | PostgreSQL 18 |
| Mapping | AutoMapper (Profile in `CodeAcademyWeb/Profiles`) |
| OpenAPI | `Microsoft.AspNetCore.OpenApi` + Swagger UI |
| Test | xUnit v3, NSubstitute, `Microsoft.AspNetCore.Mvc.Testing` |

## Test

Il progetto `CodeAcademyWeb.Tests/` (nella soluzione `CodeAcademy.sln`) ha due livelli, distinti dal trait `Category`:

| Cartella | Categoria | Cosa verifica |
|----------|-----------|---------------|
| `Controllers/` | `Unit` | Metodo del controller chiamato direttamente: tipo di `IActionResult`, status, DTO mappato, `Location`, chiamate al servizio |
| `Http/` | `Http` | API in-memory con `WebApplicationFactory`: routing, binding, status reale, header, JSON |

Supporto in `Support/`:
- `MapperFactory`: `IMapper` reale costruito dai Profile di `CodeAcademyWeb`.
- `ApiFactory`: host in-memory con i servizi (`I*Service`) sostituiti da mock NSubstitute. Il database non viene mai usato.

I servizi EF e i repository non hanno test: servirebbero test d'integrazione su un PostgreSQL vero.

`global.json` in root imposta Microsoft.Testing.Platform come runner di `dotnet test`, come richiesto da xUnit v3 sull'SDK .NET 10.

### Esecuzione

I test non richiedono PostgreSQL avviato.

Tutti i test:

```bash
dotnet test
```

Solo gli unit test:

```bash
dotnet test --filter Category=Unit
```

Solo i test HTTP:

```bash
dotnet test --filter Category=Http
```

### Test che falliscono di proposito

I test verificano il comportamento corretto atteso, non quello attuale. Gli endpoint segnati `BROKEN` nello Swagger e i DELETE per id inesistente hanno test rossi: resteranno rossi finché il relativo consiglio in `docs/consigli/` non verrà risolto. L'elenco è nei file dei controller in `modules/` e nel piano [2026-11-05-test-e-doc-tecnica-chiamate](../func-history/2026-11-05-test-e-doc-tecnica-chiamate.md).
