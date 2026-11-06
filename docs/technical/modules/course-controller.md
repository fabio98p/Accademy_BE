# CourseController

`CodeAcademyWeb/Controllers/CourseController.cs`, route `api/Course`. Contract: Swagger (`/openapi/v1.json`).

| Dependency | Implementation |
|------------|----------------|
| `ICoursesService` | `AcademyPersistance/Services/EFCoursesService.cs` |
| `IMapper` | `CodeAcademyWeb/Profiles/CourseProfile.cs` |

## Mapping

- `Course` → `CourseDTO`: `CreationDate` as `yyyy-MM-dd` (`ToLocalDateString`); `AreaName` by flattening from `Area.Name`; `Level` (enum) as `int`.
- `CourseDTO` → `Course`: `CreationDate` parsed from `yyyy-MM-dd` (`LocalDateExtensions.Parse`). If it is missing the mapping leaves the default `0001-01-01`. `AreaName` is ignored.

## Endpoints

### `GET /api/Course` — `GetAllCourse`

- **Flow**: `GetAllCourses()` → `IEnumerable<CourseDTO>` → 200.

### `GET /api/Course/lastCourse/{n}` — `GetLastNCurses`

- **Flow**: route `n` → `GetLastCourses(n)` → `IEnumerable<CourseDTO>` → 200.

### `GET /api/Course/{id}` — `GetCourseById`

- **Flow**: `GetCourseById(id)` → `CourseDTO` → 200.
- **Known issues**: with a non-existent id it returns `Ok(null)`, which ASP.NET Core sends as 204 (`docs/suggestions/2026-11-05-get-by-id-senza-404.md`). Expected: 404 `ErrorObject`.

### `POST /api/Course` — `CreateCourse`

- **Flow**: `CourseDTO` → `Course` → `CreateCourse(course)` → returned course mapped → 201.
- **Known issues**:
  - The `Location` header is `/api/courses/{id}` instead of `/api/Course/{id}` (`docs/suggestions/2026-11-05-location-header-errati.md`).
  - `CreationDate` is not set server-side: without a value it is `0001-01-01`, otherwise the client value is kept (`docs/suggestions/2026-11-05-creationdate-corso-non-valorizzata.md`). Expected: always today's date.

### `PUT /api/Course` — `UpdateCourse`

- **Flow**: `CourseDTO` → `Course` → `UpdateCourse(course)` → returned course mapped.
- **Known issues**: returns 201 Created with `Location` `/api/courses/{id}` instead of 200 OK (`docs/suggestions/2026-11-05-put-restituiscono-201.md`).

### `DELETE /api/Course` (body) — `RemoveCourse(CourseDTO)`

- **Flow**: `CourseDTO` → `Course` → `DeleteCourse(Course)` → the same course mapped back → 200.

### `DELETE /api/Course/{id}` — `RemoveCourse(long)`

- **Flow**: `GetCourseById(id)` reads the course, `DeleteCourse(id)` deletes it, the course read before is mapped → 200.
- **Known issues**: with a non-existent id `EFCrudRepository.Delete(K id)` calls `Remove(null)` and the `ArgumentNullException` becomes a 500 (`docs/suggestions/2026-11-05-delete-id-inesistente-500.md`). Expected: 404 `ErrorObject`.

## Tests

"Today" in the tests is the local `DateTime.Today`, read before the call. `TestData.AssertIsToday` also accepts the following day if the test runs across midnight.

| Test | Level | Expected result |
|------|-------|-----------------|
| `CourseControllerTests.GetAllCourse_WithCourses_ReturnsOkWithMappedDTOs` | Unit | Green |
| `CourseControllerTests.GetAllCourse_NoCourses_ReturnsOkWithEmptyList` | Unit | Green |
| `CourseControllerTests.GetLastNCurses_AnyN_PassesNToServiceAndReturnsMappedDTOs` | Unit | Green |
| `CourseControllerTests.GetLastNCurses_NoCourses_ReturnsOkWithEmptyList` | Unit | Green |
| `CourseControllerTests.GetCourseById_ExistingId_ReturnsOkWithMappedDTO` | Unit | Green |
| `CourseControllerTests.GetCourseById_NonExistentId_ReturnsNotFoundWithErrorObject` | Unit | Red on purpose |
| `CourseControllerTests.CreateCourse_ValidCourse_ReturnsCreatedWithMappedDTO` | Unit | Green |
| `CourseControllerTests.CreateCourse_ValidCourse_LocationPointsToCourse` | Unit | Red on purpose |
| `CourseControllerTests.CreateCourse_WithoutCreationDate_SetsTodayAsCreationDate` | Unit | Red on purpose |
| `CourseControllerTests.CreateCourse_WithClientCreationDate_SetsTodayAsCreationDate` | Unit | Red on purpose |
| `CourseControllerTests.UpdateCourse_ValidCourse_ReturnsOkWithMappedDTO` | Unit | Red on purpose |
| `CourseControllerTests.RemoveCourse_WithBody_ReturnsOkWithDTOAndDeletesCourse` | Unit | Green |
| `CourseControllerTests.RemoveCourse_ExistingId_ReturnsOkWithDTOAndDeletesCourse` | Unit | Green |
| `CourseControllerTests.RemoveCourse_NonExistentId_ReturnsNotFoundWithErrorObject` | Unit | Red on purpose |
| `CourseHttpTests.GetAllCourse_WithCourses_Returns200WithCamelCaseJson` | Http | Green |
| `CourseHttpTests.GetAllCourse_NoCourses_Returns200WithEmptyArray` | Http | Green |
| `CourseHttpTests.GetLastNCurses_RouteN_BindsNToService` | Http | Green |
| `CourseHttpTests.GetLastNCurses_NoCourses_Returns200WithEmptyArray` | Http | Green |
| `CourseHttpTests.GetCourseById_ExistingId_Returns200WithCourse` | Http | Green |
| `CourseHttpTests.GetCourseById_NonExistentId_Returns404WithErrorObject` | Http | Red on purpose |
| `CourseHttpTests.CreateCourse_ValidCourse_Returns201WithBody` | Http | Green |
| `CourseHttpTests.CreateCourse_ValidCourse_LocationPointsToCourse` | Http | Red on purpose |
| `CourseHttpTests.CreateCourse_WithoutCreationDate_SetsTodayAsCreationDate` | Http | Red on purpose |
| `CourseHttpTests.CreateCourse_WithClientCreationDate_SetsTodayAsCreationDate` | Http | Red on purpose |
| `CourseHttpTests.UpdateCourse_ValidCourse_Returns200WithBody` | Http | Red on purpose |
| `CourseHttpTests.RemoveCourse_WithBody_Returns200WithBodyAndDeletesCourse` | Http | Green |
| `CourseHttpTests.RemoveCourse_ExistingId_Returns200WithBodyAndDeletesCourse` | Http | Green |
| `CourseHttpTests.RemoveCourse_NonExistentId_Returns404WithErrorObject` | Http | Red on purpose |
