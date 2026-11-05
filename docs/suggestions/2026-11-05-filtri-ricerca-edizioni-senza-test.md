# Filtri della ricerca edizioni senza unit test

- Commit: faea812
- Posizione: AcademyPersistance/Repository/EFEditionRepository.cs:31-64

`Search` non ha unit test: i filtri lavorano direttamente su `ctx.CourseEditions` e sulla data di sistema, quindi non si possono testare senza DB. Non è esposta da nessun endpoint: si testerà quando verrà creato un endpoint di ricerca.
