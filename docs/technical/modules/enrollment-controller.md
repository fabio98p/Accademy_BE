# EnrollmentController

`CodeAcademyWeb/Controllers/EnrollmentController.cs`, route `api/Enrollment`. Contract: Swagger (`/openapi/v1.json`).

| Dependency | Implementation |
|------------|----------------|
| `IEnrollmentsService` | `AcademyPersistance/Services/EFEnrollmentsService.cs` |
| `IMapper` | `CodeAcademyWeb/Profiles/StudentProfile.cs` (`Enrollment` ↔ `EnrollmentDTO`) |

## Mapping `Enrollment` → `EnrollmentDTO`

- AutoMapper flattening: `StudentFirstname`, `StudentLastname` from `Student`; `CourseEditionCode` from `CourseEdition.Code`; `CourseEditionCourseTitle` from `CourseEdition.Course.Title`.
- `CourseEditionStartDate`: explicit, `CourseEdition.StartDate` as `yyyy-MM-dd`.
- `EnrollmentDate` is not exposed (commented out in the DTO).
- Reverse map `EnrollmentDTO` → `Enrollment` by property name: only `Id`, `StudentId`, `CourseEditionId`, `StudentEvaluation`, `StudentFeedback` are used.

## Endpoints

### `GET /api/Enrollment/studentSubscribed/{id}` — `GetEnrollmentByStudentId`

- **Flow**: route `id` (student id) → `GetSubscribedEnrollmentByStudentId(id)` → `IEnumerable<EnrollmentDTO>` → 200.
- **Handled errors**: none.

### `POST /api/Enrollment` — `Create`

- **Flow**: `EnrollmentDTO` → `Enrollment` → `CreateEnrollment(enrollment)` → the same instance, with the id assigned by the service, mapped back → 201.
- **Handled errors**: `EntityNotFoundException` (student or edition missing) → 400 `ErrorObject`.
- **Known issues**: the `Location` header points to `/api/edition/{id}` instead of `/api/Enrollment/{id}` (`docs/suggestions/2026-11-05-location-header-errati.md`).

### `DELETE /api/Enrollment/{id}` — `DeleteEnroll`

- **Flow**: `DeleteEnrollment(id)` → 200 without body.
- **Handled errors**: none.
- **Known issues**: with a non-existent id `EFCrudRepository.Delete(K id)` calls `Remove(null)` and the `ArgumentNullException` becomes a 500 (`docs/suggestions/2026-11-05-delete-id-inesistente-500.md`). Expected: 404 `ErrorObject`.

## Tests

The mocks simulate the real behaviour of a non-existent id: `DeleteEnrollment` throws `ArgumentNullException`. In the HTTP test, TestServer rethrows the unhandled exception to the client instead of returning 500.

| Test | Level | Expected result |
|------|-------|-----------------|
| `EnrollmentControllerTests.GetEnrollmentByStudentId_WithEnrollments_ReturnsOkWithMappedDTOs` | Unit | Green |
| `EnrollmentControllerTests.GetEnrollmentByStudentId_AnyId_PassesIdToService` | Unit | Green |
| `EnrollmentControllerTests.GetEnrollmentByStudentId_NoEnrollments_ReturnsOkWithEmptyList` | Unit | Green |
| `EnrollmentControllerTests.Create_ValidEnrollment_ReturnsCreatedWithMappedDTO` | Unit | Green |
| `EnrollmentControllerTests.Create_ValidEnrollment_LocationPointsToEnrollment` | Unit | Red on purpose |
| `EnrollmentControllerTests.Create_RelatedEntityNotFound_ReturnsBadRequestWithErrorObject` | Unit | Green |
| `EnrollmentControllerTests.DeleteEnroll_ExistingId_ReturnsOkWithoutBody` | Unit | Green |
| `EnrollmentControllerTests.DeleteEnroll_NonExistentId_ReturnsNotFoundWithErrorObject` | Unit | Red on purpose |
| `EnrollmentHttpTests.GetEnrollmentByStudentId_WithEnrollments_Returns200WithCamelCaseJson` | Http | Green |
| `EnrollmentHttpTests.GetEnrollmentByStudentId_RouteId_BindsIdToService` | Http | Green |
| `EnrollmentHttpTests.GetEnrollmentByStudentId_NoEnrollments_Returns200WithEmptyArray` | Http | Green |
| `EnrollmentHttpTests.Create_ValidEnrollment_Returns201WithBody` | Http | Green |
| `EnrollmentHttpTests.Create_ValidEnrollment_LocationPointsToEnrollment` | Http | Red on purpose |
| `EnrollmentHttpTests.Create_RelatedEntityNotFound_Returns400WithErrorObject` | Http | Green |
| `EnrollmentHttpTests.DeleteEnroll_ExistingId_Returns200WithoutBody` | Http | Green |
| `EnrollmentHttpTests.DeleteEnroll_NonExistentId_Returns404WithErrorObject` | Http | Red on purpose |
