# Migrazione da SQL Server a PostgreSQL

- **Data**: 2026-11-04
- **Progetto**: Accademy_BE
- **Progetti correlati**: nessuno
- **Stato**: In corso

## Obiettivo

Rendere il backend avviabile su Linux (CachyOS) sostituendo SQL Server con PostgreSQL, eseguito tramite `docker-compose.yml`, con il minimo impatto sul codice. Si continua a usare EF Core Migrations.

## Consigli risolti

Nessuno.

## Contesto

- Soluzione su .NET 5 (`AcademyModel`, `AcademyPersistance`, `CodeAcademyWeb`), EF Core 5.0.10, NodaTime con `SimplerSoftware.EntityFrameworkCore.SqlServer.NodaTime`.
- Connection string SQL Server scritta nel codice in due punti: `CodeAcademyWeb/Startup.cs:29` e `AcademyPersistance/EFContext/AcademyContext.cs:29` (`OnConfiguring`, che duplica anche la configurazione dei log).
- 3 migration e lo snapshot contengono tipi SQL Server (`nvarchar(max)`, `bit`, `datetime2`, `decimal(18,2)`, `SqlServer:Identity`), quindi su Postgres non si possono applicare.
- Repository e servizi non usano SQL grezzo né funzioni specifiche di SQL Server.
- Il costruttore senza parametri `AcademyContext()` non è usato da nessuna parte.
- `dotnet` non è installato; Docker e Compose sono disponibili.
- Lacune: mancano `README.md`, `docs/` (functional, technical, func-history, consigli), test unitari e Swagger. In questa feature si introduce solo il `README.md`.

## Decisioni prese

| # | Decisione | Motivazione |
|---|-----------|-------------|
| 1 | Eliminare le 3 migration SQL Server e lo snapshot, rigenerare un'unica `Initial` per Npgsql | Sono legate ai tipi SQL Server; non ci sono dati da conservare. Il flusso `dotnet ef` resta lo stesso e le migration future funzionano normalmente |
| 2 | Restare su .NET 5 | Modifica minima; l'aggiornamento tecnologico sarà una task separata |
| 3 | Pacchetti `Npgsql.EntityFrameworkCore.PostgreSQL` 5.0.10 e `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime` 5.0.10, senza override di Npgsql | La vulnerabilità di Npgsql 5.0.10 (GHSA-x9vc-6hfv-hg8c) è accettata fino alla task di aggiornamento |
| 4 | Connection string in `appsettings.Development.json` (`ConnectionStrings:AcademyDb`), letta in `Startup` | Niente valori scritti nel codice |
| 5 | Rimuovere `OnConfiguring` da `AcademyContext`; `dotnet ef` usa `-s CodeAcademyWeb` | Il contesto viene configurato solo tramite DI; non serve una design-time factory |
| 6 | `dotnet-ef` 5.0.17 come tool locale (`.config/dotnet-tools.json`) | Versione fissata nel repository |
| 7 | `postgres:17-alpine`, porta 5432, db `AccademyDb`, utente/password `academy`/`academy`, volume con nome | Credenziali solo per lo sviluppo; Npgsql 5 è più collaudato su PG 17 |
| 8 | `HasPrecision(18, 2)` su `Course.BasePrice`, `Instructor.PayRate`, `Edition.RealPrice` | Stesso comportamento di prima (`numeric(18,2)`) |
| 9 | README con installazione dell'SDK tramite AUR `dotnet-sdk-5.0-bin` | Scelta dell'utente |

## Soluzione scelta

Sostituire il provider EF Core (`UseSqlServer` → `UseNpgsql` con `UseNodaTime()`), spostare la connection string in configurazione, rigenerare la migration iniziale e aggiungere un `docker-compose.yml` con il solo Postgres.

Alternative scartate:
- Adattare a mano le 3 migration esistenti: più lavoro e più rischio di errori, senza benefici perché non ci sono dati.
- Aggiornare a .NET 10: rimandato a una task dedicata.

## Fasi implementative

### Fase 1: Postgres con Docker
- [x] Creare `docker-compose.yml` in root con il servizio `postgres` (`postgres:17-alpine`, `POSTGRES_DB=AccademyDb`, `POSTGRES_USER=academy`, `POSTGRES_PASSWORD=academy`, porta `5432:5432`, volume `pgdata:/var/lib/postgresql/data`, healthcheck `pg_isready`)

