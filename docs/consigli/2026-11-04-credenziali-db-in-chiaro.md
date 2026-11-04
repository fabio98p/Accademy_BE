# Credenziali del database in chiaro

- Commit: c50b7bb
- Posizione: docker-compose.yml:6-7, CodeAcademyWeb/appsettings.Development.json (ConnectionStrings:AcademyDb, Fase 2 del piano migrazione-postgres)

Utente e password di Postgres (`academy`/`academy`) sono scritti in chiaro in file versionati. Vanno bene solo in sviluppo locale. Per gestirle: in `docker-compose.yml` leggerle da un file `.env` non versionato (`${POSTGRES_PASSWORD}`, con un `.env.example` committato); lato app usare `dotnet user-secrets` in sviluppo e variabili d'ambiente (`ConnectionStrings__AcademyDb`) o un secret manager negli altri ambienti.
