# Errore 404 del PUT Edition non uniforme

- Commit: 0249d66
- Posizione: CodeAcademyWeb/Controllers/EditionController.cs:77

Il `PUT` Edition restituisce il 404 come stringa, mentre gli altri errori usano `ErrorObject`.
