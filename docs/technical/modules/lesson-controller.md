# LessonController

`CodeAcademyWeb/Controllers/LessonController.cs`, route `api/Lesson`. Contract: Swagger (`/openapi/v1.json`).

| Dependency | Implementation |
|------------|----------------|
| `ILessonsService` | `AcademyPersistance/Services/EFLessonsService.cs` |
| `IMapper` | `CodeAcademyWeb/Profiles/LessonProfile.cs` |

## Endpoints

### `GET /api/Lesson/lessonByEdition/{id}` — `FindLessonForEditionId`

- **Flow**: the route `id` (edition id) is passed to `ILessonsService.FindLessonForEditionId(id)` → `IEnumerable<Lesson>` mapped to `IEnumerable<LessonDTO>` → 200.
- **Mapping**: `Lesson` ↔ `LessonDTO` by property name (`Id`, `Subject`, `CourseEditionId`, `ClassroomId`). `Start` and `End` are not exposed (commented out in the DTO).
- **Handled errors**: a non-numeric `id` fails model binding and `[ApiController]` returns 400 without calling the service.
- **Known issues**: none. `ILessonsService.FindLessonInRange` has no endpoint (commented out in the controller).

## Tests

| Test | Level | Expected result |
|------|-------|-----------------|
| `LessonControllerTests.FindLessonForEditionId_WithLessons_ReturnsOkWithMappedDTOs` | Unit | Green |
| `LessonControllerTests.FindLessonForEditionId_AnyId_PassesIdToService` | Unit | Green |
| `LessonControllerTests.FindLessonForEditionId_NoLessons_ReturnsOkWithEmptyList` | Unit | Green |
| `LessonHttpTests.FindLessonForEditionId_WithLessons_Returns200WithCamelCaseJson` | Http | Green |
| `LessonHttpTests.FindLessonForEditionId_RouteId_BindsIdToService` | Http | Green |
| `LessonHttpTests.FindLessonForEditionId_NoLessons_Returns200WithEmptyArray` | Http | Green |
| `LessonHttpTests.FindLessonForEditionId_NonNumericId_Returns400` | Http | Green |
