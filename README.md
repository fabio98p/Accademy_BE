# Accademy_BE

Backend ASP.NET Core (.NET 10) dell'Accademy, con database PostgreSQL 18.

Si usa un solo file compose alla volta: `docker-compose-postgres.yml` per lo sviluppo locale, `docker-compose.yml` per la build con Docker. Entrambi usano lo stesso database (volume `pgdata`) e la porta `5432`.

## Prerequisiti (Arch / CachyOS)

Installare Docker e Docker Compose, poi aggiungere l'utente al gruppo `docker` (serve un nuovo login per renderlo attivo):

```bash
sudo usermod -aG docker <tuo-utente>
```

Installare l'SDK .NET 10 e il runtime ASP.NET Core dal repository `extra`:

```bash
sudo pacman -S dotnet-sdk aspnet-runtime aspnet-targeting-pack
```

## Sviluppo locale

Avviare PostgreSQL:

```bash
docker compose -f docker-compose-postgres.yml up -d
```

Il database è raggiungibile su `localhost:5432`:
- Database: `AccademyDb`
- Utente: `academy`
- Password: `academy`

Ripristinare i tool .NET del progetto (`dotnet-ef`):

```bash
dotnet tool restore
```

Creare o aggiornare lo schema del database:

```bash
dotnet ef database update -p AcademyPersistance -s CodeAcademyWeb
```

Generare il certificato HTTPS di sviluppo (solo la prima volta):

```bash
dotnet dev-certs https
```

Avviare l'applicazione:

```bash
dotnet run --project CodeAcademyWeb --launch-profile CodeAcademyWeb
```

L'applicazione risponde su `https://localhost:5001` e `http://localhost:5000`.

### Nuova migration

Dopo aver modificato le entità, creare la migration sostituendo `<NomeMigration>`:

```bash
dotnet ef migrations add <NomeMigration> -p AcademyPersistance -s CodeAcademyWeb
```

Applicarla al database:

```bash
dotnet ef database update -p AcademyPersistance -s CodeAcademyWeb
```

### Arresto

Fermare PostgreSQL (i dati restano nel volume `pgdata`):

```bash
docker compose -f docker-compose-postgres.yml down
```

## Build

Se è attivo il PostgreSQL dello sviluppo locale, fermarlo:

```bash
docker compose -f docker-compose-postgres.yml down
```

Buildare e avviare API e PostgreSQL:

```bash
docker compose up -d --build
```

Applicare le migration dall'host (solo la prima volta o dopo una nuova migration):

```bash
dotnet tool restore
```

```bash
dotnet ef database update -p AcademyPersistance -s CodeAcademyWeb
```

L'API risponde su `http://localhost:8080` (es. `http://localhost:8080/api/Area/areas`).

### Arresto

Fermare API e PostgreSQL (i dati restano nel volume `pgdata`):

```bash
docker compose down
```
