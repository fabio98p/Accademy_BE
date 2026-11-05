# Risposta errata del DELETE Student

- Commit: 0249d66
- Posizione: CodeAcademyWeb/Controllers/StudentController.cs:81

`DELETE` Student legge lo studente dopo averlo cancellato e mappa il risultato sull'entità `Student` invece che su `StudentDTO`.
