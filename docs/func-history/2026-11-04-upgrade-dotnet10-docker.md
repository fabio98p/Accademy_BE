# Upgrade tecnologico a .NET 10 e build Docker

- **Data**: 2026-11-04
- **Progetto**: Accademy_BE
- **Progetti correlati**: nessuno
- **Stato**: In corso

## Obiettivo

Portare tutta la soluzione alle ultime versioni stabili (.NET 10 LTS, EF Core 10, Npgsql 10, NodaTime, AutoMapper, PostgreSQL 18, librerie front-end) modificando il codice solo dove è indispensabile. A upgrade completato, aggiungere un `Dockerfile` e due file compose: `docker-compose.yml` (API + Postgres) per buildare e avviare il progetto con Docker, `docker-compose-postgres.yml` (solo Postgres) per lo sviluppo in locale.

## Consigli risolti

- `docs/consigli/2026-11-04-dotnet5-eol-e-vulnerabilita-npgsql.md`: commit 3d2aa56. Riguarda i tre csproj: net5.0 è fuori supporto, Npgsql 5.0.10 è vulnerabile (GHSA-x9vc-6hfv-hg8c) e su Linux serve OpenSSL 1.1.
- `docs/consigli/2026-11-04-upgrade-tecnologico-completo.md`: commit 3d2aa56. Riguarda l'intera soluzione: runtime e librerie fermi al 2021, da aggiornare a .NET 10 LTS e alle versioni correnti.

## Contesto

