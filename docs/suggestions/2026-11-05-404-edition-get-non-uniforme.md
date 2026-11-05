# 404 del GET Edition non uniforme

- Commit: faea812
- Posizione: CodeAcademyWeb/Controllers/EditionController.cs:52

`GET /api/Edition/{id}` con id inesistente restituisce `NotFound()` (ProblemDetails) invece di un `ErrorObject` come gli altri errori.
