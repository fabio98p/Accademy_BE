# Accademy_BE

Backend ASP.NET Core (.NET 5) dell'Accademy, con database PostgreSQL.

## Prerequisiti (Arch / CachyOS)

Installare Docker e Docker Compose, poi aggiungere l'utente al gruppo `docker` (serve un nuovo login per renderlo attivo):

```bash
sudo usermod -aG docker <tuo-utente>
```

Installare l'SDK .NET 5 da AUR:

```bash
paru -S dotnet-sdk-5.0-bin
```

Installare il runtime ASP.NET Core 5 da AUR:

```bash
paru -S aspnet-runtime-5.0-bin
```

Installare OpenSSL 1.1 da AUR (richiesto da .NET 5):

```bash
paru -S openssl-1.1
```

## Avvio in sviluppo

Avviare PostgreSQL:

```bash
docker compose up -d
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

## Nuova migration

Dopo aver modificato le entità, creare la migration sostituendo `<NomeMigration>`:

```bash
dotnet ef migrations add <NomeMigration> -p AcademyPersistance -s CodeAcademyWeb
```

Applicarla al database:

```bash
dotnet ef database update -p AcademyPersistance -s CodeAcademyWeb
```

## Arresto

Fermare PostgreSQL (i dati restano nel volume `pgdata`):

```bash
docker compose down
```
