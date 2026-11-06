# StudentController

`CodeAcademyWeb/Controllers/StudentController.cs`, route `api/Student`. Contract: Swagger (`/openapi/v1.json`).

| Dependency | Implementation |
|------------|----------------|
| `IPeopleService` | `AcademyPersistance/Services/EFPeopleService.cs` |
| `IEnrollmentsService` | Declared as a field but never assigned: the constructor only receives `IPeopleService` and `IMapper` |
| `IMapper` | `CodeAcademyWeb/Profiles/StudentProfile.cs` |

## Mapping

- `Student` ↔ `StudentDTO`: `Surname` ↔ `Lastname`; `DateOfBirth` as `yyyy-MM-dd` (`ToLocalDateString` / `Parse`); other fields by property name.
- `EnrollDataDTO` ↔ `EnrollData`: by property name (`IdStudent`, `IdEdition`).
- `Enrollment` → `EnrollmentDTO`: see [enrollment-controller.md](enrollment-controller.md#mapping-enrollment--enrollmentdto).

## Endpoints

### `GET /api/Student` — `GetAll`

- **Flow**: `GetAllStudents()` → `IEnumerable<StudentDTO>` → 200.

### `POST /api/Student` — `Create`

- **Flow**: `StudentDTO` → `Student` → `CreateStudent(student)`; the id assigned by the service on the same instance is mapped back → 201, `Location` `/api/student/{id}`.

### `PUT /api/Student` — `UpdateStudent`

- **Flow**: `StudentDTO` → `Student` → `UpdateStudent(student)` → the same instance mapped back.
- **Known issues**: returns 201 Created instead of 200 OK (`docs/suggestions/2026-11-05-put-restituiscono-201.md`).

### `GET /api/Student/{id}` — `GetStudentById`

- **Flow**: `GetStudentById(id)` → `StudentDTO` → 200.
- **Known issues**: with a non-existent id it returns `Ok(null)`, sent as 204 (`docs/suggestions/2026-11-05-get-by-id-senza-404.md`). Expected: 404 `ErrorObject`.

### `POST /api/Student/{idStudent}/enrollments` — `EnrollStudent`

- **Flow**: if the route `idStudent` differs from `IdStudent` in the body → 400 `ErrorObject`. Otherwise `EnrollDataDTO` → `EnrollData` → `IEnrollmentsService.EnrollSudentToEdition(data)` → `EnrollmentDTO` → 201, `Location` `/api/student/{idStudent}/enrollments/{enrollmentId}`.
- **Known issues**: `enrollmentsService` is never assigned, so every valid request fails with `NullReferenceException` / 500 (`docs/suggestions/2026-11-05-enrollmentsservice-mai-assegnato.md`). The `CS0649` compiler warning points to the same field.

### `DELETE /api/Student/{id}` — `DeleteStudent`

- **Flow**: `DeleteStudent(id)`, then `GetStudentById(id)`, then the result is mapped to `Student` (not `StudentDTO`) → 200.
- **Known issues**:
  - The student is read after the deletion, so the body is `null` (sent as 204) instead of the deleted student (`docs/suggestions/2026-11-05-delete-student-risposta-errata.md`). Expected: 200 with the `StudentDTO` read before the deletion.
  - With a non-existent id `EFCrudRepository.Delete(K id)` calls `Remove(null)` and the `ArgumentNullException` becomes a 500 (`docs/suggestions/2026-11-05-delete-id-inesistente-500.md`). Expected: 404 `ErrorObject`.

## Tests

The controller is built with its current constructor, without injecting `IEnrollmentsService`, so the bug stays visible. In the delete test the mock behaves like the real service: once `DeleteStudent` is called, `GetStudentById` returns `null`.

| Test | Level | Expected result |
|------|-------|-----------------|
| `StudentControllerTests.GetAll_WithStudents_ReturnsOkWithMappedDTOs` | Unit | Green |
| `StudentControllerTests.GetAll_NoStudents_ReturnsOkWithEmptyList` | Unit | Green |
| `StudentControllerTests.Create_ValidStudent_ReturnsCreatedWithLocationAndMappedDTO` | Unit | Green |
| `StudentControllerTests.UpdateStudent_ValidStudent_ReturnsOkWithMappedDTO` | Unit | Red on purpose |
| `StudentControllerTests.GetStudentById_ExistingId_ReturnsOkWithMappedDTO` | Unit | Green |
| `StudentControllerTests.GetStudentById_NonExistentId_ReturnsNotFoundWithErrorObject` | Unit | Red on purpose |
| `StudentControllerTests.EnrollStudent_IdMismatch_ReturnsBadRequestWithErrorObject` | Unit | Green |
| `StudentControllerTests.EnrollStudent_ValidData_ReturnsCreatedWithEnrollment` | Unit | Red on purpose |
| `StudentControllerTests.DeleteStudent_ExistingId_ReturnsOkWithStudentReadBeforeDeletion` | Unit | Red on purpose |
| `StudentControllerTests.DeleteStudent_NonExistentId_ReturnsNotFoundWithErrorObject` | Unit | Red on purpose |
| `StudentHttpTests.GetAll_WithStudents_Returns200WithCamelCaseJson` | Http | Green |
| `StudentHttpTests.GetAll_NoStudents_Returns200WithEmptyArray` | Http | Green |
| `StudentHttpTests.Create_ValidStudent_Returns201WithLocationAndBody` | Http | Green |
| `StudentHttpTests.UpdateStudent_ValidStudent_Returns200WithBody` | Http | Red on purpose |
| `StudentHttpTests.GetStudentById_ExistingId_Returns200WithStudent` | Http | Green |
| `StudentHttpTests.GetStudentById_NonExistentId_Returns404WithErrorObject` | Http | Red on purpose |
| `StudentHttpTests.EnrollStudent_IdMismatch_Returns400WithErrorObject` | Http | Green |
| `StudentHttpTests.EnrollStudent_ValidData_Returns201WithEnrollment` | Http | Red on purpose |
| `StudentHttpTests.DeleteStudent_ExistingId_Returns200WithStudentReadBeforeDeletion` | Http | Red on purpose |
| `StudentHttpTests.DeleteStudent_NonExistentId_Returns404WithErrorObject` | Http | Red on purpose |
