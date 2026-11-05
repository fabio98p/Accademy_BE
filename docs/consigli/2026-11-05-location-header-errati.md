# Location header errati

- Commit: 0249d66
- Posizione: CodeAcademyWeb/Controllers/CourseController.cs:47, CodeAcademyWeb/Controllers/EnrollmentController.cs:50

`Created` usa percorsi che non corrispondono alle route: `/api/courses/{id}` invece di `/api/Course/{id}`, `/api/edition/{id}` per un'iscrizione.
