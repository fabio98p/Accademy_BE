# Swagger e rimozione front-end

- **Data**: 2026-11-05
- **Progetto**: Accademy_BE
- **Progetti correlati**: nessuno
- **Stato**: Completata

## Obiettivo

Documentare con Swagger/OpenAPI tutti gli endpoint esistenti del backend. Poi eliminare la parte front-end che non serve più (view, librerie statiche, pagina Home): la root `/` risponde solo con il testo `backend running`.

## Consigli risolti

Nessuno.

## Contesto

- `CodeAcademyWeb`: 7 controller API con route attribute (`Area`, `Course`, `Edition`, `Enrollment`, `Instructor`, `Lesson`, `Student`), 26 endpoint attivi, nessuna specifica OpenAPI.
- Parte front-end residua: `Controllers/HomeController.cs`, `Views/` (Home, Shared, `_ViewImports`, `_ViewStart`), `Models/ErrorViewModel.cs`, `wwwroot/` (Bootstrap, jQuery, jquery-validation, unobtrusive, `css/site.css`, `js/site.js`, `favicon.ico`). In `Startup.cs`: `AddControllersWithViews`, `UseStaticFiles`, `MapControllerRoute` con default `Home/Index`.
- I controller ereditano da `Controller` (classe per le view) senza usare funzioni delle view.
- Pacchetti verificati il 2026-11-05: `Microsoft.AspNetCore.OpenApi` 10.0.14 (25/10/2026, ~280M download, nessuna vulnerabilità nota) e `Swashbuckle.AspNetCore.SwaggerUI` 10.3.1 (19/10/2026, ~1,39 mld download; l'advisory GHSA-qrmm-w75w-3wpx riguarda solo le versioni < 6.3.0).
- Lacune: mancano `docs/functional`, `docs/technical` e i test unitari. Per scelta dell'utente non vengono introdotti in questa feature.

## Decisioni prese

| # | Decisione | Motivazione |
|---|-----------|-------------|
| 1 | Documento generato da `Microsoft.AspNetCore.OpenApi` 10.0.14, UI con `Swashbuckle.AspNetCore.SwaggerUI` 10.3.1 | Generatore ufficiale di .NET 10, meno dipendenze di Swashbuckle completo |
| 2 | Swagger (`/openapi/v1.json` e UI su `/swagger`) solo in Development | Il docker-compose usa già Development, quindi Swagger c'è sia con `dotnet run` sia con Docker |
| 3 | Summary e descrizioni in inglese, tramite commenti XML (`GenerateDocumentationFile`) e `[ProducesResponseType]` | Scelta dell'utente |
| 4 | Swagger descrive il comportamento reale; le incoerenze vanno in `docs/consigli` | Il contratto delle API non cambia |
| 5 | Controller da `Controller` a `ControllerBase` | Backend solo API, nessun legame con le view |
| 6 | Rimozione completa di `wwwroot`, view, `HomeController`, `ErrorViewModel` e `UseStaticFiles` | Scelta dell'utente |
| 7 | Root `GET /` → `text/plain` `backend running`, esclusa dallo Swagger | Requisito dell'utente |
| 8 | `launchBrowser: false` in tutti e due i profili di `launchSettings.json` | Scelta dell'utente |
| 9 | Nel README solo gli URL di Swagger | Scelta dell'utente |
| 10 | Nessuna documentazione funzionale o tecnica né test in questa feature | Scelta dell'utente |

## Soluzione scelta

Fase 1: Swagger su tutti gli endpoint. Fase 2: rimozione del front-end e risposta fissa sulla root.

Alternative scartate:
- Swashbuckle completo: è il generatore che Microsoft ha tolto dai template di .NET; aggiunge più dipendenze.
- File `openapi.yaml` statico: va allineato a mano al codice.
- Correggere le incoerenze degli endpoint: allarga il perimetro e cambia il contratto delle API.

## Fasi implementative

### Fase 1: Swagger
- [x] `CodeAcademyWeb.csproj`: aggiungere `Microsoft.AspNetCore.OpenApi` 10.0.14 e `Swashbuckle.AspNetCore.SwaggerUI` 10.3.1; `<GenerateDocumentationFile>true</GenerateDocumentationFile>` e `<NoWarn>$(NoWarn);1591</NoWarn>`
- [x] `Startup.cs`: `services.AddOpenApi()`. In `Configure`, solo se `env.IsDevelopment()`: `endpoints.MapOpenApi()` e `app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "v1"))`
- [x] In tutti i 7 controller, per ogni azione: commento XML (`summary`, `param`, `response`) in inglese e `[ProducesResponseType]` con i codici realmente restituiti e il tipo (DTO, `ErrorObject`, `string` per il 404 del PUT Edition)
- [x] Commenti XML anche sui DTO usati come body o risposta (`CodeAcademyWeb/DTOs/*`) e su `AcademyModel/Exceptions/ErrorObject.cs` se serve allo schema
- [x] Verificare che `/openapi/v1.json` contenga tutti i 26 endpoint con tag per controller

### Fase 2: Rimozione front-end
- [x] Eliminare `Controllers/HomeController.cs`, `Models/ErrorViewModel.cs`, `Views/` e `wwwroot/`
- [x] `Startup.cs`: `AddControllersWithViews` → `AddControllers`; rimuovere `UseStaticFiles`; `MapControllerRoute` (e il commento `//endpoints.MapControllers();`) → `endpoints.MapControllers()`; aggiungere `endpoints.MapGet("/", () => "backend running").ExcludeFromDescription()`
- [x] I 7 controller: `: Controller` → `: ControllerBase`
- [x] `Properties/launchSettings.json`: `launchBrowser: false` in tutti e due i profili
- [x] Librerie front-end eliminate con `wwwroot/lib`: Bootstrap 5.3.8, jQuery 3.7.1, jquery-validation 1.22.1, jquery-validation-unobtrusive 4.0.0, più `css/site.css` e `js/site.js`. Swagger UI non le usa (i suoi asset sono già nel pacchetto NuGet)
- [x] Controllo dipendenze superflue: nel csproj non devono restare riferimenti a view, Razor o contenuti statici, e non devono esistere `libman.json`, `package.json` o `bundleconfig.json`. I NuGet rimasti (AutoMapper, EF Core Design, OpenApi, SwaggerUI) sono tutti usati dal backend

### Fase 3: README
- [x] `README.md` (skill `procedure`): URL di Swagger, `http://localhost:5000/swagger` in Sviluppo locale e `http://localhost:8080/swagger` in Build

### Consigli registrati in pianificazione
- `2026-11-05-put-restituiscono-201.md`: `PUT` Course e Student restituiscono `201 Created` invece di `200 OK`
- `2026-11-05-location-header-errati.md`: `Created` con percorsi errati (`/api/courses/{id}` in Course, `/api/edition/{id}` in Enrollment)
- `2026-11-05-errore-404-put-edition-non-uniforme.md`: il `PUT` Edition restituisce 404 come stringa e non come `ErrorObject`
- `2026-11-05-get-by-id-senza-404.md`: `GET` Course e Student by id restituiscono 200 con body vuoto se l'entità non esiste
- `2026-11-05-delete-student-risposta-errata.md`: `DELETE` Student legge lo studente dopo averlo cancellato e restituisce l'entità, non il DTO

## Impatti

- **Codice**: `CodeAcademyWeb.csproj`, `Startup.cs`, 7 controller, DTO (solo commenti), `launchSettings.json`; eliminati `HomeController`, `ErrorViewModel`, `Views/`, `wwwroot/`
- **API / Swagger**: nuova specifica `/openapi/v1.json` e UI `/swagger` (solo Development); nuova root `GET /` in `text/plain`. Endpoint `/api/*` invariati
- **Dati**: nessuna modifica
- **Documentazione funzionale / tecnica**: nessuna (assenti, non introdotte)

## Test

- Nessun test unitario (assenti, non introdotti). Verifica manuale:
  - `dotnet build` della soluzione senza errori né warning nuovi
  - `dotnet run --project CodeAcademyWeb --launch-profile CodeAcademyWeb`: il browser non si apre; `curl http://localhost:5000/` risponde `backend running`
  - `http://localhost:5000/swagger` mostra i 7 gruppi e i 26 endpoint con summary, parametri, body e risposte; `/openapi/v1.json` valido
  - Una chiamata da Swagger UI (es. `GET /api/Area/areas`) funziona
  - Con `ASPNETCORE_ENVIRONMENT=Production`, `/swagger` e `/openapi/v1.json` rispondono 404
  - `docker compose up -d --build`: `http://localhost:8080/` e `http://localhost:8080/swagger` rispondono

## Criteri di accettazione

- [x] Tutti gli endpoint esistenti sono documentati in Swagger con codici di risposta corrispondenti al codice
- [x] Swagger disponibile solo in Development
- [x] La root risponde `backend running` e non compare nello Swagger
- [x] Nessuna view, libreria FE (Bootstrap, jQuery, jquery-validation, unobtrusive) o file statico nel progetto; nessuna dipendenza superflua
- [x] I controller ereditano da `ControllerBase`
- [x] Il README indica gli URL di Swagger
- [x] Consigli sulle incoerenze registrati

## Note di implementazione

- Gli endpoint attivi sono 26 (16 percorsi), non 21: il numero è stato corretto nel piano.
- `ErrorObject` non è stato commentato: `AcademyModel` non genera la documentazione XML e lo schema è comunque completo.
- Dopo l'eliminazione di `wwwroot` serve una build pulita (`dotnet clean`): il manifest degli static web assets rimasto in `bin/obj` faceva fallire l'avvio.
- In sviluppo `UseHttpsRedirection` reindirizza HTTP su HTTPS: nel README l'URL di Swagger locale è `https://localhost:5001/swagger` invece di `http://localhost:5000/swagger`.
- Dopo la code-review, su richiesta dell'utente, gli endpoint che non danno il risultato atteso hanno il summary con prefisso `BROKEN -` e una description con il problema (9 endpoint: `POST`/`PUT`/`GET {id}` Course, `PUT` Edition, `POST` Enrollment, `PUT`/`GET {id}`/`DELETE {id}`/`POST {idStudent}/enrollments` Student).
- Documentati i codici realmente restituiti: `GET` Course e Student by id restituiscono 204 (non 200 con body vuoto) se l'entità non esiste; `DELETE` Student restituisce sempre 204; l'iscrizione dello studente restituisce 500.
- La chiamata di prova da Swagger UI non è stata verificata con il database attivo (PostgreSQL non avviato durante l'implementazione).
