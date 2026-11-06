# AreaController

`CodeAcademyWeb/Controllers/AreaController.cs`, route `api/Area`. Contract: Swagger (`/openapi/v1.json`).

| Dependency | Implementation |
|------------|----------------|
| `IAreasService` | `AcademyPersistance/Services/EFAreasService.cs` |
| `IMapper` | `CodeAcademyWeb/Profiles/AreaProfile.cs` |

## Endpoints

### `GET /api/Area/areas` — `GetAllAreas`

- **Flow**: `IAreasService.GetAllAreas()` → `IEnumerable<Area>` mapped to `IEnumerable<AreaDTO>` → 200.
- **Mapping**: `Area` ↔ `AreaDTO` by property name (`Id`, `Name`, `Description`), no computed fields.
- **Handled errors**: none.
- **Known issues**: none.

## Tests

| Test | Level | Expected result |
|------|-------|-----------------|
| `AreaControllerTests.GetAllAreas_WithAreas_ReturnsOkWithMappedDTOs` | Unit | Green |
| `AreaControllerTests.GetAllAreas_NoAreas_ReturnsOkWithEmptyList` | Unit | Green |
| `AreaHttpTests.GetAllAreas_WithAreas_Returns200WithCamelCaseJson` | Http | Green |
| `AreaHttpTests.GetAllAreas_NoAreas_Returns200WithEmptyArray` | Http | Green |
