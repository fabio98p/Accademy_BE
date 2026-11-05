# GET by id senza 404

- Commit: 0249d66
- Posizione: CodeAcademyWeb/Controllers/CourseController.cs:35, CodeAcademyWeb/Controllers/StudentController.cs:59

`GET` Course e Student by id restituiscono 200 con body vuoto se l'entità non esiste, invece di 404.
