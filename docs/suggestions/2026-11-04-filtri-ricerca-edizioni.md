# Filtri errati nella ricerca edizioni

- Commit: caf09b7
- Posizione: AcademyPersistance/Repository/EFEditionRepository.cs:36-63

Il filtro `InstructorId` confronta `e.Id` invece dell'id dell'istruttore. Con `Start` ed `End` entrambi valorizzati, il filtro su `End` riparte da `ctx.CourseEditions` e annulla quello su `Start`.
