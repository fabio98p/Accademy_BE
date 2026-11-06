# Unit test, test HTTP e documentazione tecnica delle chiamate

- **Data**: 2026-11-05
- **Progetto**: Accademy_BE
- **Progetti correlati**: nessuno
- **Stato**: In corso

## Obiettivo

Coprire con test automatici e documentazione tecnica i 26 endpoint già documentati nello Swagger. Il codice delle API **non si modifica né si corregge**. I test verificano il comportamento corretto atteso, quindi quelli sugli endpoint `BROKEN` falliscono: è voluto. Niente documentazione funzionale e niente test e2e.

## Consigli risolti

Nessuno. I test degli endpoint `BROKEN` evidenziano i consigli esistenti senza risolverli: `put-restituiscono-201`, `location-header-errati`, `errore-404-put-edition-non-uniforme`, `get-by-id-senza-404`, `delete-student-risposta-errata`, `enrollmentsservice-mai-assegnato`, `creationdate-corso-non-valorizzata`.

## Contesto

- 7 controller in `CodeAcademyWeb/Controllers`, 26 endpoint, 9 con summary `BROKEN -` nello Swagger.
- Le chiamate passano per controller → AutoMapper (`CodeAcademyWeb/Profiles`) → servizi (`AcademyModel/Services/I*Service`, implementati in `AcademyPersistance/Services`) → repository EF → PostgreSQL.
- `Startup.cs` registra servizi e repository in DI e AutoMapper con `AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies())`.
- Lacune: nessun progetto di test, `docs/technical/` e `docs/functional/` assenti (la funzionale resta fuori per scelta dell'utente).
- Pacchetti verificati il 2026-11-05 su NuGet: `xunit.v3` 4.0.2 (12/09/2026, ~65M download), `xunit.runner.visualstudio` 4.0.1 (~1,17 mld), `Microsoft.NET.Test.Sdk` 18.11.0 (~2 mld), `NSubstitute` 6.2.0 (09/10/2026, ~334M), `coverlet.collector` 10.1.0 (~1,13 mld), `Microsoft.AspNetCore.Mvc.Testing` 10.0.14 (~441M, allineato agli altri pacchetti ASP.NET 10.0.14). `NullLoggerFactory` arriva già dal framework ASP.NET Core, senza pacchetti in più.

## Decisioni prese

| # | Decisione | Motivazione |
|---|-----------|-------------|
| 1 | Stack: xUnit v3 + NSubstitute, assert nativi di xUnit | Scelta dell'utente; FluentAssertions 8 ha licenza commerciale |
| 2 | **Unit test** del controller: servizi mockati, `IMapper` reale costruito dai Profile del progetto con `NullLoggerFactory` | Il contratto della chiamata è deciso da controller e mapping; un mapper mockato renderebbe i test tautologici |
| 3 | **Test HTTP** in-memory con `WebApplicationFactory<Startup>`: servizi sostituiti in DI con mock NSubstitute, nessun DB | Coprono routing, model binding, status code reali, JSON, `Location`, validazione `[ApiController]` |
| 4 | Servizi e repository EF esclusi | Per testarli servono test d'integrazione con un DB vero; InMemory/SQLite non replicano Postgres e NodaTime |
| 5 | Un solo progetto `CodeAcademyWeb.Tests/` in root, aggiunto a `CodeAcademy.sln`; cartelle `Controllers/` (unit) e `Http/` (HTTP); `[Trait("Category", "Unit" \| "Http")]` | Struttura allineata agli altri progetti; le categorie si lanciano separatamente |
| 6 | Naming dei test `Metodo_Condizione_RisultatoAtteso`, schema Arrange / Act / Assert | Convenzione della skill `unit-tests` |
| 7 | Gli endpoint non `BROKEN` si testano come documentati nello Swagger (es. GET Edition by id 404 senza `ErrorObject`, DELETE Enrollment 200 senza body) | Lo Swagger descrive il comportamento reale |
| 8 | Contratto atteso per gli endpoint `BROKEN` (vedi tabella sotto) | Deciso dall'utente endpoint per endpoint |
| 9 | DELETE per id con id inesistente (Course, Student, Enrollment): il mock simula il comportamento reale (`GetById` → `null`, `Delete` → `ArgumentNullException`); atteso 404 con `ErrorObject` | Scelta dell'utente; oggi rispondono 500 |
| 10 | Iscrizione studente: il controller si costruisce con il costruttore attuale, senza iniettare `IEnrollmentsService` (niente reflection sul campo privato); atteso 201, il test fallisce con `NullReferenceException` | Il progetto di test deve compilare senza toccare il codice |
| 11 | Documentazione tecnica: `docs/technical/README.md`, `architecture.md`, `decisions.md` e un file per controller in `docs/technical/modules/` | Scelta dell'utente; il contratto resta nello Swagger e non va duplicato |
| 12 | Fase 0 di setup, poi una fase per controller dal più semplice: Area, Instructor, Lesson, Enrollment, Edition, Course, Student | Scelta dell'utente |
| 13 | Swagger e codice di `CodeAcademyWeb`, `AcademyModel`, `AcademyPersistance` invariati | Requisito dell'utente |

### Contratto atteso per gli endpoint `BROKEN`

| Endpoint | Oggi | Atteso dai test |
|----------|------|-----------------|
| `GET /api/Course/{id}` inesistente | 200 `null` (HTTP 204) | 404 con `ErrorObject` |
| `GET /api/Student/{id}` inesistente | 200 `null` (HTTP 204) | 404 con `ErrorObject` |
| `POST /api/Course` | `CreationDate` 0001-01-01, `Location` `/api/courses/{id}` | `CreationDate` sempre la data odierna (anche se inviata dal client), `Location` `/api/Course/{id}` |
| `PUT /api/Course` | 201 Created | 200 OK con `CourseDTO` aggiornato |
| `PUT /api/Student` | 201 Created | 200 OK con `StudentDTO` aggiornato |
| `PUT /api/Edition` edizione inesistente | 404 con stringa | 404 con `ErrorObject` |
| `POST /api/Enrollment` | `Location` `/api/edition/{id}` | `Location` `/api/Enrollment/{id}` |
| `DELETE /api/Student/{id}` | rilegge dopo la delete, mappa su `Student` (HTTP 204) | 200 con `StudentDTO` dello studente letto prima della cancellazione |
| `POST /api/Student/{idStudent}/enrollments` valido | 500 (`NullReferenceException`) | 201 con `EnrollmentDTO`, `Location` `/api/student/{idStudent}/enrollments/{enrId}` |
| DELETE per id inesistente (Course, Student, Enrollment) | 500 (`ArgumentNullException`) | 404 con `ErrorObject` |

## Soluzione scelta

Per ogni controller ci sono due livelli di test sugli stessi endpoint:
- **Unit**: si chiama direttamente il metodo del controller e si verificano il tipo di `IActionResult`, lo status, il DTO mappato, la `Location`, le chiamate al servizio (`Received`) e la gestione delle eccezioni.
- **HTTP**: `HttpClient` della `WebApplicationFactory` e verifica di status reale, header, JSON deserializzato, binding di route e body.

Con i test arriva la documentazione tecnica del controller.

Alternative scartate:
- `IMapper` mockato: i test verificherebbero solo i valori impostati nel mock.
- Test dei servizi EF con InMemory/SQLite: provider diverso da Postgres, risultati non affidabili.
- Due progetti di test separati: duplica configurazione e dipendenze senza vantaggi.
- Iniettare `IEnrollmentsService` via reflection: il test passerebbe e nasconderebbe il bug.

## Fasi implementative

### Fase 0: Setup
- [x] `CodeAcademyWeb.Tests/CodeAcademyWeb.Tests.csproj` (net10.0): `xunit.v3` 4.0.2, `xunit.runner.visualstudio` 4.0.1, `Microsoft.NET.Test.Sdk` 18.11.0, `NSubstitute` 6.2.0, `coverlet.collector` 10.1.0, `Microsoft.AspNetCore.Mvc.Testing` 10.0.14; `ProjectReference` a `CodeAcademyWeb`. Ricontrollare in implementazione le advisory sulle versioni
- [x] Aggiungere il progetto a `CodeAcademy.sln` (`dotnet sln add`)
- [x] `Support/MapperFactory.cs`: `IMapper` reale da `MapperConfiguration` con i Profile dell'assembly `CodeAcademyWeb` e `NullLoggerFactory.Instance`
- [x] `Support/ApiFactory.cs`: `WebApplicationFactory<Startup>` che in `ConfigureTestServices` sostituisce `IAreasService`, `ICoursesService`, `IEditionsService`, `IEnrollmentsService`, `ILessonsService`, `IPeopleService` con mock NSubstitute esposti come proprietà. Nessuna connessione al DB (il `DbContext` resta registrato ma non viene risolto)
- [x] Verificare che `dotnet build` e `dotnet test` girino con zero test
- [x] `docs/technical/README.md` (skill `tech-docs`): indice, stack, struttura dei test, comandi `dotnet test` (tutti / `--filter Category=Unit` / `--filter Category=Http`), nota sui test che falliscono di proposito
- [x] `docs/technical/architecture.md`: progetti e dipendenze (`AcademyModel` ← `AcademyPersistance` ← `CodeAcademyWeb`), DI in `Startup.cs`, flusso di una richiesta (diagramma Mermaid), gestione errori (`EntityNotFoundException` → `ErrorObject`), livelli di test
- [x] `docs/technical/decisions.md`: decisioni di questo piano con link al file in `docs/func-history/`
- [x] `README.md` root: link a `docs/technical/README.md` (manca `docs/functional`, da segnalare come lacuna)

### Pattern di ogni fase controller (Fasi 1–7)
- [ ] `CodeAcademyWeb.Tests/Controllers/<Nome>ControllerTests.cs`: per ogni endpoint, caso nominale, liste vuote dove ha senso, casi di errore (eccezioni del servizio, id inesistente) e, per gli endpoint `BROKEN`, il contratto atteso della tabella
- [ ] `CodeAcademyWeb.Tests/Http/<Nome>HttpTests.cs`: per ogni endpoint, route, status reale, `Location`, JSON camelCase, binding; stesso contratto atteso
- [ ] `docs/technical/modules/<nome>-controller.md`: per ogni endpoint servizio chiamato, Profile e mapping usati (campi calcolati o rinominati), flusso, errori gestiti, problemi noti (link ai consigli), test che lo coprono ed esito atteso (verde / rosso voluto). Link allo Swagger per il contratto, senza duplicarlo
- [ ] Eseguire `dotnet test` e annotare negli appunti del piano i test che falliscono, verificando che siano solo quelli attesi

### Fase 1: AreaController
- [x] `GET /api/Area/areas`: lista mappata su `AreaDTO`, lista vuota

### Fase 2: InstructorController
- [x] `GET /api/Instructor`: lista mappata su `InstructorDTO`, lista vuota

### Fase 3: LessonController
- [ ] `GET /api/Lesson/lessonByEdition/{id}`: id passato al servizio, mapping `LessonDTO`, lista vuota

### Fase 4: EnrollmentController
- [ ] `GET studentSubscribed/{id}`: mapping `EnrollmentDTO` (campi `CourseEdition*`, `CourseEditionStartDate`)
- [ ] `POST`: 201 con `Location` `/api/Enrollment/{id}` (rosso), `EntityNotFoundException` → 400 `ErrorObject`
- [ ] `DELETE {id}`: 200 senza body; id inesistente → 404 `ErrorObject` (rosso)

### Fase 5: EditionController
- [ ] `GET`: mapping `EditionDTO`
- [ ] `GET {id}`: `EditionDetailsDTO` (date `yyyy-MM-dd`, `InstructorFullName`); inesistente → 404 senza `ErrorObject` (come lo Swagger)
- [ ] `POST`: 201 con `Location` `/api/edition/{id}`; `EntityNotFoundException` → 400 `ErrorObject`
- [ ] `PUT`: 200; course/instructor inesistente → 400 `ErrorObject`; edizione inesistente → 404 `ErrorObject` (rosso)
- [ ] `DELETE {id}`: 204; inesistente → 404 `ErrorObject`
- [ ] `GET course/{id}` e `GET studentAvailable/{id}`: id passato al servizio, mapping `EditionDetailsDTO`

### Fase 6: CourseController
- [ ] `GET`, `GET lastCourse/{n}`: mapping `CourseDTO` (`CreationDate`, `AreaName`), `n` passato al servizio
- [ ] `GET {id}`: nominale; inesistente → 404 `ErrorObject` (rosso)
- [ ] `POST`: 201, `Location` `/api/Course/{id}` (rosso), `CreationDate` odierna con o senza valore dal client (rosso)
- [ ] `PUT`: 200 con DTO (rosso)
- [ ] `DELETE` con body: 200 con DTO, `DeleteCourse(Course)` chiamato
- [ ] `DELETE {id}`: 200 con DTO; inesistente → 404 `ErrorObject` (rosso)

### Fase 7: StudentController
- [ ] `GET`: mapping `StudentDTO` (`Surname`↔`Lastname`, `DateOfBirth`)
- [ ] `POST`: 201, `Location` `/api/student/{id}`
- [ ] `PUT`: 200 con DTO (rosso)
- [ ] `GET {id}`: nominale; inesistente → 404 `ErrorObject` (rosso)
- [ ] `POST {idStudent}/enrollments`: id URL ≠ body → 400 `ErrorObject`; valido → 201 con `EnrollmentDTO` e `Location` (rosso, `NullReferenceException` / 500)
- [ ] `DELETE {id}`: 200 con `StudentDTO` letto prima della cancellazione (rosso); inesistente → 404 `ErrorObject` (rosso)

### Consigli registrati in pianificazione
- `2026-11-05-filtri-ricerca-edizioni-senza-test.md`: `EFEditionRepository.Search` non ha unit test perché i filtri dipendono da `DbContext` e dalla data di sistema. Si testerà quando verrà creato un endpoint di ricerca
- `2026-11-05-delete-id-inesistente-500.md`: DELETE per id inesistente (Course, Student, Enrollment) → `Remove(null)` in `EFCrudRepository.Delete(K id)` → 500
- `2026-11-05-404-edition-get-non-uniforme.md`: `GET /api/Edition/{id}` restituisce 404 senza `ErrorObject`, a differenza degli altri errori

## Impatti

- **Codice**: nuovo progetto `CodeAcademyWeb.Tests/` e voce in `CodeAcademy.sln`. Nessuna modifica al codice esistente
- **API / Swagger**: nessuna modifica
- **Dati**: nessuna modifica
- **Documentazione funzionale**: nessuna (esclusa dall'utente; resta lacuna)
- **Documentazione tecnica**: nuova `docs/technical/` (`README.md`, `architecture.md`, `decisions.md`, `modules/<nome>-controller.md` ×7); link nel `README.md` root

## Test

- Unit e HTTP per tutti i 26 endpoint, con casi nominali, liste vuote, errori e id inesistenti
- Rossi attesi: tutti i casi segnati "(rosso)" nelle fasi, su entrambi i livelli

### Verifica

1. `dotnet build CodeAcademy.sln` senza errori
2. `dotnet test` senza PostgreSQL avviato: nessun test dipende dal DB
3. `dotnet test --filter Category=Unit` e `--filter Category=Http` eseguono solo il livello scelto
4. I test falliti sono solo quelli segnati come rossi nel piano
5. `git diff` non tocca `CodeAcademyWeb/`, `AcademyModel/`, `AcademyPersistance/`

## Criteri di accettazione

- [ ] Progetto di test nella soluzione, eseguibile senza DB
- [ ] Ognuno dei 26 endpoint ha unit test e test HTTP
- [ ] I test falliscono solo sui comportamenti `BROKEN` e sui DELETE con id inesistente
- [ ] `docs/technical/` con README, architecture, decisions e un file per controller
- [ ] README root con link alla documentazione tecnica
- [ ] Codice delle API e Swagger invariati
- [ ] Consigli nuovi registrati

## Note di implementazione

(Compilato da `/add-feature`.)
