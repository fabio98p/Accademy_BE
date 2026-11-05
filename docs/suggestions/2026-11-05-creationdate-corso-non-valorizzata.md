# Data di creazione del corso non valorizzata

- Commit: d369bef
- Posizione: CodeAcademyWeb/Profiles/CourseProfile.cs:19

Con un POST su `/api/Course` senza `creationDate`, il corso viene salvato con `CreationDate` = `0001-01-01`: la data di creazione non viene impostata lato server.