### Fase 2: Provider e configurazione
- [x] `AcademyPersistance/AcademyEfPersistance.csproj`: rimuovere `Microsoft.EntityFrameworkCore.SqlServer` e `SimplerSoftware.EntityFrameworkCore.SqlServer.NodaTime`; aggiungere `Npgsql.EntityFrameworkCore.PostgreSQL` 5.0.10 e `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime` 5.0.10. Lasciare invariato `Microsoft.EntityFrameworkCore.Tools` 5.0.10
- [ ] `CodeAcademyWeb/CodeAcademyWeb.csproj`: aggiungere `Microsoft.EntityFrameworkCore.Design` 5.0.10 se `dotnet ef` lo richiede sullo startup project (da verificare in fase di implementazione)
- [x] `CodeAcademyWeb/appsettings.Development.json`: aggiungere `ConnectionStrings:AcademyDb` = `Host=localhost;Port=5432;Database=AccademyDb;Username=academy;Password=academy`
- [x] `CodeAcademyWeb/Startup.cs`: `opt.UseNpgsql(Configuration.GetConnectionString("AcademyDb"), x => x.UseNodaTime())`, mantenendo `LogTo` e `EnableSensitiveDataLogging`
- [x] `AcademyPersistance/EFContext/AcademyContext.cs`: rimuovere `OnConfiguring`; in `OnModelCreating` aggiungere `HasPrecision(18, 2)` sui 3 campi decimal

### Fase 3: Migration
- [ ] Creare `.config/dotnet-tools.json` con `dotnet-ef` 5.0.17
- [ ] Eliminare `AcademyPersistance/Migrations/*` (3 migration, Designer e snapshot)
- [ ] `dotnet ef migrations add Initial -p AcademyPersistance -s CodeAcademyWeb`
- [ ] Verificare che la migration generata usi i tipi di Postgres (`text`, `boolean`, `date`, `timestamp`, `numeric(18,2)`, identity `Npgsql:ValueGenerationStrategy`) e contenga l'indice unico `(StudentId, CourseEditionId)` su `Enrollments`

### Fase 4: README
- [ ] Creare `README.md` (skill `procedure`): prerequisiti (Docker, SDK .NET 5 tramite AUR `dotnet-sdk-5.0-bin`), `docker compose up -d`, `dotnet tool restore`, `dotnet ef database update -p AcademyPersistance -s CodeAcademyWeb`, `dotnet dev-certs https`, `dotnet run --project CodeAcademyWeb --launch-profile CodeAcademyWeb`, comando per creare una nuova migration. I link a `docs/functional` e `docs/technical` restano da aggiungere quando la documentazione verrà introdotta

### Consigli registrati in pianificazione
- `docs/consigli/2026-11-04-dotnet5-eol-e-vulnerabilita-npgsql.md`
- `docs/consigli/2026-11-04-upgrade-tecnologico-completo.md`

## Impatti

- **Codice**: `AcademyPersistance/AcademyEfPersistance.csproj`, `AcademyPersistance/EFContext/AcademyContext.cs`, `CodeAcademyWeb/Startup.cs`, `CodeAcademyWeb/appsettings.Development.json`, eventualmente `CodeAcademyWeb/CodeAcademyWeb.csproj`
- **Nuovi file**: `docker-compose.yml`, `.config/dotnet-tools.json`, `README.md`
- **API / Swagger**: nessuna modifica
- **Dati**: database da SQL Server a PostgreSQL; migration azzerate e rigenerate (`Initial`). Nomi di tabelle e colonne invariati (PascalCase, tra virgolette in Postgres)
- **Documentazione funzionale**: nessuna (assente, non introdotta)
- **Documentazione tecnica**: nessuna (assente, non introdotta)

## Test

- Il progetto non ha test unitari e non vengono introdotti in questa feature. Verifica manuale:
  - `dotnet build` della soluzione senza errori
  - `docker compose up -d` e container `healthy`
  - `dotnet ef database update` crea lo schema in `AccademyDb`
  - Avvio dell'app e chiamata a un endpoint di lettura (es. lista aree o corsi) senza errori di connessione
  - Inserimento di un'iscrizione duplicata (stesso studente e stessa edizione): deve fallire per l'indice unico

## Criteri di accettazione

- [ ] `docker compose up -d` avvia Postgres 17
- [ ] La soluzione compila su Linux con l'SDK .NET 5
- [ ] Nessun riferimento a SQL Server nel codice e nei csproj
- [ ] La connection string viene letta da `appsettings.Development.json`
- [ ] `dotnet ef database update` applica la migration `Initial` su Postgres
- [ ] L'app si avvia e legge e scrive sul database
- [ ] `README.md` descrive setup e avvio in sviluppo

## Note di implementazione

(Compilato da `/add-feature`.)
