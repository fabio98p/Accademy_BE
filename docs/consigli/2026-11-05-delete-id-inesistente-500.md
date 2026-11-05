# DELETE con id inesistente restituisce 500

- Commit: faea812
- Posizione: AcademyPersistance/Repository/EFCrudRepository.cs:29-39, AcademyPersistance/Services/EFEnrollmentsService.cs:60

`DELETE /api/Course/{id}`, `/api/Student/{id}` e `/api/Enrollment/{id}` con id inesistente chiamano `Remove(null)`: viene lanciata `ArgumentNullException` e la risposta è 500 invece di 404.