- 3 progetti `net5.0`: `AcademyModel`, `AcademyPersistance` (`AcademyEfPersistance.csproj`), `CodeAcademyWeb`. Hosting `Program` + `Startup`, MVC + controller API, AutoMapper via DI (`AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies())`).
- EF Core 5.0.10 + Npgsql 5.0.10 + NodaTime 3.0.7. Una migration `20261104120909_Initial`. Tipi: `date` (`LocalDate`), `timestamp` (`LocalDateTime`), `numeric(18,2)`.
- I profili AutoMapper usano solo `CreateMap`/`ForMember`/`MapFrom`, nessuna API rimossa nelle versioni recenti. I DTO espongono le date come `string`.
- `docker-compose.yml` contiene solo `postgres:17-alpine`. La connection string è in `appsettings.Development.json`.
- Sistema: SDK 5.0.408 e `openssl-1.1` da AUR. `dotnet-sdk`/`aspnet-runtime` 10.0.13 disponibili in `extra`.
- Front-end statico in `wwwroot/lib`: Bootstrap 4.3.1, jQuery 3.5.1, jquery-validation, jquery-validation-unobtrusive. Usato solo da `_Layout`, Home e Privacy.
- Lacune: mancano test unitari, Swagger, `docs/functional` e `docs/technical`. Non vengono introdotti in questa feature (scelta dell'utente).

## Versioni di destinazione (verificate il 2026-11-04)

| Componente | Da | A |
|---|---|---|
| TargetFramework | net5.0 | net10.0 |
| Microsoft.EntityFrameworkCore.Tools / .Design | 5.0.10 | 10.0.14 |
| dotnet-ef (tool locale) | 5.0.17 | 10.0.14 |
| Npgsql.EntityFrameworkCore.PostgreSQL / .NodaTime | 5.0.10 | 10.0.3 |
| NodaTime | 3.0.7 | 3.3.7 |
| AutoMapper | Extensions.DI 8.1.1 | AutoMapper 16.2.0 (DI incluso) |
| PostgreSQL | 17-alpine | 18-alpine |
| Immagini Docker | — | mcr.microsoft.com/dotnet/sdk:10.0, aspnet:10.0 |
| Bootstrap | 4.3.1 | 5.3.8 |
| jQuery | 3.5.1 | 3.7.1 |
| jquery-validation | — | 1.22.1 |
| jquery-validation-unobtrusive | — | 4.0.0 |

## Decisioni prese

| # | Decisione | Motivazione |
|---|-----------|-------------|
| 1 | .NET 10 LTS | Supporto fino a nov 2028; .NET 11 è ancora RC |
| 2 | Mantenere `Program` + `Startup` (niente minimal hosting), nullable e implicit usings non attivati | Stretto indispensabile: il modello è ancora supportato |
| 3 | AutoMapper 16.2.0; `AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies())`; license key tramite variabile `AUTOMAPPER_LICENSE_KEY` (senza chiave solo warning nel log) | La 14 (MIT) ha la CVE-2026-32933, non corretta. Dalla 15 l'azione di configurazione è il primo parametro. Registrare un consiglio per sostituirlo |
| 4 | Rimuovere `AutoMapper.Extensions.Microsoft.DependencyInjection` | Inglobato in AutoMapper dalla 13 |
| 5 | `postgres:18-alpine`; volume da ricreare (`docker compose down -v`) | Ultima major. Con PG 18 cambia il percorso dati dell'immagine: montare il volume su `/var/lib/postgresql` |
| 6 | Migration: nessuna nuova se il modello non cambia; verifica con `dotnet ef migrations has-pending-model-changes`. Se segnala differenze, aggiungere la migration `UpgradeEf10` | Da EF 9 `database update` fallisce se ci sono modifiche pendenti |
| 7 | Migration applicate dall'host con `dotnet ef database update` | Nessuna modifica al codice |
| 8 | Due file compose: `docker-compose.yml` con `api` + `postgres` (`docker compose up -d --build`); `docker-compose-postgres.yml` con solo `postgres` (`docker compose -f docker-compose-postgres.yml up -d`). Stesso nome progetto, servizio `postgres` e volume `pgdata`: si usa un file alla volta | Scelta dell'utente; dati condivisi tra i due flussi, nessun conflitto sulla porta 5432 se non avviati insieme |
| 9 | Container in solo HTTP sulla 8080, `UseHttpsRedirection` invariato | Nessuna modifica al codice; resta un warning nel log |
| 10 | Connection string del container via `ConnectionStrings__AcademyDb` (Host=postgres), `ASPNETCORE_ENVIRONMENT=Development` | `appsettings.json` resta invariato |
| 11 | Front-end: Bootstrap 5.3.8, jQuery 3.7.1, jquery-validation 1.22.1, unobtrusive 4.0.0; in `_Layout` `data-toggle`/`data-target` → `data-bs-toggle`/`data-bs-target` | Scelta dell'utente; jQuery 4 scartato per la compatibilità con unobtrusive |
| 12 | README con due sezioni: **Sviluppo locale** (prerequisiti .NET 10 da `extra`, `docker-compose-postgres.yml`, migration, `dotnet run`) e **Build** (build e avvio con `docker-compose.yml`). Rimozione dei pacchetti AUR 5.0 e `openssl-1.1` | Scelta dell'utente; allineamento allo stack nuovo |

## Soluzione scelta

Upgrade in un'unica feature, con fasi successive verificabili (build dopo ogni fase). Docker viene aggiunto solo a upgrade completato.

Alternative scartate:
- .NET 11 RC: prerelease, supporto STS.
- AutoMapper 14 (MIT): CVE alta non patchata. Mapping manuale: va oltre lo stretto indispensabile (rimandato a un consiglio).
- `Database.Migrate()` all'avvio o servizio `migrate` nel compose: modifica al codice o complessità non necessaria.
- jQuery 4.0.0: possibili incompatibilità con jquery-validation-unobtrusive.

## Fasi implementative

### Fase 1: SDK e target framework
- [x] Installare `dotnet-sdk` e `aspnet-runtime` da `extra` (lasciare installati i pacchetti 5.0 fino a fine verifica)
- [x] `AcademyModel.csproj`, `AcademyEfPersistance.csproj`, `CodeAcademyWeb.csproj`: `net5.0` → `net10.0`

### Fase 2: Pacchetti NuGet
- [x] `AcademyModel.csproj`: NodaTime 3.3.7
- [x] `AcademyEfPersistance.csproj`: EF Core Tools 10.0.14, Npgsql EF e NodaTime 10.0.3
- [x] `CodeAcademyWeb.csproj`: EF Core Design 10.0.14; sostituire `AutoMapper.Extensions.Microsoft.DependencyInjection` con `AutoMapper` 16.2.0
- [x] `.config/dotnet-tools.json`: dotnet-ef 10.0.14
- [x] `CodeAcademyWeb/Startup.cs`: `services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());`
- [x] `dotnet build` della soluzione: correggere solo errori e warning bloccanti dovuti all'upgrade (nessun refactoring)

### Fase 3: Database e migration
- [x] Rinominare l'attuale `docker-compose.yml` in `docker-compose-postgres.yml` (solo Postgres): `postgres:18-alpine`, volume `pgdata:/var/lib/postgresql`
- [x] `docker compose down -v` e `docker compose -f docker-compose-postgres.yml up -d`
- [x] `dotnet ef migrations has-pending-model-changes -p AcademyPersistance -s CodeAcademyWeb`; se ci sono modifiche, `dotnet ef migrations add UpgradeEf10` e verificarne il contenuto
- [x] `dotnet ef database update -p AcademyPersistance -s CodeAcademyWeb`

### Fase 4: Front-end
- [x] Sostituire i file `dist` in `CodeAcademyWeb/wwwroot/lib/` (bootstrap, jquery, jquery-validation, jquery-validation-unobtrusive) con le versioni di destinazione, mantenendo percorsi e file `LICENSE`
- [x] `Views/Shared/_Layout.cshtml`: `data-toggle` → `data-bs-toggle`, `data-target` → `data-bs-target`

### Fase 5: Docker
- [x] `.dockerignore` in root (`**/bin`, `**/obj`, `.git`, `.vs`, `docs`)
- [x] `Dockerfile` in root multi-stage: `sdk:10.0` (restore dei csproj, poi `dotnet publish CodeAcademyWeb -c Release -o /app`) → `aspnet:10.0`, `EXPOSE 8080`, `ENTRYPOINT ["dotnet", "CodeAcademyWeb.dll"]`
- [x] Nuovo `docker-compose.yml`: servizio `postgres` identico a `docker-compose-postgres.yml` (porta 5432 esposta per le migration dall'host) + servizio `api` (`build: .`, porta `8080:8080`, `depends_on: postgres: condition: service_healthy`, env `ASPNETCORE_ENVIRONMENT=Development`, `ConnectionStrings__AcademyDb=Host=postgres;Port=5432;Database=AccademyDb;Username=academy;Password=academy`, `AUTOMAPPER_LICENSE_KEY=${AUTOMAPPER_LICENSE_KEY:-}`)

### Fase 6: README
- [x] Aggiornare `README.md` (skill `procedure`):
  - **Sviluppo locale**: prerequisiti .NET 10 da `extra` (rimozione dei pacchetti 5.0 e `openssl-1.1`), `docker compose -f docker-compose-postgres.yml up -d`, ricreazione del volume per PG 18, `dotnet tool restore`, migration, `dotnet dev-certs https`, `dotnet run`, nuova migration, arresto
  - **Build**: `docker compose up -d --build` (API su `http://localhost:8080`), migration dall'host, license key AutoMapper opzionale (`AUTOMAPPER_LICENSE_KEY`), arresto
  - Nota: usare un solo file compose alla volta

### Consigli registrati in pianificazione
- `docs/consigli/2026-11-04-sostituire-automapper.md`
- `docs/consigli/2026-11-04-data-odierna-errata-ricerca-edizioni.md`
- `docs/consigli/2026-11-04-filtri-ricerca-edizioni.md`

## Impatti

- **Codice**: i 3 csproj, `.config/dotnet-tools.json`, `CodeAcademyWeb/Startup.cs` (una riga), `Views/Shared/_Layout.cshtml`, `wwwroot/lib/*`. Eventuale nuova migration
- **Nuovi file**: `Dockerfile`, `.dockerignore`, `docker-compose-postgres.yml`
- **Configurazione**: `docker-compose-postgres.yml` (ex `docker-compose.yml`, PG 18), nuovo `docker-compose.yml` (PG 18 + `api`), `README.md`
- **API / Swagger**: nessuna modifica agli endpoint (Swagger assente)
- **Dati**: volume `pgdata` da ricreare (si perdono i dati locali di sviluppo); schema invariato salvo esito di `has-pending-model-changes`
- **Documentazione funzionale / tecnica**: nessuna (assente, non introdotta)

## Test

- Nessun test unitario (assenti, non introdotti). Verifica manuale:
  - `dotnet build` della soluzione senza errori
  - `dotnet ef migrations has-pending-model-changes` e `dotnet ef database update` su PG 18
  - `dotnet run --project CodeAcademyWeb --launch-profile CodeAcademyWeb`: GET `/api/Area/areas` e `/api/Course` rispondono; un POST su `/api/Course` scrive su DB
  - Ricerca lezioni in un intervallo (`LocalDateTime.Date`) e iscrizione duplicata: deve fallire per l'indice unico
  - `docker compose -f docker-compose-postgres.yml up -d`: solo Postgres avviato, app da `dotnet run` funzionante
  - `docker compose up -d --build`: container `postgres` e `api` avviati, `http://localhost:8080/api/Area/areas` risponde
  - Pagina Home: navbar e toggler mobile funzionanti con Bootstrap 5

## Criteri di accettazione

- [ ] Tutti i progetti su `net10.0`; nessun pacchetto con vulnerabilità note (`dotnet list package --vulnerable`)
- [ ] La soluzione compila con l'SDK 10, senza SDK 5 né `openssl-1.1`
- [ ] `dotnet ef database update` funziona su PostgreSQL 18
- [ ] L'app si avvia, legge e scrive sul database
- [ ] `docker compose -f docker-compose-postgres.yml up -d` avvia solo PostgreSQL 18 per lo sviluppo locale
- [ ] `docker compose up -d --build` builda e avvia API e PostgreSQL, API raggiungibile su `http://localhost:8080`
- [ ] Librerie front-end aggiornate e layout funzionante
- [ ] `README.md` con le sezioni Sviluppo locale e Build

## Note di implementazione

(Compilato da `/add-feature`.)
