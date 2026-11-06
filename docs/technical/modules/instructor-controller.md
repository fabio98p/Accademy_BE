# InstructorController

`CodeAcademyWeb/Controllers/InstructorController.cs`, route `api/Instructor`. Contract: Swagger (`/openapi/v1.json`).

| Dependency | Implementation |
|------------|----------------|
| `IPeopleService` | `AcademyPersistance/Services/EFPeopleService.cs` |
| `IMapper` | `CodeAcademyWeb/Profiles/InstructorProfile.cs` |

## Endpoints

### `GET /api/Instructor` — `GetAllInstructor`

- **Flow**: `IPeopleService.GetInstructors()` → `IEnumerable<Instructor>` mapped to `IEnumerable<InstructorDTO>` → 200.
- **Mapping**: `Instructor` ↔ `InstructorDTO` by property name. `DateOfBirth` (`LocalDate` → `string`) has no explicit conversion: AutoMapper falls back to `LocalDate.ToString()`, so the format depends on the culture.
- **Handled errors**: none.
- **Known issues**: date of birth format, see `docs/suggestions/2026-11-06-instructor-dateofbirth-format.md`. The tests do not check `DateOfBirth`, because the plan does not define its expected format.

## Tests

| Test | Level | Expected result |
|------|-------|-----------------|
| `InstructorControllerTests.GetAllInstructor_WithInstructors_ReturnsOkWithMappedDTOs` | Unit | Green |
| `InstructorControllerTests.GetAllInstructor_NoInstructors_ReturnsOkWithEmptyList` | Unit | Green |
| `InstructorHttpTests.GetAllInstructor_WithInstructors_Returns200WithCamelCaseJson` | Http | Green |
| `InstructorHttpTests.GetAllInstructor_NoInstructors_Returns200WithEmptyArray` | Http | Green |
