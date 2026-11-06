# Accademy_BE

ASP.NET Core (.NET 10) backend of the Accademy, with a PostgreSQL 18 database.

Only one compose file is used at a time: `docker-compose-postgres.yml` for local development, `docker-compose.yml` for the Docker build. Both use the same database (`pgdata` volume) and port `5432`.

## Documentation

- [Technical documentation](docs/technical/README.md): architecture, controllers, tests, decisions

## Prerequisites (Arch / CachyOS)

Install Docker and Docker Compose, then add your user to the `docker` group (a new login is needed to make it active):

```bash
sudo usermod -aG docker <your-user>
```

Install the .NET 10 SDK and the ASP.NET Core runtime from the `extra` repository:

```bash
sudo pacman -S dotnet-sdk aspnet-runtime aspnet-targeting-pack
```

## Local development

Start PostgreSQL:

```bash
docker compose -f docker-compose-postgres.yml up -d
```

The database is reachable at `localhost:5432`:
- Database: `AccademyDb`
- User: `academy`
- Password: `academy`

Restore the project's .NET tools (`dotnet-ef`):

```bash
dotnet tool restore
```

Create or update the database schema:

```bash
dotnet ef database update -p AcademyPersistance -s CodeAcademyWeb
```

Generate the HTTPS development certificate (first time only):

```bash
dotnet dev-certs https
```

Start the application:

```bash
dotnet run --project CodeAcademyWeb --launch-profile CodeAcademyWeb
```

The application listens on `https://localhost:5001` and `http://localhost:5000`.

Swagger is available at `https://localhost:5001/swagger`.

### New migration

After changing the entities, create the migration replacing `<MigrationName>`:

```bash
dotnet ef migrations add <MigrationName> -p AcademyPersistance -s CodeAcademyWeb
```

Apply it to the database:

```bash
dotnet ef database update -p AcademyPersistance -s CodeAcademyWeb
```

### Stop

Stop PostgreSQL (the data stays in the `pgdata` volume):

```bash
docker compose -f docker-compose-postgres.yml down
```

## Build

If the local development PostgreSQL is running, stop it:

```bash
docker compose -f docker-compose-postgres.yml down
```

Build and start the API and PostgreSQL:

```bash
docker compose up -d --build
```

Apply the migrations from the host (first time only or after a new migration):

```bash
dotnet tool restore
```

```bash
dotnet ef database update -p AcademyPersistance -s CodeAcademyWeb
```

The API listens on `http://localhost:8080` (e.g. `http://localhost:8080/api/Area/areas`).

Swagger is available at `http://localhost:8080/swagger`.

### Stop

Stop the API and PostgreSQL (the data stays in the `pgdata` volume):

```bash
docker compose down
```
