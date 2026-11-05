# Campo enrollmentsService mai assegnato

- Commit: d369bef
- Posizione: CodeAcademyWeb/Controllers/StudentController.cs:21

Il campo `enrollmentsService` non viene mai inizializzato dal costruttore (warning CS0649): se viene usato resta `null` e provoca una `NullReferenceException`.
