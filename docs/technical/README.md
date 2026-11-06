# Technical documentation

REST backend of the Accademy: manages areas, courses, editions, instructors, students, enrollments and lessons on PostgreSQL.

## Index

- [Architecture](architecture.md): projects, dependencies, DI, request flow, error handling, test levels
- [Decisions](decisions.md): architectural decisions with links to the plans in `docs/func-history/`
- Controllers (`modules/`):
  - [AreaController](modules/area-controller.md)
  - [InstructorController](modules/instructor-controller.md)
  - [LessonController](modules/lesson-controller.md)
  - [EnrollmentController](modules/enrollment-controller.md)
  - [EditionController](modules/edition-controller.md)
  - [CourseController](modules/course-controller.md)
  - [StudentController](modules/student-controller.md)

The API contract (routes, bodies, status codes) is in the Swagger spec: `/openapi/v1.json`, UI at `/swagger` (Development environment only). This documentation does not duplicate it.

## Stack

| Area | Technology |
|------|------------|
| Runtime | .NET 10, ASP.NET Core (MVC controllers) |
| Persistence | EF Core 10 + Npgsql, NodaTime for dates |
| Database | PostgreSQL 18 |
| Mapping | AutoMapper (Profiles in `CodeAcademyWeb/Profiles`) |
| OpenAPI | `Microsoft.AspNetCore.OpenApi` + Swagger UI |
| Tests | xUnit v3, NSubstitute, `Microsoft.AspNetCore.Mvc.Testing` |

## Tests

The `CodeAcademyWeb.Tests/` project (in the `CodeAcademy.sln` solution) has two levels, told apart by the `Category` trait:

| Folder | Category | What it checks |
|--------|----------|----------------|
| `Controllers/` | `Unit` | Controller method called directly: `IActionResult` type, status, mapped DTO, `Location`, service calls |
| `Http/` | `Http` | In-memory API with `WebApplicationFactory`: routing, binding, real status, headers, JSON |

Support code in `Support/`:
- `MapperFactory`: real `IMapper` built from the `CodeAcademyWeb` Profiles.
- `ApiFactory`: in-memory host with the services (`I*Service`) replaced by NSubstitute mocks. The database is never used.

The EF services and repositories have no tests: they would need integration tests on a real PostgreSQL.

`global.json` in the root sets Microsoft.Testing.Platform as the `dotnet test` runner, as required by xUnit v3 on the .NET 10 SDK.

### Running

The tests do not need PostgreSQL to be running.

All tests:

```bash
dotnet test
```

Unit tests only:

```bash
dotnet test --filter Category=Unit
```

HTTP tests only:

```bash
dotnet test --filter Category=Http
```

### Tests that fail on purpose

The tests check the expected correct behaviour, not the current one. The endpoints marked `BROKEN` in Swagger and the DELETEs by non-existent id have red tests: they stay red until the related suggestion in `docs/suggestions/` is resolved. The list is in the controller files in `modules/` and in the plan [2026-11-05-test-e-doc-tecnica-chiamate](../func-history/2026-11-05-test-e-doc-tecnica-chiamate.md).
